using System.Collections.ObjectModel;
using System.Net;
using System.Reflection;
using System.Text;
using Studio.Exceptions;
using Studio.Http;
using Studio.Support;
using Studio.Support.Extensions;

namespace Studio.Foundation;

public class Application : Container
{
    public string Name { get; set; } = "Studio";
    private static readonly HttpListener Listener = new();
    private readonly string _uri;
    // private RouteCollection _routes { get; set; }
    
    // bad practice, response sollte nicht in der klasse gehalten werden
    private HttpListenerResponse _response { get; set; }
    
    private IEnumerable<Type> ConfigTypes { get; set; }
    
    private static readonly Assembly? Assembly = Assembly.GetEntryAssembly();
    private static string _assemblyName = Assembly?.GetName().Name ?? string.Empty;
    private static readonly string ConfigAssembly = _assemblyName.Append(".Config");
    
    public Application()
    {
        this.InitEnvironment();
        
        this.Register(this);
        var router = new Router(this);
        this.Register(router);
        
        this.ConfigTypes = Assembly?.GetTypes().Where(type => type.Namespace == ConfigAssembly) ?? new List<Type>();
        this.RegisterConfigurations();
        
        this._uri = Env.Get("APP_URL");
        
        Listener.Prefixes.Add(this._uri);
    }

    protected void InitEnvironment()
    {
        Env.LoadFrom(".env");
    }

    private void RegisterConfigurations()
    {
        this.RegisterAppConfiguration(this.GetConfigType("App"));
    }

    private Type? GetConfigType(string name)
    {
        return ConfigTypes.FirstOrDefault(type => type.Name == name);
    }

    private void RegisterAppConfiguration(Type? type)
    {
        if (type is null)
            throw new ConfigurationNotFoundException("App");

        object? configuration = Activator.CreateInstance(type);
        
        PropertyInfo? urlInfo = configuration?.GetType().GetProperty("Url");
        string url = urlInfo?.GetValue(configuration)?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(url))
            throw new InvalidOperationException("Url not found in App configuration");
        
        PropertyInfo? providersProperty = configuration?.GetType().GetProperty("Providers");
        
        if (providersProperty is null)
            throw new InvalidOperationException("Providers property not found in App configuration");

        switch (providersProperty.PropertyType)
        {
            case { } t when t == typeof(List<string>):
                IEnumerable<string> providerStrings = providersProperty.GetValue(configuration) as List<string> ?? new();

                foreach (string provider in providerStrings)
                {
                    // get type for provider
                    Type? providerType = Type.GetType(provider + ", " + Assembly.GetEntryAssembly()?.GetName().Name);
                    if (providerType is null)
                        throw new InvalidOperationException($"Provider not found: {provider}");
                    
                    this.Register(() => providerType);
                }
                
                break;
            case { } t when t == typeof(List<Type>):
                IEnumerable<Type> providerTypes = providersProperty.GetValue(configuration) as List<Type> ?? new();

                foreach (Type provider in providerTypes)
                {
                    this.Register(() => provider);
                }
                
                foreach (Type provider in providerTypes)
                {
                    ConstructorInfo? constructor = provider.GetConstructor(Type.EmptyTypes);
                    object? instance = constructor?.Invoke(new object[] {});

                    // get all fields of the provider
                    FieldInfo[] fields = provider.GetFields();
                    foreach (FieldInfo field in fields)
                    {
                        // set the field value
                        field.SetValue(instance, this.Resolve(field.FieldType));
                    }
                    
                    MethodInfo? method = provider.GetMethod("Boot");
                    var parameters = base.ResolveMultiple(method?.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
                    method?.Invoke(instance, parameters);
                }
                
                break;
            default:
                throw new InvalidOperationException("Invalid type for Providers in App configuration.");
                break;
        }
    }

    public void Run()
    {
        try
        {
            Listener.Start();
            Console.WriteLine($"Listening for connections on {this._uri}");
        }
        catch (HttpListenerException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return;
        }

        Task.Run(async () =>
        {
            while (true)
            {
                var context = await Listener.GetContextAsync();
                Task.Run(() => this.ProcessRequest(context));
            }
        }).Wait();
    }

    public string? ExecuteRoute(string path)
    {
        Route? route = (this.Resolve(typeof(Router)) as Router)?.Routes.Where(route => route.Path == path)?.FirstOrDefault();

        if (route is null)
            throw new InvalidOperationException("Route not found");
        
        Type controllerType = route.Controller;
        ConstructorInfo? constructor = controllerType?.GetConstructor(Type.EmptyTypes);
        object? controller = constructor?.Invoke(new object[] {});
        
        MethodInfo? methodInfo = controllerType?.GetMethod(route.Method);
        var parameters = base.ResolveMultiple(methodInfo.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        object? magicValue = methodInfo?.Invoke(controller, parameters);

        return magicValue?.ToString();
    }

    private async void Abort(string message, int statusCode)
    {
        // ToDo: Get the right path for Views without hardcoding it
        string? content = File.ReadAllText($"Studio.Foundation/Studio.Foundation/Views/Errors/{statusCode}.cshtml").ReplaceLineEndings().Replace("{{ message }}", message);
        byte[] buffer = Encoding.UTF8.GetBytes(content);
        this._response.ContentLength64 = buffer?.Length ?? 0;
        this._response.StatusCode = statusCode;
        this._response.ContentType = "text/html";

        await this._response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        this._response.OutputStream.Close();
    }
    
    private async Task ProcessRequest(HttpListenerContext context)
    {
        this.Register(() => new Request(context.Request));
        this._response = context.Response;

        List<Route> routes = this.Resolve<Router>().Routes;
        
        Route? route = routes.FirstOrDefault(route => route.Path == context.Request.Url?.AbsolutePath);

        byte[] buffer = new byte[0];

        // accept text/html and application/json content types
        if (route is not null && context.Request.AcceptTypes != null && context.Request.AcceptTypes.Any() && context.Request.AcceptTypes.Contains("text/html"))
        {
            switch (context.Request.HttpMethod)
            {
                case "GET":
                    if (route.HttpMethod != HttpMethod.Get)
                        Abort("Method not allowed", 405);
        
                    break;
                case "POST":
                    if (route.HttpMethod != HttpMethod.Post)
                        Abort("Method not allowed", 405);

                    break;
                case "PUT":
                    if (route.HttpMethod != HttpMethod.Put)
                        Abort("Method not allowed", 405);

                    break;
                case "DELETE":
                    if (route.HttpMethod != HttpMethod.Delete)
                        Abort("Method not allowed", 405);

                    break;
                case "PATCH":
                    if (route.HttpMethod != HttpMethod.Patch)
                        Abort("Method not allowed", 405);

                    break;
            }

            string? content = this.ExecuteRoute(route.Path);

            if (content is null)
                throw new InvalidOperationException($"{route.Controller.ToString() + "." + route.Method} returned null");
        
            buffer = Encoding.UTF8.GetBytes(content);
        }
        else
        {
            buffer = Encoding.UTF8.GetBytes(string.Empty);
            Abort("404 - Not Found", 404);
        }

        this._response.ContentLength64 = buffer?.Length ?? 0;
        this._response.StatusCode = 200;
        this._response.ContentType = "text/html";

        await this._response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        this._response.OutputStream.Close();
    }
}