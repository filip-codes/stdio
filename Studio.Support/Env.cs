using Studio.Exceptions;

namespace Studio.Support;

public class Env
{
    public static void LoadFrom(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("File not found", path);

        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            string[] parts = line.Split('=');

            if (parts.Length != 2)
                throw new InvalidOperationException("Invalid .env file");

            string key = parts[0];
            string value = parts[1];

            Environment.SetEnvironmentVariable(key, value);
        }
    }

    public static string Get(string key, string? defaultValue = null)
    {
        string? value = Environment.GetEnvironmentVariable(key);
        if (value is null)
            value = defaultValue;

        if (value is null)
            throw new EnvironmentVariableNotFoundException(key);

        return value;
    }
}