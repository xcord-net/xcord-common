namespace Xcord.Exceptions;

/// <summary>
/// Thrown when verification of Docker network membership / connectivity fails.
/// </summary>
public sealed class NetworkVerificationException : XcordException
{
    public string? NetworkId { get; init; }

    public NetworkVerificationException(string message, Exception? innerException = null)
        : base(message, "network_verification_failed", innerException)
    {
    }

    public NetworkVerificationException(string message, string networkId, Exception? innerException = null)
        : base(message, "network_verification_failed", innerException)
    {
        NetworkId = networkId;
    }
}
