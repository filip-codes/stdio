using Studio.Controllers;

namespace Studio.Routes;

public class Web : Foundation.Routes
{
    public void Register()
    {
        Route.Get("/", typeof(WelcomeController), "Index");
        Route.Get("/about", typeof(AboutController), "Index");
        Route.Get("/contact", typeof(ContactController), "Index");
    }
}
