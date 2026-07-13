using System.Text;
using Xcord.Captcha.Rendering;
using Xunit;

public class GhostFontRendererWavTests
{
    private readonly IGhostFontRenderer _r = new GhostFontRenderer();

    [Fact]
    public void RenderWav_returns_valid_riff_wave()
    {
        var b = _r.RenderWav("ACDEF", 5);
        Assert.Equal("RIFF", Encoding.ASCII.GetString(b, 0, 4));
        Assert.Equal("WAVE", Encoding.ASCII.GetString(b, 8, 4));
        Assert.Equal("data", Encoding.ASCII.GetString(b, 36, 4));
    }

    [Fact]
    public void Longer_code_yields_longer_audio()
        => Assert.True(_r.RenderWav("ACDEFH", 5).Length > _r.RenderWav("AC", 5).Length);

    [Fact]
    public void RenderWav_is_deterministic()
        => Assert.Equal(_r.RenderWav("ACDEF", 5), _r.RenderWav("ACDEF", 5));

    [Fact]
    public void Every_charset_character_has_a_playable_clip()
    {
        // Each single-char code must render without a missing-resource error and carry audio
        // beyond the fixed lead-in/gap noise (i.e. the embedded clip contributed samples).
        var baseline = _r.RenderWav("A", 1).Length;
        foreach (var c in CaptchaCharset.Chars)
        {
            var bytes = _r.RenderWav(c.ToString(), 1);
            Assert.True(bytes.Length > 44, $"'{c}' produced no audio");
        }
        Assert.True(baseline > 44);
    }
}
