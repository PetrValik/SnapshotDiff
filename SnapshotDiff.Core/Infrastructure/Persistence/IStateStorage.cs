namespace SnapshotDiff.Infrastructure.Persistence;

/// <summary>
/// Abstraction for loading and persisting application state.
/// All state access in the application goes through this interface.
/// </summary>
public interface IStateStorage<TState> where TState : new()
{
    /// <summary>
    /// Loads the persisted state, or returns a default <typeparamref name="TState"/> instance
    /// when no saved state exists yet.
    /// </summary>
    Task<TState> LoadAsync(CancellationToken ct = default);

    /// <summary>
    /// Persists the current application state.
    /// </summary>
    Task SaveAsync(TState state, CancellationToken ct = default);

}