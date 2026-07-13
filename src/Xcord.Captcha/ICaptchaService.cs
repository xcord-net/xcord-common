namespace Xcord.Captcha;

public sealed record CaptchaChallenge(string Id);

public interface ICaptchaService
{
    /// <summary>Generate a new challenge, persist its answer, and return its id.</summary>
    Task<CaptchaChallenge> GenerateAsync();

    /// <summary>Render the challenge as an animated GIF, or null if the id is unknown/expired.</summary>
    Task<byte[]?> RenderGifAsync(string captchaId);

    /// <summary>Render the challenge as audio (WAV), or null if the id is unknown/expired.</summary>
    Task<byte[]?> RenderWavAsync(string captchaId);

    /// <summary>Validate an answer. Single-use: a correct answer is consumed.</summary>
    Task<bool> ValidateAsync(string captchaId, string answer);
}
