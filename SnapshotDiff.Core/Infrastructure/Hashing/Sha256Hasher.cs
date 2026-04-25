using System.Security.Cryptography;

namespace SnapshotDiff.Infrastructure.Hashing;

public sealed class Sha256Hasher : IHasher
{
    public string ComputeHash(byte[] data)
        => Convert.ToHexStringLower(SHA256.HashData(data));
}
