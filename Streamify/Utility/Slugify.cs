namespace Streamify.Utility;

public class Slugify
{
    public static string SlugifyUrl(string title)
    {
        return string.Join("-", title
                .ToLowerInvariant()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Replace(".", "")
            .Replace(",", "")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("/", "")
            .Replace("\\", "")
            .Replace("&", "and");
    }
}