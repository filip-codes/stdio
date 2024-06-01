namespace Studio;

public class Route
{
    public string Path { get; set; }
    public Type Controller { get; set; }
    public string Method { get; set; }

    public Route(string path, Type controller, string method)
    {
        this.Path = path;
        this.Controller = controller;
        this.Method = method;
    }
}