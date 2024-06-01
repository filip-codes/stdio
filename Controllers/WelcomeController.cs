namespace Studio.Controllers;

public class WelcomeController
{
    protected string View(string filename)
    {
        return File.ReadAllText($"Views/{filename}.cshtml").ReplaceLineEndings();
    }
    
    public string Index(Request request)
    {
        return this.View("Welcome");
    }
}