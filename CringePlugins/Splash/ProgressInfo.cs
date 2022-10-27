namespace CringePlugins.Splash;

public record ProgressInfo(string Text)
{
    public static implicit operator ProgressInfo(string s) => new(s);
    public static implicit operator ProgressInfo((string, float) s) => new PercentProgressInfo(s.Item1, s.Item2);
}

public record PercentProgressInfo(string Text, float Percent = 0) : ProgressInfo(Text);