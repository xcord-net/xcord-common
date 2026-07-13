using System.Drawing;
using Xcord.Captcha.Rendering;
using Xunit;

public class GhostGlyphsTests
{
    [Fact]
    public void Every_charset_character_has_at_least_one_stroke()
    {
        foreach (var c in CaptchaCharset.Chars)
        {
            var strokes = GhostGlyphs.Strokes(c);
            Assert.NotEmpty(strokes);
            Assert.All(strokes, s => Assert.True(s.Count >= 2, $"'{c}' stroke needs >=2 points"));
        }
    }

    [Fact]
    public void All_points_are_within_unit_box()
    {
        foreach (var c in CaptchaCharset.Chars)
        foreach (var stroke in GhostGlyphs.Strokes(c))
        foreach (var p in stroke)
        {
            Assert.InRange(p.X, 0f, 1f);
            Assert.InRange(p.Y, 0f, 1f);
        }
    }

    [Fact]
    public void Unknown_character_throws()
        => Assert.Throws<ArgumentOutOfRangeException>(() => GhostGlyphs.Strokes('*'));
}
