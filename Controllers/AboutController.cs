using Studio.Http;

namespace Studio.Controllers;

public class AboutController : Controller
{
    public string Index()
    {
        return this.View("About");
    }
}
