namespace WebServer.Website;

public static class AdrnExtns
{
    public static string After(this string originalString, char x)
    {
        if (originalString.IndexOf(x) != -1) { return originalString[(originalString.IndexOf(x) + 1)..]; }
        return String.Empty;
    }

    public static string After(this string originalString, string x)
    {
        if (originalString.IndexOf(x) != -1) { return originalString[(originalString.IndexOf(x) + 1)..]; }
        return String.Empty;
    }

    public static string Before(this string originalString, char x)
    {
        if (originalString.IndexOf(x) != -1) { return originalString[1..originalString.IndexOf(x)]; }
        return String.Empty;
    }
    public static string Before(this string originalString, string x)
    {
        if (originalString.IndexOf(x) != -1) { return originalString[1..originalString.IndexOf(x)]; }
        return String.Empty;
    }

    public static bool IsNull<T>(this object obj)
    {
        return obj == null;
    }

    public static bool IsNull<T>(this IEnumerable<T> obj)
    {
        return obj == null;
    }

}
