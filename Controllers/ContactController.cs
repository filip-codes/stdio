using Studio.Http;

namespace Studio.Controllers;

public class ContactController : Controller
{
    public string Index()
    {
        return this.View("Contact");
    }
}
