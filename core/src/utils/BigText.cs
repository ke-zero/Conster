namespace Conster.Core.utils;

public static class BigText
{
    public static string Parse(string content, int maxChars, string terminal = "...")
    {
        if (content.Length <= maxChars) return content;

        var chars = new List<char>();

        chars.AddRange(content.ToCharArray(0, maxChars - terminal.Length));
        chars.AddRange(terminal);

        return new string(chars.ToArray());
    }
}