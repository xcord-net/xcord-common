namespace Xcord.Exceptions;

/// <summary>
/// Thrown when provisioning of underlying infrastructure (Docker service, Swarm,
/// secret, volume, etc.) fails in an unrecoverable way for the current operation.
/// </summary>
public sealed class ProvisioningFailedException : XcordException
{
    public string? Resource { get; init; }

    public ProvisioningFailedException(string message, Exception? innerException = null)
        : base(message, "provisioning_failed", innerException)
    {
    }

    public ProvisioningFailedException(string message, string resource, Exception? innerException = null)
        : base(message, "provisioning_failed", innerException)
    {
        Resource = resource;
    }
}
