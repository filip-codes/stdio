namespace Studio.Support;

public class DotEnv
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
}