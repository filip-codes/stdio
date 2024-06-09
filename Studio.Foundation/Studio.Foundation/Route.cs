using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Studio.Foundation;

public class Route
{
    public Application App;
    public string Path { get; set; }
    public Type Controller { get; set; }
    public string Method { get; set; }
    public HttpMethod HttpMethod { get; set; }
    private string _name { get; set; }

    protected List<Type> Middlewares { get; set; } = new();

    public Route(Application app)
    {
        this.App = app;
    }
    
    public Route(string path, Type controller, string method, HttpMethod httpMethod)
    {
        this.Path = path;
        this.Controller = controller;
        this.Method = method;
        this.HttpMethod = httpMethod;
    }

    public Route Middleware(params Type[] middleware)
    {
        Middlewares.AddRange(middleware);
        
        return this;
    }

    public Route Name(string name)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));

        this._name = name;
        
        return this;
    }
    
    public string GetName()
    {
        return this._name;
    }
    
    /// <summary>
    /// Run every registered middleware for the route.
    /// </summary>
    public void RunMiddlewares(Application app)
    {
        foreach (var middleware in Middlewares)
        {
            var instance = Activator.CreateInstance(middleware);
            
            foreach (var field in middleware.GetFields())
                field.SetValue(instance, this.App.Resolve(field.FieldType));

            MethodInfo? handle = middleware.GetMethod("Handle");
            var parameters = app.ResolveMultiple(handle?.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
            handle?.Invoke(instance, parameters);
        }
    }
}