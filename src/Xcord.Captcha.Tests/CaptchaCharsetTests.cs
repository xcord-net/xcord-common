using Xcord.Captcha.Rendering;
using Xunit;

public class CaptchaCharsetTests
{
    [Fact]
    public void Charset_excludes_lookalike_characters()
    {
        foreach (var c in "O0I1LS5Z2B8G6Q")
            Assert.DoesNotContain(c, CaptchaCharset.Chars);
    }

    [Fact]
    public void NewCode_has_requested_length_and_only_charset_chars()
    {
        var code = CaptchaCharset.NewCode(5);
        Assert.Equal(5, code.Length);
        Assert.All(code, c => Assert.Contains(c, CaptchaCharset.Chars));
    }

    [Fact]
    public void NewCode_is_not_constant()
    {
        var codes = Enumerable.Range(0, 50).Select(_ => CaptchaCharset.NewCode()).Distinct().Count();
        Assert.True(codes > 1);
    }
}
