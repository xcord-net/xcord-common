using Microsoft.Extensions.Options;
using Xcord.Captcha;
using Xcord.Captcha.Rendering;
using Xcord.Captcha.Storage;
using Xunit;

public class CaptchaServiceTests
{
    private static (CaptchaService svc, InMemoryCaptchaStore store) Make()
    {
        var store = new InMemoryCaptchaStore();
        var svc = new CaptchaService(new GhostFontRenderer(), store, Options.Create(new CaptchaOptions()));
        return (svc, store);
    }

    [Fact]
    public async Task Generate_renders_gif_and_wav_for_the_id()
    {
        var (svc, _) = Make();
        var ch = await svc.GenerateAsync();
        var gif = await svc.RenderGifAsync(ch.Id);
        var wav = await svc.RenderWavAsync(ch.Id);
        Assert.NotNull(gif);
        Assert.Equal((byte)'G', gif![0]);
        Assert.NotNull(wav);
        Assert.Equal((byte)'R', wav![0]);
    }

    [Fact]
    public async Task Correct_answer_validates_once_then_is_consumed()
    {
        var (svc, store) = Make();
        var ch = await svc.GenerateAsync();
        var answer = await store.PeekAnswerAsync(ch.Id); // read the server-side code for the test
        Assert.NotNull(answer);

        Assert.True(await svc.ValidateAsync(ch.Id, answer!.ToLowerInvariant())); // case-insensitive
        Assert.False(await svc.ValidateAsync(ch.Id, answer)); // single-use, already consumed
    }

    [Fact]
    public async Task Wrong_answer_fails()
    {
        var (svc, _) = Make();
        var ch = await svc.GenerateAsync();
        Assert.False(await svc.ValidateAsync(ch.Id, "ZZZZZ"));
    }

    [Fact]
    public async Task Render_unknown_id_returns_null()
    {
        var (svc, _) = Make();
        Assert.Null(await svc.RenderGifAsync("nope"));
        Assert.Null(await svc.RenderWavAsync("nope"));
    }

    [Fact]
    public async Task NoOp_service_always_validates_and_renders_valid_placeholder()
    {
        var noop = new NoOpCaptchaService();
        Assert.True(await noop.ValidateAsync("x", "y"));
        var gif = await noop.RenderGifAsync("x");
        Assert.NotNull(gif);
        Assert.Equal((byte)'G', gif![0]);
        Assert.Equal("disabled", (await noop.GenerateAsync()).Id);
    }
}
