using System.Numerics;

namespace CringePlugins.Utils;

public static class ColorExtensions
{
    public static Vector4 ToFloat4(this Color color) =>
        new((float)color.R / 255, (float)color.G / 255, (float)color.B / 255, (float)color.A / 255);
}