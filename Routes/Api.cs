using Studio.App.Http.Controllers.Api;

namespace Studio.Routes;

public class Api : Foundation.Routes
{
    public void Register()
    {
        Route.Get("/users", typeof(UserController), "Index");
    }
}