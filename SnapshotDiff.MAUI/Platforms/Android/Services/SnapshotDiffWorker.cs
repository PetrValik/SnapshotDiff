using AndroidX.Work;

namespace SnapshotDiff.MAUI.Platforms.Android.Services;

/// <summary>
/// WorkManager Worker stub — background scanning has been removed.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1816")]
public sealed class SnapshotDiffWorker : Worker
{
    public SnapshotDiffWorker(
        global::Android.Content.Context context,
        WorkerParameters workerParams)
        : base(context, workerParams)
    {
    }

    public override Result DoWork() => Result.InvokeSuccess();
}