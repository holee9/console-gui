using System.IO;
using FluentAssertions;
using HnVue.Security;
using Xunit;
using Path = System.IO.Path;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace HnVue.Security.Tests;

/// <summary>
/// Tests for <see cref="PersistentTokenDenylist"/> implementing IEC 62304 compliance (durable token revocation).
/// Sequential collection to prevent file I/O and timing flakiness under parallel execution.
/// </summary>
[Collection("Security-Sequential")]
public class PersistentTokenDenylistTests : IDisposable
{
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(15);
    private readonly string _tempDir;
    private readonly string _persistPath;
    private readonly PersistentTokenDenylist _denylist;

    public PersistentTokenDenylistTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        _persistPath = Path.Combine(_tempDir, "denylist.json");
        _denylist = new PersistentTokenDenylist(_defaultTtl, _persistPath);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task RevokeAsync_IsRevokedAsync_WorksRoundTrip()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();

        // Act
        await _denylist.RevokeAsync(jti, cancellationToken: CancellationToken.None);
        var isRevoked = await _denylist.IsRevokedAsync(jti, CancellationToken.None);

        // Assert
        isRevoked.Should().BeTrue("Token should be revoked after RevokeAsync call.");
    }

    [Fact]
    public async Task RevokeAsync_WithExpiredEntries_CleansUpOnNextRevoke()
    {
        // Arrange
        var expiredJti = Guid.NewGuid().ToString();
        var validJti = Guid.NewGuid().ToString();
        var shortTtl = TimeSpan.FromMilliseconds(50);

        // Act - Add expired and valid entries
        await _denylist.RevokeAsync(expiredJti, shortTtl, CancellationToken.None);
        await _denylist.RevokeAsync(validJti, _defaultTtl, CancellationToken.None);

        // Wait for expiry
        await Task.Delay(100);

        // Trigger cleanup by revoking another token
        await _denylist.RevokeAsync(Guid.NewGuid().ToString(), TimeSpan.Zero, CancellationToken.None);

        // Assert
        var isExpiredRevoked = await _denylist.IsRevokedAsync(expiredJti, CancellationToken.None);
        var isValidRevoked = await _denylist.IsRevokedAsync(validJti, CancellationToken.None);

        isExpiredRevoked.Should().BeFalse("Expired entry should be cleaned up.");
        isValidRevoked.Should().BeTrue("Valid entry should still be revoked.");
    }

    [Fact]
    public async Task Persistence_SurvivesSimulatedRestart()
    {
        // Arrange
        var jti1 = Guid.NewGuid().ToString();
        var jti2 = Guid.NewGuid().ToString();
        var jti3 = Guid.NewGuid().ToString();

        // Act - Create first instance, revoke tokens
        var firstDenylist = new PersistentTokenDenylist(_defaultTtl, _persistPath);
        await firstDenylist.RevokeAsync(jti1, cancellationToken: CancellationToken.None);
        await firstDenylist.RevokeAsync(jti2, cancellationToken: CancellationToken.None);
        await firstDenylist.RevokeAsync(jti3, TimeSpan.FromMilliseconds(50), CancellationToken.None); // Short TTL

        // Wait for jti3 to expire
        await Task.Delay(100);

        // Create second instance (simulates restart)
        var secondDenylist = new PersistentTokenDenylist(_defaultTtl, _persistPath);

        // Assert - jti1 and jti2 should be persisted, jti3 expired
        var isJti1Revoked = await secondDenylist.IsRevokedAsync(jti1, CancellationToken.None);
        var isJti2Revoked = await secondDenylist.IsRevokedAsync(jti2, CancellationToken.None);
        var isJti3Revoked = await secondDenylist.IsRevokedAsync(jti3, CancellationToken.None);

        isJti1Revoked.Should().BeTrue("jti1 should be persisted across restart.");
        isJti2Revoked.Should().BeTrue("jti2 should be persisted across restart.");
        isJti3Revoked.Should().BeFalse("jti3 should be expired and not loaded.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task RevokeAsync_WithInvalidJti_ThrowsArgumentException(string? invalidJti)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _denylist.RevokeAsync(invalidJti!, cancellationToken: CancellationToken.None);
        });

        exception.Message.Should().Contain("JTI cannot be null, empty, or whitespace.");
        exception.ParamName.Should().Be("jti");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task IsRevokedAsync_WithInvalidJti_ThrowsArgumentException(string? invalidJti)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _denylist.IsRevokedAsync(invalidJti!, CancellationToken.None);
        });

        exception.Message.Should().Contain("JTI cannot be null, empty, or whitespace.");
        exception.ParamName.Should().Be("jti");
    }

    [Fact]
    public async Task RevokeAsync_ConcurrentCalls_ThreadSafe()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        var tasks = new List<Task>();

        // Act - Revoke the same JTI from multiple threads
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(_denylist.RevokeAsync(jti, TimeSpan.FromMinutes(1), CancellationToken.None));
        }
        await Task.WhenAll(tasks);

        // Assert
        var isRevoked = await _denylist.IsRevokedAsync(jti, CancellationToken.None);
        isRevoked.Should().BeTrue("Concurrent revokes should result in revoked token.");
    }

    [Fact]
    public async Task RevokeAsync_WithCustomTtl_UsesProvidedTtl()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        var shortTtl = TimeSpan.FromMilliseconds(100);

        // Act
        await _denylist.RevokeAsync(jti, shortTtl, CancellationToken.None);
        var isRevokedImmediately = await _denylist.IsRevokedAsync(jti, CancellationToken.None);

        // Assert
        isRevokedImmediately.Should().BeTrue("Token should be revoked immediately.");

        await Task.Delay(150); // Wait for TTL to expire
        var isRevokedAfterExpiry = await _denylist.IsRevokedAsync(jti, CancellationToken.None);
        isRevokedAfterExpiry.Should().BeFalse("Token should not be revoked after TTL expiry.");
    }

    [Fact]
    public async Task LoadFromFile_WithCorruptJson_HandlesGracefully()
    {
        // Arrange - Create a corrupt JSON file
        Directory.CreateDirectory(_tempDir);
        File.WriteAllText(_persistPath, "{ invalid json content");

        // Act - Creating denylist with corrupt file should not throw
        var denylist = new PersistentTokenDenylist(_defaultTtl, _persistPath);
        var jti = Guid.NewGuid().ToString();
        await denylist.RevokeAsync(jti);

        // Assert - Denylist should work normally despite corrupt load
        var isRevoked = await denylist.IsRevokedAsync(jti, CancellationToken.None);
        isRevoked.Should().BeTrue("Denylist should function after corrupt file load.");
    }

    [Fact]
    public async Task PersistToFile_WithUnavailablePath_HandlesGracefully()
    {
        // Arrange - Create denylist with invalid path (e.g., reserved Windows filename)
        var denylist = new PersistentTokenDenylist(_defaultTtl, "C:\\CON\\denylist.json");
        var jti = Guid.NewGuid().ToString();

        // Act - Should not throw despite invalid path
        await denylist.RevokeAsync(jti, null, CancellationToken.None);

        // Assert - In-memory denylist should still work
        var isRevoked = await denylist.IsRevokedAsync(jti, CancellationToken.None);
        isRevoked.Should().BeTrue("Denylist should work in-memory even if file persistence fails.");
    }

    [Fact]
    public async Task Persistence_WithDefaultPath_UsesAppDataLocation()
    {
        // Act - Create denylist with default (null) path
        var denylist = new PersistentTokenDenylist(_defaultTtl);
        var jti = Guid.NewGuid().ToString();
        await denylist.RevokeAsync(jti, null, CancellationToken.None);

        // Assert - Should work without exception
        var isRevoked = await denylist.IsRevokedAsync(jti, CancellationToken.None);
        isRevoked.Should().BeTrue("Denylist with default path should work correctly.");
    }

    [Fact]
    public async Task RevokeAsync_MultipleJtIs_IndependentRevocation()
    {
        // Arrange
        var jti1 = Guid.NewGuid().ToString();
        var jti2 = Guid.NewGuid().ToString();
        var jti3 = Guid.NewGuid().ToString();

        // Act
        await _denylist.RevokeAsync(jti1, cancellationToken: CancellationToken.None);
        await _denylist.RevokeAsync(jti2, cancellationToken: CancellationToken.None);
        // jti3 is not revoked

        // Assert
        (await _denylist.IsRevokedAsync(jti1, CancellationToken.None)).Should().BeTrue();
        (await _denylist.IsRevokedAsync(jti2, CancellationToken.None)).Should().BeTrue();
        (await _denylist.IsRevokedAsync(jti3, CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task CleanupExpiredEntries_WithMultipleExpiredEntries_RemovesAll()
    {
        // Arrange
        var expiredJtis = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            var jti = Guid.NewGuid().ToString();
            expiredJtis.Add(jti);
            await _denylist.RevokeAsync(jti, TimeSpan.FromMilliseconds(10), CancellationToken.None);
        }

        var validJti = Guid.NewGuid().ToString();
        await _denylist.RevokeAsync(validJti, TimeSpan.FromMinutes(1), CancellationToken.None);

        // Act
        await Task.Delay(100); // Wait for short TTL to expire
        await _denylist.RevokeAsync(Guid.NewGuid().ToString(), TimeSpan.Zero, CancellationToken.None); // Trigger cleanup

        // Assert
        foreach (var expiredJti in expiredJtis)
        {
            var isRevoked = await _denylist.IsRevokedAsync(expiredJti, CancellationToken.None);
            isRevoked.Should().BeFalse($"Expired JTI {expiredJti} should be removed.");
        }

        (await _denylist.IsRevokedAsync(validJti, CancellationToken.None)).Should()
            .BeTrue("Valid JTI should still be revoked.");
    }
}
