using System.Reflection;
using Studio.Support.Extensions;

namespace Studio.Foundation;

public class Router
{
    public Application App;
    public List<Route> Routes { get; set; }
    
    private string _prefix { get; set; } = "";
    
    public Router(Application app)
    {
        App = app;
        if (Routes == null)
            Routes = new List<Route>();
    }
    
    public Router Prefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            throw new ArgumentNullException(nameof(prefix));
        
        if (!prefix.EndsWith("/"))
            prefix = prefix.Append("/");

        if (!prefix.StartsWith("/"))
            prefix = prefix.Prepend("/");
        
        this._prefix = prefix;
        return this;
    }
    
    public Route Get(string path, Type controller, string method)
    {
        if (!string.IsNullOrEmpty(this._prefix))
        {
            if (path.StartsWith("/"))
                path = path.Remove(0, 1);
                
            path = this._prefix.Append(path);   
        }
        else
        {
            path = path.StartsWith("/") ? path : path.Prepend("/");
        }
        
        Route route = new Route(path, controller, method, HttpMethod.Get);
        route.App = this.App.Resolve<Application>();
        Routes.Add(route);
        
        return route;
    }
    
    public Route Get<T>(string path, T controller, string method) where T : new()
    {
        if (controller is null)
            throw new ArgumentNullException(nameof(controller));
        
        return this.Get(path, controller.GetType(), method);
    }

    public void Group(Type type)
    {
        // create an instance of the type
        var instance = Activator.CreateInstance(type);

        // get all fields from the type
        var fields = type.GetFields();

        foreach (var field in fields)
            field.SetValue(instance, App.Resolve(field.FieldType));

        MethodInfo? registerMethod = type.GetMethod("Register");
        var parameters = App.ResolveMultiple(registerMethod?.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        registerMethod?.Invoke(instance, parameters);
        
        this._prefix = "";
    }
}