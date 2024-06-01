using System.Text.RegularExpressions;

namespace Studio.Http;

public class Controller
{
    protected string View(string filename)
    {
        string content = File.ReadAllText($"Views/{filename}.box");

        content = this.ReplaceLayouts(content);
        content = this.ReplaceYields(content);
        content = this.ReplaceIncludes(content);

        return content;
    }
    
    protected string ReplaceLayouts(string content)
    {
        content = Regex.Replace(content, "@layout\\('(.*)'\\)", (match) =>
        {
            string layout = match.Groups[1].Value;
            layout = layout.Replace(".", Path.DirectorySeparatorChar.ToString());
            string layoutContent = File.ReadAllText($"Views/{layout}.box")!;
            
            return layoutContent;
        });
        
        return content;
    }
    
    protected string ReplaceYields(string content)
    {
        // copy content to a new variable
        string newContent = content;
        
        // Take everything between @section('content') and @endsection
        string pattern = "@section\\('(.*)'\\)(.*)@endsection";
        MatchCollection matches = Regex.Matches(content, pattern, RegexOptions.Singleline);
        
        foreach (Match match in matches)
        {
            string sectionName = match.Groups[1].Value;
            string sectionContent = match.Groups[2].Value.Trim();
            
            // Replace @yield('content') with the content of the section
            newContent = newContent.Replace($"@yield('{sectionName}')", sectionContent);
        }
        
        // you replaced welcome and thats good. but we need to remove the section at the end
        // we can do this by replacing the section with an empty string
        newContent = Regex.Replace(newContent, pattern, string.Empty, RegexOptions.Singleline).Trim();
        
        return newContent;
    }
    
    protected string ReplaceIncludes(string content)
    {
        content = Regex.Replace(content, "@include\\('(.*)'\\)", (match) =>
        {
            string include = match.Groups[1].Value;
            include = include.Replace(".", Path.DirectorySeparatorChar.ToString());
            return File.ReadAllText($"Views/{include}.box")!;
        });

        return content;
    }
}
