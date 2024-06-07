using Studio.Http;

namespace Studio.App.Http.Controllers;

public class ContactController : Controller
{
    public string Index()
    {
        return this.View("Contact");
    }
}
