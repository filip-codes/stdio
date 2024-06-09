using System.Net;
using Studio.Http;

namespace Studio.Foundation;

public class Middleware
{
    public Application App;
    
    public void Redirect(string path)
    {
        Response response = this.App.Resolve<Response>();
        response.Redirect(path);
    }

    public void Redirect(Route route)
    {
        Response response = this.App.Resolve<Response>();
        response.Redirect(route.Path);
    }

    public Route Route(string name)
    {
        Route? route = this.App?.Resolve<Router>()?.Routes?.FirstOrDefault(route => route.GetName() == name);

        if (route is null)
            throw new ArgumentNullException(nameof(route));

        return route;
    }
}