using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using PointF = System.Drawing.PointF;

namespace Xcord.Captcha.Rendering;

/// <summary>
/// Renders a captcha code as a motion-coherence animated GIF: the glyphs are formed only
/// by the motion of low-contrast, background-matched dots. No single frame is readable and a
/// max-projection of all frames stays diffuse (decoys dominate); only coherent directional
/// motion along the glyph strokes reveals the code to human perception.
/// </summary>
public sealed partial class GhostFontRenderer : IGhostFontRenderer
{
    private const int Width = 240;
    private const int Height = 80;
    private const int FrameCount = 30;
    private const int FrameDelayCs = 5; // centiseconds -> 50ms per frame (~1.5s loop)

    private const int GlyphAdvance = 44;
    private const int PadX = 12;
    private const int GlyphTop = 14;
    private const int GlyphHeight = 52;
    private const int GlyphWidth = 30;

    private const int PointsPerSegment = 24;
    private const float LitBand = 0.06f;   // fraction of the sweep that is lit at any frame
    private const int DecoyCount = 1300;   // spread so the stack stays noisy (>35% union)
    private const int DotRadius = 1;

    private static readonly Rgba32 Bg = new(96, 96, 104);
    private static readonly Rgba32 Dot = new(120, 120, 128); // low contrast vs Bg

    public byte[] RenderGif(string code, int seed)
    {
        if (string.IsNullOrEmpty(code)) throw new ArgumentException("Code required", nameof(code));

        var rng = new Random(seed);
        var strokePoints = BuildStrokePixels(code);
        var decoys = BuildDecoys(rng);

        Image<Rgba32>? gif = null;
        try
        {
            for (var f = 0; f < FrameCount; f++)
            {
                var buffer = new Rgba32[Width * Height];
                Array.Fill(buffer, Bg);
                DrawRealDots(buffer, strokePoints, f);
                DrawDecoys(buffer, decoys, f);

                var frame = Image.LoadPixelData<Rgba32>(buffer, Width, Height);
                frame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = FrameDelayCs;

                if (gif is null)
                {
                    gif = frame;
                }
                else
                {
                    gif.Frames.AddFrame(frame.Frames.RootFrame);
                    frame.Dispose();
                }
            }

            var meta = gif!.Metadata.GetGifMetadata();
            meta.RepeatCount = 0; // loop forever

            using var ms = new MemoryStream();
            gif.SaveAsGif(ms, new GifEncoder { ColorTableMode = GifColorTableMode.Global });
            return ms.ToArray();
        }
        finally
        {
            gif?.Dispose();
        }
    }

    // Resample every glyph stroke into an ordered list of pixel-space points along arclength.
    private static List<PointF> BuildStrokePixels(string code)
    {
        var pts = new List<PointF>();
        for (var i = 0; i < code.Length; i++)
        {
            var ox = PadX + i * GlyphAdvance;
            foreach (var stroke in GhostGlyphs.Strokes(code[i]))
            {
                for (var s = 0; s < stroke.Count - 1; s++)
                {
                    var a = stroke[s];
                    var b = stroke[s + 1];
                    for (var t = 0; t <= PointsPerSegment; t++)
                    {
                        var u = t / (float)PointsPerSegment;
                        var x = ox + (a.X + (b.X - a.X) * u) * GlyphWidth;
                        var y = GlyphTop + (a.Y + (b.Y - a.Y) * u) * GlyphHeight;
                        pts.Add(new PointF(x, y));
                    }
                }
            }
        }
        return pts;
    }

    // A lit band sweeps along the ordered stroke points: point k is lit at frame f when
    // ((k/total) - f/FrameCount) mod 1 falls inside a narrow window. The human integrates the
    // sweep over time into the glyph; no single frame shows more than a thin arc of each letter.
    private static void DrawRealDots(Rgba32[] buffer, List<PointF> pts, int f)
    {
        if (pts.Count == 0) return;
        var phase = f / (float)FrameCount;
        for (var k = 0; k < pts.Count; k++)
        {
            var s = k / (float)pts.Count;
            var d = s - phase;
            d -= MathF.Floor(d);
            if (d < LitBand) PlotDot(buffer, pts[k]);
        }
    }

    // Each decoy fires on one assigned frame (plus a light random flicker) so single frames look
    // like uniform noise, while the union across frames covers a wide, diffuse area.
    private static void DrawDecoys(Rgba32[] buffer, Decoy[] decoys, int f)
    {
        foreach (var decoy in decoys)
            if (decoy.Frame == f)
                PlotDot(buffer, new PointF(decoy.X, decoy.Y));
    }

    private static Decoy[] BuildDecoys(Random rng)
    {
        var arr = new Decoy[DecoyCount];
        for (var i = 0; i < DecoyCount; i++)
            arr[i] = new Decoy(rng.Next(Width), rng.Next(Height), rng.Next(FrameCount));
        return arr;
    }

    private static void PlotDot(Rgba32[] buffer, PointF p)
    {
        var cx = (int)MathF.Round(p.X);
        var cy = (int)MathF.Round(p.Y);
        for (var dy = -DotRadius; dy <= DotRadius; dy++)
        for (var dx = -DotRadius; dx <= DotRadius; dx++)
        {
            var x = cx + dx;
            var y = cy + dy;
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                buffer[y * Width + x] = Dot;
        }
    }

    private readonly record struct Decoy(int X, int Y, int Frame);

    // Implemented in Task 4 (GhostFontRenderer.Audio.cs); stub keeps the interface satisfied.
    public byte[] RenderWav(string code, int seed) => throw new NotImplementedException();
}
