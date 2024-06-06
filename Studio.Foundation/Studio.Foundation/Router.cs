using System.Collections.ObjectModel;
using System.Reflection;

namespace Studio.Foundation;

public class Router
{
    public Application App;
    public List<Route> Routes { get; set; }
    
    public Router(Application app)
    {
        App = app;
        if (Routes == null)
            Routes = new List<Route>();
    }
    
    public Route Get(string path, Type controller, string method)
    {
        Route route = new Route(path, controller, method, HttpMethod.Get);
        
        Routes.Add(route);
        
        return route;
    }

    public void Group(Type type)
    {
        // create an instance of the type
        var instance = Activator.CreateInstance(type);

        // get all fields from the type
        var fields = type.GetFields();

        foreach (var field in fields)
        {
            field.SetValue(instance, App.Resolve(field.FieldType));
        }

        MethodInfo? registerMethod = type.GetMethod("Register");
        registerMethod?.Invoke(instance, null);
    }
}