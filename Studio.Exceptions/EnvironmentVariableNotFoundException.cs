namespace Studio.Exceptions;

public class EnvironmentVariableNotFoundException : Exception
{
    public EnvironmentVariableNotFoundException(string name) : base($"Environment variable {name} not found.")
    {
    }
}