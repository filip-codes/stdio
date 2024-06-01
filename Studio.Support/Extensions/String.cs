using System.Text.RegularExpressions;

namespace Studio.Support.Extensions;

public static class String
{
    public static string Camel(this string str)
    {
        string[] words = str.Split("_");
        string camel = words[0];
        
        for (int i = 1; i < words.Length; i++)
        {
            camel += char.ToUpper(words[i][0]) + words[i][1..];
        }
        
        return camel;
    }
    
    public static string Snake(this string str, string delimeter = "_")
    {
        string value = Regex.Replace(str, "/(.)(?=[A-Z])/u", "$1" + delimeter + "$2").ToLower();
        
        return value.Replace(" ", delimeter);
    }
    
    public static string Kebab(this string str)
    {
        return str.Snake("-");
    }
}