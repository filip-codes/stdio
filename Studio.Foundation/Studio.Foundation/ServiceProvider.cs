namespace Studio.Foundation;

public class ServiceProvider
{
    public Application App;
    public Router Route;

    public void Routes(Action callback)
    {
        callback();
    }
}
