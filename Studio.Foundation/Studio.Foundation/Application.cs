using System.Net;
using System.Reflection;
using System.Text;
using Studio.Support;

namespace Studio.Foundation;

public class Application
{
    private readonly Container _container = new();
    private readonly HttpListener _listener = new();
    private readonly string _uri;
    private List<Route> _routes { get; set; }
    
    public Application()
    {
        DotEnv.LoadFrom(".env");
        this._uri = Environment.GetEnvironmentVariable("APP_URL")!;
        this._routes = new();
        
        this._listener.Prefixes.Add(this._uri);
    }
    
    public void Run()
    {
        this._listener.Start();
        Console.WriteLine($"Listening for connections on {this._uri}");
        
        while (true)
        {
            var context = this._listener.GetContext();
            Task.Run(() => this.ProcessRequest(context));
        }
    }
    
    public void Get(string path, string controller, string method)
    {
        this.Get(path, Type.GetType(controller + ", " + Assembly.GetEntryAssembly()?.GetName().Name), method);
    }
    
    public void Get(string path, Type? controller, string method)
    {
        if (controller is null)
            throw new InvalidOperationException("Controller not found");
        
        Route route = new Route(path, controller, method, HttpMethod.Get);
        this._routes.Add(route);
    }

    public string? ExecuteRoute(string path)
    {
        Route? route = this._routes.Where(route => route.Path == path)?.FirstOrDefault();

        if (route is null)
            throw new InvalidOperationException("Route not found");
        
        Type controllerType = route.Controller;
        ConstructorInfo? constructor = controllerType?.GetConstructor(Type.EmptyTypes);
        object? controller = constructor?.Invoke(new object[] {});
        
        MethodInfo? methodInfo = controllerType?.GetMethod(route.Method);
        var parameters = this._container.ResolveMultiple(methodInfo.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        object? magicValue = methodInfo?.Invoke(controller, parameters);

        return magicValue?.ToString();
    }

    private async Task ProcessRequest(HttpListenerContext context)
    {
        _container.Register(() => new Request(context.Request));

        Route? route = this._routes.FirstOrDefault(route => route.Path == context.Request.Url?.AbsolutePath);

        byte[] buffer = new byte[0];

        // accept text/html and application/json content types
        if (route is not null && context.Request.AcceptTypes != null && context.Request.AcceptTypes.Any() && context.Request.AcceptTypes.Contains("text/html"))
        {
            switch (context.Request.HttpMethod)
            {
                case "GET":
                    if (route.HttpMethod != HttpMethod.Get)
                        throw new InvalidOperationException("Method not allowed");
        
                    break;
                case "POST":
                    if (route.HttpMethod != HttpMethod.Post)
                        throw new InvalidOperationException("Method not allowed");

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
        }

        HttpListenerResponse response = context.Response;

        response.ContentLength64 = buffer?.Length ?? 0;
        response.StatusCode = 200;
        response.ContentType = "text/html";

        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
}