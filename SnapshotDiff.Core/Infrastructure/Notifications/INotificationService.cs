namespace SnapshotDiff.Infrastructure.Notifications;

/// <summary>
/// Pub/sub service for displaying toast notifications across the Blazor component tree.
/// Components subscribe to <see cref="OnNotification"/> and unsubscribe on dispose.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Fired whenever a new notification is raised. Subscribers must call <c>StateHasChanged</c> to update the UI.
    /// </summary>
    event Action<Notification>? OnNotification;

    /// <summary>
    /// Shows a green success toast.
    /// </summary>
    void ShowSuccess(string message);

    /// <summary>
    /// Shows a red error toast.
    /// </summary>
    void ShowError(string message);

    /// <summary>
    /// Shows a blue informational toast.
    /// </summary>
    void ShowInfo(string message);

    /// <summary>
    /// Shows a yellow warning toast.
    /// </summary>
    void ShowWarning(string message);
}