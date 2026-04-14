using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SnapshotDiff.Features.Trash.Domain;
using SnapshotDiff.Infrastructure.Storage;

namespace SnapshotDiff.Features.Trash.Infrastructure;

public sealed class SqliteTrashRepository(IStoragePathProvider storagePathProvider) : ITrashRepository, IDisposable
{
    private readonly SqliteConnection _connection = OpenAndInit(storagePathProvider);
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static SqliteConnection OpenAndInit(IStoragePathProvider provider)
    {
        var dbPath = Path.Combine(provider.AppDataDirectory, "trash.db");
        Directory.CreateDirectory(provider.AppDataDirectory);
        var conn = new SqliteConnection($"Data Source={dbPath}");
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS TrashItems (
                Id           TEXT PRIMARY KEY,
                OriginalPath TEXT NOT NULL,
                Name         TEXT NOT NULL,
                DeletedAt    TEXT NOT NULL,
                ExpiresAt    TEXT NOT NULL,
                IsDirectory  INTEGER NOT NULL,
                SizeBytes    INTEGER NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
        return conn;
    }

    public async Task InsertAsync(TrashItemMeta meta, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            await using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                INSERT INTO TrashItems (Id, OriginalPath, Name, DeletedAt, ExpiresAt, IsDirectory, SizeBytes)
                VALUES (@id, @originalPath, @name, @deletedAt, @expiresAt, @isDirectory, @sizeBytes);
                """;
            cmd.Parameters.AddWithValue("@id", meta.Id);
            cmd.Parameters.AddWithValue("@originalPath", meta.OriginalPath);
            cmd.Parameters.AddWithValue("@name", meta.Name);
            cmd.Parameters.AddWithValue("@deletedAt", meta.DeletedAt.ToString("O"));
            cmd.Parameters.AddWithValue("@expiresAt", meta.ExpiresAt.ToString("O"));
            cmd.Parameters.AddWithValue("@isDirectory", meta.IsDirectory ? 1 : 0);
            cmd.Parameters.AddWithValue("@sizeBytes", meta.SizeBytes);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        finally { _lock.Release(); }
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            await using var cmd = _connection.CreateCommand();
            cmd.CommandText = "DELETE FROM TrashItems WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        finally { _lock.Release(); }
    }

    public async Task<TrashItemMeta?> GetAsync(string id, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            await using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM TrashItems WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await reader.ReadAsync(ct) ? ReadMeta(reader) : null;
        }
        finally { _lock.Release(); }
    }

    public async Task<IReadOnlyList<TrashItemMeta>> GetAllAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            await using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM TrashItems ORDER BY DeletedAt DESC;";
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var results = new List<TrashItemMeta>();
            while (await reader.ReadAsync(ct))
                results.Add(ReadMeta(reader));
            return results;
        }
        finally { _lock.Release(); }
    }

    public async Task<IReadOnlyList<TrashItemMeta>> GetExpiredAsync(DateTime now, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            await using var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM TrashItems WHERE ExpiresAt < @now;";
            cmd.Parameters.AddWithValue("@now", now.ToString("O"));
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var results = new List<TrashItemMeta>();
            while (await reader.ReadAsync(ct))
                results.Add(ReadMeta(reader));
            return results;
        }
        finally { _lock.Release(); }
    }

    public async Task DeleteAllAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            await using var cmd = _connection.CreateCommand();
            cmd.CommandText = "DELETE FROM TrashItems;";
            await cmd.ExecuteNonQueryAsync(ct);
        }
        finally { _lock.Release(); }
    }

    private static TrashItemMeta ReadMeta(SqliteDataReader reader) => new()
    {
        Id = reader.GetString(0),
        OriginalPath = reader.GetString(1),
        Name = reader.GetString(2),
        DeletedAt = DateTime.Parse(reader.GetString(3), null, System.Globalization.DateTimeStyles.RoundtripKind),
        ExpiresAt = DateTime.Parse(reader.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind),
        IsDirectory = reader.GetInt64(5) != 0,
        SizeBytes = reader.GetInt64(6),
    };

    public void Dispose()
    {
        _lock.Dispose();
        _connection.Dispose();
    }
}
