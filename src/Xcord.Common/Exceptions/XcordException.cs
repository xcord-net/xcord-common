namespace Xcord.Exceptions;

/// <summary>
/// Base type for all Xcord-defined exceptions. Catch this instead of broad
/// <see cref="System.Exception"/> when you want to recover from a known failure
/// produced inside Xcord code paths.
/// </summary>
public abstract class XcordException : Exception
{
    /// <summary>
    /// Stable machine-readable code for logging / metrics / client error mapping.
    /// May be null when no specific code is set.
    /// </summary>
    public string? ErrorCode { get; init; }

    protected XcordException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    protected XcordException(string message, string errorCode, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
