namespace Xcord.Captcha.Storage;

/// <summary>
/// Persistence for issued captcha challenges. Implementations must make <see cref="TakeAnswerAsync"/>
/// single-use (the answer is consumed atomically) and expire entries after the given TTL.
/// </summary>
public interface ICaptchaStore
{
    /// <summary>Store the answer and render seed for a challenge id, expiring after <paramref name="ttl"/>.</summary>
    Task SaveAsync(string id, string answer, int seed, TimeSpan ttl);

    /// <summary>Return the render seed for an id (keeps the entry), or null if unknown/expired.</summary>
    Task<int?> PeekSeedAsync(string id);

    /// <summary>Return the answer for an id WITHOUT consuming it (rendering only), or null.</summary>
    Task<string?> PeekAnswerAsync(string id);

    /// <summary>Atomically return and delete the answer for an id (single-use validation), or null.</summary>
    Task<string?> TakeAnswerAsync(string id);
}
