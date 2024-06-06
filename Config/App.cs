using Studio.App.Providers;
using Studio.Support;

namespace Studio.Config;

public class App
{
    public string Url { get; } = Env.Get("APP_URL", "http://localhost");
    
    public List<Type> Providers { get; } = [
        typeof(RouteServiceProvider),
    ];
}