using System.Net;
using System.Reflection;
using System.Text;
using Studio.Support;

namespace Studio.Foundation;

public class Application : Container
{
    private readonly HttpListener _listener = new();
    private readonly string _uri;
    private List<Route> _routes { get; set; }
    private HttpListenerResponse _response { get; set; }
    
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
    
    public void Post(string path, string controller, string method)
    {
        this.Post(path, Type.GetType(controller + ", " + Assembly.GetEntryAssembly()?.GetName().Name), method);
    }

    public void Post(string path, Type? controller, string method)
    {
        if (controller is null)
            throw new InvalidOperationException("Controller not found");

        Route route = new Route(path, controller, method, HttpMethod.Post);
        this._routes.Add(route);
    }

    public void Patch(string path, string controller, string method)
    {
        this.Patch(path, Type.GetType(controller + ", " + Assembly.GetEntryAssembly()?.GetName().Name), method);
    }

    public void Patch(string path, Type? controller, string method)
    {
        if (controller is null)
            throw new InvalidOperationException("Controller not found");

        Route route = new Route(path, controller, method, HttpMethod.Patch);
        this._routes.Add(route);
    }
    
    public void Put(string path, string controller, string method)
    {
        this.Put(path, Type.GetType(controller + ", " + Assembly.GetEntryAssembly()?.GetName().Name), method);
    }
    
    public void Put(string path, Type? controller, string method)
    {
        if (controller is null)
            throw new InvalidOperationException("Controller not found");
        
        Route route = new Route(path, controller, method, HttpMethod.Put);
        this._routes.Add(route);
    }
    
    public void Delete(string path, string controller, string method)
    {
        this.Delete(path, Type.GetType(controller + ", " + Assembly.GetEntryAssembly()?.GetName().Name), method);
    }
    
    public void Delete(string path, Type? controller, string method)
    {
        if (controller is null)
            throw new InvalidOperationException("Controller not found");
        
        Route route = new Route(path, controller, method, HttpMethod.Delete);
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
        base.Register(() => new Request(context.Request));
        this._response = context.Response;

        Route? route = this._routes.FirstOrDefault(route => route.Path == context.Request.Url?.AbsolutePath);

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