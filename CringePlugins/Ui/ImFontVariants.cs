using ImGuiNET;

namespace CringePlugins.Ui;

public sealed class ImFontVariants : IImFontDictionary
{
    public static ImFontPtr Regular { get; private set; }
    public static ImFontPtr Bold { get; private set; }
    public static ImFontPtr ExtraBold { get; private set; }
    public static ImFontPtr Black { get; private set; }
    public static ImFontPtr Light { get; private set; }
    public static ImFontPtr ExtraLight { get; private set; }
    public static ImFontPtr Medium { get; private set; }
    public static ImFontPtr SemiBold { get; private set; }

    internal static void LoadFonts(ImGuiIOPtr io, string basePath, string name, nint glyphRanges, params ReadOnlySpan<FontVariant> variants)
    {
        ImFontPtr LoadFont(FontVariant variant, bool italic)
        {
            var fileName = $"{name}-{(italic ? $"{variant}Italic" : variant)}.ttf";
            var fontPtr = io.Fonts.AddFontFromFileTTF(Path.Join(basePath, fileName), 14, default, glyphRanges);

            unsafe
            {
                if (fontPtr.NativePtr == null)
                    throw new Exception($"Failed to load font {fileName}");
            }
            
            return fontPtr;
        }
        foreach (var variant in variants)
        {
            switch (variant)
            {
                case FontVariant.Regular:
                    Regular = LoadFont(variant, false);
                    break;
                case FontVariant.Bold:
                    Bold = LoadFont(variant, false);
                    break;
                case FontVariant.ExtraBold:
                    ExtraBold = LoadFont(variant, false);
                    break;
                case FontVariant.Black:
                    Black = LoadFont(variant, false);
                    break;
                case FontVariant.Light:
                    Light = LoadFont(variant, false);
                    break;
                case FontVariant.ExtraLight:
                    ExtraLight = LoadFont(variant, false);
                    break;
                case FontVariant.Medium:
                    Medium = LoadFont(variant, false);
                    break;
                case FontVariant.SemiBold:
                    SemiBold = LoadFont(variant, false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(variants));
            }
        }
        foreach (var variant in variants)
        {
            switch (variant)
            {
                case FontVariant.Regular:
                    Italic.Regular = LoadFont(variant, true);
                    break;
                case FontVariant.Bold:
                    Italic.Bold = LoadFont(variant, true);
                    break;
                case FontVariant.ExtraBold:
                    Italic.ExtraBold = LoadFont(variant, true);
                    break;
                case FontVariant.Black:
                    Italic.Black = LoadFont(variant, true);
                    break;
                case FontVariant.Light:
                    Italic.Light = LoadFont(variant, true);
                    break;
                case FontVariant.ExtraLight:
                    Italic.ExtraLight = LoadFont(variant, true);
                    break;
                case FontVariant.Medium:
                    Italic.Medium = LoadFont(variant, true);
                    break;
                case FontVariant.SemiBold:
                    Italic.SemiBold = LoadFont(variant, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(variants));
            }
        }
    }

    public sealed class Italic : IImFontDictionary
    {
        public static ImFontPtr Regular { get; internal set; }
        public static ImFontPtr Bold { get; internal set; }
        public static ImFontPtr ExtraBold { get; internal set; }
        public static ImFontPtr Black { get; internal set; }
        public static ImFontPtr Light { get; internal set; }
        public static ImFontPtr ExtraLight { get; internal set; }
        public static ImFontPtr Medium { get; internal set; }
        public static ImFontPtr SemiBold { get; internal set; }
    }
}

internal enum FontVariant : byte
{
    Regular,
    Bold,
    ExtraBold,
    Black,
    Light,
    ExtraLight,
    Medium,
    SemiBold
}

public interface IImFontDictionary
{
    static abstract ImFontPtr Regular { get; }
    static abstract ImFontPtr Bold { get; }
    static abstract ImFontPtr ExtraBold { get; }
    static abstract ImFontPtr Black { get; }
    static abstract ImFontPtr Light { get; }
    static abstract ImFontPtr ExtraLight { get; }
    static abstract ImFontPtr Medium { get; }
    static abstract ImFontPtr SemiBold { get; }
}