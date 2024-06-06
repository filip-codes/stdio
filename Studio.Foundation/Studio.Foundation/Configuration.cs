namespace Studio.Foundation;

public class Configuration
{
    public Type? GetType(string name)
    {
        return Type.GetType($"Studio.Config.{name}");
    }
}