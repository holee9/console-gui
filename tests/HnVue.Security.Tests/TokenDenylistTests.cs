using HnVue.Security;
using Xunit;

namespace HnVue.Security.Tests;

/// <summary>
/// Tests for <see cref="InMemoryTokenDenylist"/> implementing SWR-CS-077 (Concurrent Session Handling).
/// Sequential collection to prevent timing-dependent test flakiness under parallel execution.
/// </summary>
[Collection("Security-Sequential")]
public class TokenDenylistTests
{
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(15);
    private readonly InMemoryTokenDenylist _denylist;

    public TokenDenylistTests()
    {
        _denylist = new InMemoryTokenDenylist(_defaultTtl);
    }

    [Fact]
    public async Task RevokeAsync_WithValidJti_AddsToDenylist()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();

        // Act
        await _denylist.RevokeAsync(jti, cancellationToken: CancellationToken.None);

        // Assert
        var isRevoked = await _denylist.IsRevokedAsync(jti, CancellationToken.None);
        Assert.True(isRevoked, "Token should be revoked after RevokeAsync call.");
    }

    [Fact]
    public async Task IsRevokedAsync_WithNonExistentJti_ReturnsFalse()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();

        // Act
        var isRevoked = await _denylist.IsRevokedAsync(jti, CancellationToken.None);

        // Assert
        Assert.False(isRevoked, "Non-existent JTI should not be revoked.");
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
        Assert.True(isRevokedImmediately, "Token should be revoked immediately.");
        await Task.Delay(150); // Wait for TTL to expire
        var isRevokedAfterExpiry = await _denylist.IsRevokedAsync(jti, CancellationToken.None);
        Assert.False(isRevokedAfterExpiry, "Token should not be revoked after TTL expiry.");
    }

    [Fact]
    public async Task RevokeAsync_WithNullTtl_UsesDefaultTtl()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();

        // Act
        await _denylist.RevokeAsync(jti, null, CancellationToken.None);
        var isRevoked = await _denylist.IsRevokedAsync(jti, CancellationToken.None);

        // Assert
        Assert.True(isRevoked, "Token should be revoked with default TTL.");
    }

    [Fact]
    public async Task RevokeAsync_WithSameJtiMultipleTimes_UpdatesExpiryTime()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        var firstTtl = TimeSpan.FromMinutes(5);
        var secondTtl = TimeSpan.FromMinutes(10);

        // Act
        await _denylist.RevokeAsync(jti, firstTtl, CancellationToken.None);
        await Task.Delay(100); // Small delay to ensure different timestamps
        await _denylist.RevokeAsync(jti, secondTtl, CancellationToken.None);

        // Assert
        var isRevoked = await _denylist.IsRevokedAsync(jti, CancellationToken.None);
        Assert.True(isRevoked, "Token should still be revoked after updating expiry.");
    }

    [Fact]
    public async Task IsRevokedAsync_WithExpiredEntry_RemovesEntry()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        var shortTtl = TimeSpan.FromMilliseconds(50);
        await _denylist.RevokeAsync(jti, shortTtl, CancellationToken.None);

        // Act
        await Task.Delay(100); // Wait for TTL to expire
        await _denylist.IsRevokedAsync(jti, CancellationToken.None); // Triggers lazy cleanup

        // Assert
        var isRevokedAfterCleanup = await _denylist.IsRevokedAsync(jti, CancellationToken.None);
        Assert.False(isRevokedAfterCleanup, "Expired entry should be removed.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task RevokeAsync_WithInvalidJti_ThrowsArgumentException(string? invalidJti)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _denylist.RevokeAsync(invalidJti!, cancellationToken: CancellationToken.None);
        });
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task IsRevokedAsync_WithInvalidJti_ThrowsArgumentException(string? invalidJti)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _denylist.IsRevokedAsync(invalidJti!, CancellationToken.None);
        });
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
        Assert.True(isRevoked, "Concurrent revokes should result in revoked token.");
    }

    [Fact]
    public async Task IsRevokedAsync_ConcurrentCalls_ThreadSafe()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        await _denylist.RevokeAsync(jti, TimeSpan.FromMinutes(1), CancellationToken.None);
        var tasks = new List<Task<bool>>();

        // Act - Check revocation status from multiple threads
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(_denylist.IsRevokedAsync(jti, CancellationToken.None));
        }
        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, result => Assert.True(result, "All concurrent checks should return true."));
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
        Assert.True(await _denylist.IsRevokedAsync(jti1, CancellationToken.None));
        Assert.True(await _denylist.IsRevokedAsync(jti2, CancellationToken.None));
        Assert.False(await _denylist.IsRevokedAsync(jti3, CancellationToken.None));
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
            Assert.False(await _denylist.IsRevokedAsync(expiredJti, CancellationToken.None),
                $"Expired JTI {expiredJti} should be removed.");
        }
        Assert.True(await _denylist.IsRevokedAsync(validJti, CancellationToken.None),
            "Valid JTI should still be revoked.");
    }
}
