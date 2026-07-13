using Microsoft.Extensions.Options;
using Xcord.Captcha.Rendering;
using Xcord.Captcha.Storage;

namespace Xcord.Captcha;

/// <summary>
/// Composes the renderer and store. Stores (answer, seed) per challenge; renders re-derive from
/// the seed so bytes are reproducible for an id. Validation is single-use.
/// </summary>
public sealed class CaptchaService(
    IGhostFontRenderer renderer,
    ICaptchaStore store,
    IOptions<CaptchaOptions> options) : ICaptchaService
{
    private readonly CaptchaOptions _options = options.Value;

    public async Task<CaptchaChallenge> GenerateAsync()
    {
        var code = CaptchaCharset.NewCode(_options.CodeLength);
        var id = Guid.NewGuid().ToString("N");
        var seed = System.Security.Cryptography.RandomNumberGenerator.GetInt32(int.MaxValue);
        await store.SaveAsync(id, code, seed, TimeSpan.FromMinutes(_options.TtlMinutes)).ConfigureAwait(false);
        return new CaptchaChallenge(id);
    }

    public async Task<byte[]?> RenderGifAsync(string captchaId)
    {
        var (answer, seed) = await LoadForRenderAsync(captchaId).ConfigureAwait(false);
        return answer is null || seed is null ? null : renderer.RenderGif(answer, seed.Value);
    }

    public async Task<byte[]?> RenderWavAsync(string captchaId)
    {
        var (answer, seed) = await LoadForRenderAsync(captchaId).ConfigureAwait(false);
        return answer is null || seed is null ? null : renderer.RenderWav(answer, seed.Value);
    }

    public async Task<bool> ValidateAsync(string captchaId, string answer)
    {
        if (string.IsNullOrWhiteSpace(captchaId) || string.IsNullOrWhiteSpace(answer)) return false;
        var stored = await store.TakeAnswerAsync(captchaId).ConfigureAwait(false);
        return stored is not null &&
               string.Equals(stored.Trim(), answer.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    // Non-consuming load for rendering (the answer/code is needed to draw, but must NOT be
    // consumed - only ValidateAsync consumes it).
    private async Task<(string? answer, int? seed)> LoadForRenderAsync(string id)
    {
        var seed = await store.PeekSeedAsync(id).ConfigureAwait(false);
        if (seed is null) return (null, null);
        var answer = await store.PeekAnswerAsync(id).ConfigureAwait(false);
        return (answer, seed);
    }
}
