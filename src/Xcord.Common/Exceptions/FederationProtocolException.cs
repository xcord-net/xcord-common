namespace Xcord.Exceptions;

/// <summary>
/// Thrown when a federation payload fails protocol-level validation:
/// signature mismatch, malformed JSON, missing required actor/audience, etc.
/// Catch this in inbox loops to skip the offending message and continue.
/// </summary>
public sealed class FederationProtocolException : XcordException
{
    public FederationProtocolException(string message, Exception? innerException = null)
        : base(message, "federation_protocol_error", innerException)
    {
    }

    public FederationProtocolException(string message, string errorCode, Exception? innerException = null)
        : base(message, errorCode, innerException)
    {
    }
}
