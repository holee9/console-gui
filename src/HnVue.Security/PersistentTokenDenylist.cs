using System.Collections.Concurrent;
using System.Text.Json;
using System.IO;
using Microsoft.Extensions.Logging;

namespace HnVue.Security;

/// <summary>
/// Persistent token denylist that survives application restarts.
/// Persists revoked JTIs to a JSON file for IEC 62304 medical device compliance.
/// SWR-CS-077: Token revocation must be durable across restarts.
/// </summary>
internal sealed class PersistentTokenDenylist : ITokenDenylist, IDisposable
{
    // @MX:NOTE DenylistEntries - ConcurrentDictionary for thread-safe JTI revocation tracking with expiration
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly ConcurrentDictionary<string, DateTimeOffset> _denylist = new();
    private readonly TimeSpan _defaultTtl;
    private readonly string _persistPath;
    private readonly object _fileLock = new();
    private readonly ILogger<PersistentTokenDenylist>? _logger;

    /// <summary>
    /// Initializes a new instance with the default TTL and optional persistence path.
    /// </summary>
    /// <param name="defaultTtl">Default time-to-live for revocation entries (typically token expiry duration).</param>
    /// <param name="persistPath">
    /// Optional file path for persistence. Defaults to %APPDATA%\HnVue\token_denylist.json.
    /// </param>
    /// <param name="logger">Optional logger for diagnostics (REQ-SEC-003).</param>
    public PersistentTokenDenylist(TimeSpan defaultTtl, string? persistPath = null, ILogger<PersistentTokenDenylist>? logger = null)
    {
        _defaultTtl = defaultTtl;
        _persistPath = persistPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HnVue", "token_denylist.json");
        _logger = logger;
        LoadFromFile();
    }

    /// <inheritdoc/>
    public Task RevokeAsync(string jti, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
            throw new ArgumentException("JTI cannot be null, empty, or whitespace.", nameof(jti));

        var expiryTime = DateTimeOffset.UtcNow.Add(ttl ?? _defaultTtl);
        _denylist.AddOrUpdate(jti, expiryTime, (_, _) => expiryTime);
        CleanupExpiredEntries();
        PersistToFile();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> IsRevokedAsync(string jti, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
            throw new ArgumentException("JTI cannot be null, empty, or whitespace.", nameof(jti));

        if (!_denylist.TryGetValue(jti, out var expiryTime))
            return Task.FromResult(false);

        // Lazy expiration check
        if (DateTimeOffset.UtcNow >= expiryTime)
        {
            _denylist.TryRemove(jti, out _);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    // @MX:NOTE CleanupExpiredEntries - Lazy cleanup strategy to prevent unbounded growth of denylist
    private void CleanupExpiredEntries()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var kvp in _denylist)
        {
            if (now >= kvp.Value)
                _denylist.TryRemove(kvp.Key, out _);
        }
    }

    /// <summary>
    /// Loads revoked JTIs from the persistence file on startup.
    /// Ignores file errors gracefully - denylist operates in-memory if file is unavailable.
    /// REQ-SEC-003: Logs warning when file corruption is detected.
    /// </summary>
    private void LoadFromFile()
    {
        try
        {
            if (!File.Exists(_persistPath)) return;

            var json = File.ReadAllText(_persistPath);
            var entries = JsonSerializer.Deserialize<Dictionary<string, DateTimeOffset>>(json, JsonOptions);
            if (entries is null) return;

            // Filter out expired entries on load
            var now = DateTimeOffset.UtcNow;
            foreach (var kvp in entries)
            {
                if (now < kvp.Value)
                    _denylist[kvp.Key] = kvp.Value;
            }
        }
        catch (IOException ex)
        {
            // REQ-SEC-003: Log warning for file I/O errors
            _logger?.LogWarning(ex, "Token denylist file I/O error: {Message}. Starting with empty denylist.", ex.Message);
        }
        catch (JsonException ex)
        {
            // REQ-SEC-003: Log warning for JSON corruption
            _logger?.LogWarning(ex, "Token denylist file corrupted: {Message}. Starting with empty denylist.", ex.Message);
        }
    }

    /// <summary>
    /// Persists current denylist state to file.
    /// Non-fatal if persistence fails - denylist continues working in-memory.
    /// </summary>
    private void PersistToFile()
    {
        try
        {
            lock (_fileLock)
            {
                var dir = Path.GetDirectoryName(_persistPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonSerializer.Serialize(_denylist, JsonOptions);
                File.WriteAllText(_persistPath, json);
            }
        }
        catch (IOException)
        {
            // Non-fatal: denylist works in-memory even if persist fails
        }
    }

    /// <summary>
    /// Persists the current denylist state and releases resources.
    /// </summary>
    public void Dispose()
    {
        // Persist final state before disposal
        PersistToFile();
    }
}
