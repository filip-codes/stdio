using Studio.Http;

namespace Studio.App.Http.Controllers;

public class AboutController : Controller
{
    public string Index()
    {
        return this.View("About");
    }
}
