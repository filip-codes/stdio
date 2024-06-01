namespace Studio.Http;

public class Controller
{
    protected string View(string filename)
    {
        return File.ReadAllText($"Views/{filename}.cshtml").ReplaceLineEndings();
    }
}