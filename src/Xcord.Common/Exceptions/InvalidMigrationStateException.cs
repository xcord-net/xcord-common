namespace Xcord.Exceptions;

/// <summary>
/// Thrown when a Discord (or other) migration phase enters an invalid or
/// unrecoverable state - missing prerequisite data, conflicting state, or a
/// downstream API surface that we cannot reconcile.
/// </summary>
public sealed class InvalidMigrationStateException : XcordException
{
    public string? Phase { get; init; }

    public InvalidMigrationStateException(string message, Exception? innerException = null)
        : base(message, "invalid_migration_state", innerException)
    {
    }

    public InvalidMigrationStateException(string message, string phase, Exception? innerException = null)
        : base(message, "invalid_migration_state", innerException)
    {
        Phase = phase;
    }
}
