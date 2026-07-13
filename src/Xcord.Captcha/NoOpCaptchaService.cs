namespace Xcord.Captcha;

/// <summary>
/// Used when captcha is disabled (dev/test). Validation always passes and renders return a
/// tiny valid placeholder (a 1x1 GIF and an empty WAV) so the endpoints stay well-formed.
/// </summary>
public sealed class NoOpCaptchaService : ICaptchaService
{
    // Minimal valid 1x1 GIF89a.
    private static readonly byte[] PlaceholderGif =
    [
        0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x80, 0x00, 0x00, 0x00,
        0x00, 0x00, 0xFF, 0xFF, 0xFF, 0x21, 0xF9, 0x04, 0x01, 0x00, 0x00, 0x00, 0x00, 0x2C,
        0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x02, 0x02, 0x44, 0x01, 0x00, 0x3B
    ];

    // Minimal valid empty 16-bit mono 16kHz WAV (44-byte header, zero samples).
    private static readonly byte[] PlaceholderWav = BuildEmptyWav();

    public Task<CaptchaChallenge> GenerateAsync() => Task.FromResult(new CaptchaChallenge("disabled"));

    public Task<byte[]?> RenderGifAsync(string captchaId) => Task.FromResult<byte[]?>(PlaceholderGif);

    public Task<byte[]?> RenderWavAsync(string captchaId) => Task.FromResult<byte[]?>(PlaceholderWav);

    public Task<bool> ValidateAsync(string captchaId, string answer) => Task.FromResult(true);

    private static byte[] BuildEmptyWav()
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        w.Write("RIFF"u8.ToArray());
        w.Write(36);
        w.Write("WAVE"u8.ToArray());
        w.Write("fmt "u8.ToArray());
        w.Write(16);
        w.Write((short)1);
        w.Write((short)1);
        w.Write(16000);
        w.Write(32000);
        w.Write((short)2);
        w.Write((short)16);
        w.Write("data"u8.ToArray());
        w.Write(0);
        w.Flush();
        return ms.ToArray();
    }
}
