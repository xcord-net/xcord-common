using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xcord.Captcha.Rendering;
using Xunit;

public class GhostFontRendererGifTests
{
    private readonly IGhostFontRenderer _r = new GhostFontRenderer();

    [Fact]
    public void RenderGif_returns_valid_multiframe_gif()
    {
        var bytes = _r.RenderGif("ACDEF", 12345);
        Assert.True(bytes.Length > 0);
        Assert.Equal((byte)'G', bytes[0]);
        Assert.Equal((byte)'I', bytes[1]);
        Assert.Equal((byte)'F', bytes[2]);
        using var img = Image.Load<Rgba32>(bytes);
        Assert.True(img.Frames.Count > 1, "GIF must be animated");
    }

    [Fact]
    public void RenderGif_is_deterministic_for_same_code_and_seed()
    {
        Assert.Equal(_r.RenderGif("ACDEF", 7), _r.RenderGif("ACDEF", 7));
    }

    [Fact]
    public void RenderGif_differs_for_different_seed()
    {
        Assert.NotEqual(_r.RenderGif("ACDEF", 1), _r.RenderGif("ACDEF", 2));
    }

    // Security guard: no single frame is readable. Approximate "readable" via lit-pixel
    // coverage: in any single frame the lit dots must be a small fraction of the canvas.
    [Fact]
    public void No_single_frame_reveals_more_than_threshold_coverage()
    {
        using var img = Image.Load<Rgba32>(_r.RenderGif("ACDEF", 99));
        var bg = img.Frames[0][2, 2]; // corner ~ background
        foreach (var frame in img.Frames)
        {
            var lit = 0;
            var total = 0;
            frame.ProcessPixelRows(acc =>
            {
                for (var y = 0; y < acc.Height; y++)
                {
                    var row = acc.GetRowSpan(y);
                    for (var x = 0; x < row.Length; x++)
                    {
                        total++;
                        var p = row[x];
                        var dist = Math.Abs(p.R - bg.R) + Math.Abs(p.G - bg.G) + Math.Abs(p.B - bg.B);
                        if (dist > 24) lit++;
                    }
                }
            });
            var coverage = (double)lit / total;
            Assert.True(coverage < 0.15, $"single frame coverage {coverage:P1} too high (readable)");
        }
    }

    // Anti-stacking guard: max-projection across frames must stay diffuse, i.e. the union of
    // lit pixels covers a large area (decoys dominate), not a sharp isolated glyph.
    [Fact]
    public void Max_projection_stack_stays_diffuse()
    {
        using var img = Image.Load<Rgba32>(_r.RenderGif("ACDEF", 99));
        var w = img.Width;
        var h = img.Height;
        var everLit = new bool[w * h];
        var bg = img.Frames[0][2, 2];
        foreach (var frame in img.Frames)
        {
            frame.ProcessPixelRows(acc =>
            {
                for (var y = 0; y < acc.Height; y++)
                {
                    var row = acc.GetRowSpan(y);
                    for (var x = 0; x < row.Length; x++)
                    {
                        var p = row[x];
                        var dist = Math.Abs(p.R - bg.R) + Math.Abs(p.G - bg.G) + Math.Abs(p.B - bg.B);
                        if (dist > 24) everLit[y * w + x] = true;
                    }
                }
            });
        }
        var union = everLit.Count(b => b) / (double)(w * h);
        Assert.True(union > 0.35, $"stack union {union:P1} too sparse - glyph may be recoverable");
    }
}
