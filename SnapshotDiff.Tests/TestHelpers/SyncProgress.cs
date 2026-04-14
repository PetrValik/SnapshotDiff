namespace SnapshotDiff.Tests.TestHelpers;

/// <summary>
/// Synchronous IProgress&lt;T&gt; implementation for use in unit tests.
/// Unlike <see cref="System.Progress{T}"/>, this invokes the callback inline
/// (on the calling thread) so tests don't need Task.Delay to wait for callbacks.
/// </summary>
internal sealed class SyncProgress<T>(Action<T> handler) : IProgress<T>
{
    public void Report(T value) => handler(value);
}
