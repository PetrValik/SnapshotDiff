using System.Security.Cryptography;

namespace SnapshotDiff.Infrastructure.Hashing;

/// <summary>
/// 
/// </summary>
public sealed class Sha256Hasher : IHasher
{
    public Sha256Hasher() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public string ComputeHash(byte[] data)
    {
        var hashBytes = SHA256.HashData(data);
        return Convert.ToHexStringLower(hashBytes);
    }
}
