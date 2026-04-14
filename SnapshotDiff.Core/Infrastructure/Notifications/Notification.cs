namespace SnapshotDiff.Infrastructure.Notifications;

/// <summary>
/// Carries a single notification raised by <see cref="INotificationService"/>.
/// </summary>
public sealed class Notification
{
    /// <summary>
    /// Text to display in the toast.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Visual severity of the notification.
    /// </summary>
    public required NotificationType Type { get; init; }

    /// <summary>
    /// Unique identifier used by the UI to key and auto-dismiss individual toasts.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();
}