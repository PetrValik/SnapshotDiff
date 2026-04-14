namespace SnapshotDiff.Infrastructure.Notifications;

/// <summary>
/// Visual severity level of a <see cref="Notification"/>.
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Operation completed successfully.
    /// </summary>
    Success,
    /// <summary>
    /// An error occurred that requires user attention.
    /// </summary>
    Error,
    /// <summary>
    /// Informational message with no action required.
    /// </summary>
    Info,
    /// <summary>
    /// Potentially harmful situation that the user should be aware of.
    /// </summary>
    Warning
}
