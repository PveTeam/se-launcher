namespace CringeLauncher.Platform.Xplat;

internal class PathComparer : IEqualityComparer<string>
{
    public static readonly IEqualityComparer<string> Instance = new PathComparer();
    public bool Equals(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (x is null || y is null)
            return false;
        if (x.Length != y.Length)
            return false;

        for (var i = 0; i < x.Length; i++)
        {
            var c1 = x[i];
            var c2 = y[i];
            if (c1 is '\\' or '/' && c2 is '\\' or '/')
                continue;
            ReadOnlySpan<char> span1 = [c1];
            ReadOnlySpan<char> span2 = [c2];
            if (!span1.Equals(span2, StringComparison.OrdinalIgnoreCase)) return false;
        }

        return true;
    }

    public int GetHashCode(string obj)
    {
        var code = new HashCode();
        foreach (var c in obj)
        {
            var c1 = c;
            if (c == '/') c1 = '\\';
            ReadOnlySpan<char> span = [c1];
            code.Add(string.GetHashCode(span, StringComparison.OrdinalIgnoreCase));
        }

        return code.ToHashCode();
    }
}
