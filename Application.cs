using System.Net;
using System.Reflection;
using System.Text;

namespace Studio;

public class Application
{
    private readonly Container _container = new();

    private readonly HttpListener _listener = new();

    private readonly string _uri;

    private List<Route> _routes { get; set; }
    
    public Application(string uri)
    {
        this._uri = uri;
        this._routes = new();
        
        this._listener.Prefixes.Add(uri);
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
        this.Get(path, Type.GetType(controller), method);
    }
    
    public void Get(string path, Type? controller, string method)
    {
        if (controller is null)
            throw new InvalidOperationException("Controller not found");
        
        Route route = new Route(path, controller, method);
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
        object? magicValue = methodInfo?.Invoke(controller, new object[] {});

        return magicValue?.ToString();
    }
    
    protected string View(string filename)
    {
        return File.ReadAllText($"Views/{filename}.cshtml").ReplaceLineEndings();
    }
    
    private async Task ProcessRequest(HttpListenerContext context)
    {
        _container.Register(() => new Request(context.Request));

        Route? route = this._routes.FirstOrDefault(route => route.Path == context.Request.Url.AbsolutePath);
        
        if (route is null)
            throw new Exception($"Route not found for {context.Request.Url?.AbsolutePath}");

        string? content = this.ExecuteRoute(route.Path);

        if (content is null)
            throw new InvalidOperationException($"{route.Controller.ToString() + "." + route.Method} returned null");
        
        byte[] buffer = Encoding.UTF8.GetBytes(content);

        HttpListenerResponse response = context.Response;
        
        response.ContentLength64 = buffer.Length;
        response.StatusCode = 200;
        response.ContentType = "text/html";

        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
}