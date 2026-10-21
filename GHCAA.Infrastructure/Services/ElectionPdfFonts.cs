using QuestPDF.Drawing;

namespace GHCAA.Infrastructure.Services;

// Lato, QuestPDF's own font, has no Bengali letters, so a Bengali name printed blank boxes.
// Election PDFs list Noto Sans Bengali after it, and QuestPDF takes each missing letter from there.
// The font files ship inside this assembly under the SIL Open Font License (Fonts/NotoSansBengali-OFL.txt).
public static class ElectionPdfFonts
{
    public const string Primary = "Lato";
    public const string Bengali = "Noto Sans Bengali";

    private static readonly object Gate = new();
    private static bool _registered;

    // Returns null once both weights are registered, or the error. A failure is not remembered,
    // so the next PDF tries again, and the caller still prints with Lato alone.
    public static Exception? EnsureRegistered()
    {
        if (Volatile.Read(ref _registered))
            return null;
        lock (Gate)
        {
            if (_registered)
                return null;
            try
            {
                var assembly = typeof(ElectionPdfFonts).Assembly;
                foreach (var file in new[] { "NotoSansBengali-Regular.ttf", "NotoSansBengali-Bold.ttf" })
                {
                    using var stream = assembly.GetManifestResourceStream($"GHCAA.Infrastructure.Fonts.{file}")
                        ?? throw new InvalidOperationException($"Font {file} is not embedded in {assembly.GetName().Name}.");
                    FontManager.RegisterFont(stream);
                }
                Volatile.Write(ref _registered, true);
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}
