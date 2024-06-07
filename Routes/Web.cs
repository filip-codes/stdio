using Studio.App.Http.Controllers;
using Studio.App.Http.Middleware;

namespace Studio.Routes;

public class Web : Foundation.Routes
{
    /// <summary>
    /// Register the routes for the application.
    /// </summary>
    public void Register()
    {
        Route.Get("/", typeof(WelcomeController), "Index");
        
        Route.Get("/about", typeof(AboutController), "Index")
            .Middleware(typeof(IsGuestMiddleware));
        
        Route.Get("/contact", typeof(ContactController), "Index")
            .Name("contact");
    }
}
