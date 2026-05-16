namespace Xcord.Exceptions;

/// <summary>
/// Thrown when an upstream external service (webhook target, third-party API,
/// captcha provider, GIF search, etc.) fails in a way the caller may want to
/// surface differently from internal errors.
/// </summary>
public sealed class ExternalServiceException : XcordException
{
    public string ServiceName { get; }

    public int? StatusCode { get; init; }

    public ExternalServiceException(string serviceName, string message, Exception? innerException = null)
        : base(message, "external_service_error", innerException)
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, int statusCode, Exception? innerException = null)
        : base(message, "external_service_error", innerException)
    {
        ServiceName = serviceName;
        StatusCode = statusCode;
    }
}
