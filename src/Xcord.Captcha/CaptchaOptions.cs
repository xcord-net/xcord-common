namespace Xcord.Captcha;

public sealed class CaptchaOptions
{
    /// <summary>When false, a no-op service is used (validation always passes) for dev/test.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Number of characters in a generated code.</summary>
    public int CodeLength { get; set; } = 5;

    /// <summary>How long an issued challenge remains valid.</summary>
    public int TtlMinutes { get; set; } = 5;

    /// <summary>Redis key prefix (namespacing on shared Redis).</summary>
    public string KeyPrefix { get; set; } = "captcha:";
}
