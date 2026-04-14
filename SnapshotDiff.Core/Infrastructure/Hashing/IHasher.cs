namespace SnapshotDiff.Infrastructure.Hashing;

/// <summary>
/// 
/// </summary>
public interface IHasher
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    string ComputeHash(byte[] data);
}
