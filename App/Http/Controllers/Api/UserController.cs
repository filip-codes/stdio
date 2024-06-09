using Studio.Http;

namespace Studio.App.Http.Controllers.Api;

public class UserController : Controller
{
    public string Index()
    {
        return View("Contact");
    }
}