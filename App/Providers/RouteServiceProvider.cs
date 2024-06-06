using Studio.Routes;

namespace Studio.App.Providers;

using Foundation;

public class RouteServiceProvider : ServiceProvider
{
    public void Boot()
    {
        Routes(() =>
        {
            Route.Group(typeof(Web));
        });
    }
}
