namespace SnapshotDiff.Infrastructure.Notifications;

/// <summary>
/// In-process notification bus. Raise toast messages from any service or handler and let
/// the UI component subscribe to <see cref="OnNotification"/> to display them.
/// Registered as a scoped singleton so that all components within a Blazor circuit share
/// the same instance.
/// </summary>
public sealed class NotificationService : INotificationService
{
    public event Action<Notification>? OnNotification;

    public void ShowSuccess(string message)
    {
        OnNotification?.Invoke(new Notification
        {
            Message = message,
            Type = NotificationType.Success
        });
    }

    public void ShowError(string message)
    {
        OnNotification?.Invoke(new Notification
        {
            Message = message,
            Type = NotificationType.Error
        });
    }

    public void ShowInfo(string message)
    {
        OnNotification?.Invoke(new Notification
        {
            Message = message,
            Type = NotificationType.Info
        });
    }

    public void ShowWarning(string message)
    {
        OnNotification?.Invoke(new Notification
        {
            Message = message,
            Type = NotificationType.Warning
        });
    }
}