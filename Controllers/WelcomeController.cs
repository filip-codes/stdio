using Studio.Http;

namespace Studio.Controllers;

public class WelcomeController : Controller
{
    public string Index(Request request)
    {
        return this.View("Welcome");
    }
}