namespace Studio.Exceptions;

public class ConfigurationNotFoundException : Exception
{
    public ConfigurationNotFoundException(string name) : base($"Configuration '{name}' not found.")
    {
        //
    }
}