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

    public static string AfterFinal(this string originalString, char x)
    {

        for (int i = 1; i < originalString.Length; i++)
        {
            if (originalString[^i] == x)
                return originalString[^(i - 1)..];
        }
        return String.Empty;
    }

    public static string AfterXthChar(this string originalString, char x, int y)
    {
        int count = 0;
        for (int i = 0; i < originalString.Length; i++)
        {
            if (originalString[i] == x)
            {
                count++;
                if (count == y)
                    return originalString[(i + 1)..];
            }
        }
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
