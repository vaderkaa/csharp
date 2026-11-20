using System.Text;

namespace tasks;

/// <summary>
/// Here you should implement PascalToSnakeCase and SnakeToPascalCase string extension methods.
/// </summary>
public static class StringExtensions
{
   public static string PascalToSnakeCase(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        var sb = new StringBuilder();
        sb.Append(char.ToLower(s[0]));

        for(int i = 1; i < s.Length; i++)
        {
            if (char.IsUpper(s[i]))
            {
                sb.Append('_');
                sb.Append(char.ToLower(s[i]));
            }
            else
                sb.Append(s[i]);
        }

        return sb.ToString();
    }

    public static string SnakeToPascalCase(this string s)
    {
        if (string.IsNullOrEmpty(s))
            return string.Empty;

        var sb = new StringBuilder();
        bool capitalizeNext = true;

        foreach (char c in s)
        {
            if (c == '_')
            {
                capitalizeNext = true;
            }
            else if (capitalizeNext)
            {
                sb.Append(char.ToUpper(c));
                capitalizeNext = false;
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}