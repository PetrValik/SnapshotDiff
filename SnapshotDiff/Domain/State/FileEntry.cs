namespace SnapshotDiff.Domain.State
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class FileEntry
    {
        /// <summary>
        /// 
        /// </summary>
        internal string FileRelativePath { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        internal string Extension { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        internal string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        internal string FileHash { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        internal long FileSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal DateTime LastModification { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal int Version { get; set; } = 1;
    }
}
