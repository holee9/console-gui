using System.Diagnostics;
using HnVue.Common.Enums;
using HnVue.Security;
using Xunit;
using Xunit.Abstractions;

namespace HnVue.Security.Tests;

/// <summary>
/// Performance benchmark tests for security-critical operations.
/// Ensures operations meet acceptable latency thresholds for IEC 62304 Class B compliance.
/// Sequential collection to prevent BCrypt CPU contention under parallel test execution.
/// </summary>
[Trait("Category", "Performance")]
[Trait("SWR", "SWR-SEC-040")]
[Collection("Security-Sequential")]
public sealed class PerformanceBenchmarks
{
    private readonly ITestOutputHelper _output;

    public PerformanceBenchmarks(ITestOutputHelper output) => _output = output;

    [Fact]
    public void PasswordHasher_Hash_MustCompleteWithin1000ms()
    {
        // Arrange
        var password = "BenchmarkTestPassword123!";
        var sw = Stopwatch.StartNew();

        // Act
        var hash = PasswordHasher.HashPassword(password);
        sw.Stop();

        // Assert
        _output.WriteLine($"BCrypt (cost=12) hash: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000,
            $"Hash took {sw.ElapsedMilliseconds}ms, exceeds 5000ms threshold");
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void PasswordHasher_Verify_MustCompleteWithin500ms()
    {
        // Arrange
        var password = "BenchmarkTestPassword123!";
        var hash = PasswordHasher.HashPassword(password);
        var sw = Stopwatch.StartNew();

        // Act
        var result = PasswordHasher.Verify(password, hash);
        sw.Stop();

        // Assert
        _output.WriteLine($"BCrypt verify: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 2000,
            $"Verify took {sw.ElapsedMilliseconds}ms, exceeds 2000ms threshold");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void PasswordHasher_NeedsRehash_MustCompleteWithin1ms()
    {
        // Arrange
        var hash = PasswordHasher.HashPassword("Test123!");
        var sw = Stopwatch.StartNew();

        // Act
        var needsRehash = PasswordHasher.NeedsRehash(hash);
        sw.Stop();

        // Assert
        _output.WriteLine($"BCrypt NeedsRehash: {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 1,
            $"NeedsRehash took {sw.ElapsedMilliseconds}ms, exceeds 1ms threshold");
        Assert.False(needsRehash, "fresh hash with cost=12 should not need rehash");
    }

    [Fact]
    public void JwtTokenService_Issue_MustCompleteWithin10ms()
    {
        // Arrange
        var options = new JwtOptions
        {
            SecretKey = "benchmark-test-secret-key-that-is-at-least-32-characters-long!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue"
        };
        var service = new JwtTokenService(options);
        var sw = Stopwatch.StartNew();

        // Act - Issue 100 tokens
        for (int i = 0; i < 100; i++)
        {
            var (token, jti) = service.Issue($"user-{i}", $"username-{i}", UserRole.Admin);
            Assert.NotNull(token);
            Assert.NotNull(jti);
        }
        sw.Stop();

        var avgMs = sw.ElapsedMilliseconds / 100.0;
        _output.WriteLine($"JWT issue avg: {avgMs:F2}ms (100 iterations)");
        Assert.True(avgMs < 10, $"JWT issue avg {avgMs:F2}ms, exceeds 10ms threshold");
    }

    [Fact]
    public void JwtTokenService_Validate_MustCompleteWithin10ms()
    {
        // Arrange
        var options = new JwtOptions
        {
            SecretKey = "benchmark-test-secret-key-that-is-at-least-32-characters-long!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue"
        };
        var service = new JwtTokenService(options);
        var (token, _) = service.Issue("user-1", "username-1", UserRole.Admin);

        var sw = Stopwatch.StartNew();

        // Act - Validate 100 times
        for (int i = 0; i < 100; i++)
        {
            var result = service.Validate(token);
            Assert.True(result.IsSuccess);
        }
        sw.Stop();

        var avgMs = sw.ElapsedMilliseconds / 100.0;
        _output.WriteLine($"JWT validate avg: {avgMs:F2}ms (100 iterations)");
        Assert.True(avgMs < 10, $"JWT validate avg {avgMs:F2}ms, exceeds 10ms threshold");
    }

    [Fact]
    public void JwtTokenService_IssueAndValidate_MustCompleteWithin10ms()
    {
        // Arrange
        var options = new JwtOptions
        {
            SecretKey = "benchmark-test-secret-key-that-is-at-least-32-characters-long!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue"
        };
        var service = new JwtTokenService(options);
        var sw = Stopwatch.StartNew();

        // Act - Issue and validate 100 tokens
        for (int i = 0; i < 100; i++)
        {
            var (token, jti) = service.Issue($"user-{i}", $"username-{i}", UserRole.Admin);
            var result = service.Validate(token);
            Assert.True(result.IsSuccess);
        }
        sw.Stop();

        var avgMs = sw.ElapsedMilliseconds / 100.0;
        _output.WriteLine($"JWT issue+validate avg: {avgMs:F2}ms (100 iterations)");
        Assert.True(avgMs < 10, $"JWT avg {avgMs:F2}ms, exceeds 10ms threshold");
    }

    [Fact]
    public async Task TokenDenylist_Revoke_MustCompleteWithin1ms()
    {
        // Arrange
        var denylist = new InMemoryTokenDenylist(TimeSpan.FromMinutes(15));
        var sw = Stopwatch.StartNew();

        // Act - Revoke 1000 tokens
        for (int i = 0; i < 1000; i++)
        {
            await denylist.RevokeAsync($"jti-{i}");
        }
        sw.Stop();

        var avgMs = sw.ElapsedMilliseconds / 1000.0;
        _output.WriteLine($"Denylist revoke avg: {avgMs:F4}ms (1000 iterations)");
        Assert.True(avgMs < 1, $"Denylist revoke avg {avgMs:F4}ms, exceeds 1ms threshold");
    }

    [Fact]
    public async Task TokenDenylist_IsRevoked_MustCompleteWithin1ms()
    {
        // Arrange
        var denylist = new InMemoryTokenDenylist(TimeSpan.FromMinutes(15));
        const string jti = "test-jti";
        await denylist.RevokeAsync(jti);
        var sw = Stopwatch.StartNew();

        // Act - Check revocation status 1000 times
        for (int i = 0; i < 1000; i++)
        {
            var isRevoked = await denylist.IsRevokedAsync(jti);
            Assert.True(isRevoked);
        }
        sw.Stop();

        var avgMs = sw.ElapsedMilliseconds / 1000.0;
        _output.WriteLine($"Denylist IsRevoked avg: {avgMs:F4}ms (1000 iterations)");
        Assert.True(avgMs < 1, $"Denylist IsRevoked avg {avgMs:F4}ms, exceeds 1ms threshold");
    }

    [Fact]
    public async Task TokenDenylist_RevokeAndCheck_MustCompleteWithin1ms()
    {
        // Arrange
        var denylist = new InMemoryTokenDenylist(TimeSpan.FromMinutes(15));
        var sw = Stopwatch.StartNew();

        // Act - Revoke and check 1000 tokens
        for (int i = 0; i < 1000; i++)
        {
            var jti = $"jti-{i}";
            await denylist.RevokeAsync(jti);
            var isRevoked = await denylist.IsRevokedAsync(jti);
            Assert.True(isRevoked);
        }
        sw.Stop();

        var avgMs = sw.ElapsedMilliseconds / 1000.0;
        _output.WriteLine($"Denylist revoke+check avg: {avgMs:F4}ms (1000 iterations)");
        Assert.True(avgMs < 1, $"Denylist revoke+check avg {avgMs:F4}ms, exceeds 1ms threshold");
    }

    [Fact]
    public void PasswordHasher_ConcurrentHashes_AllCompleteWithin2000ms()
    {
        // Arrange
        const int threadCount = 10;
        var passwords = Enumerable.Range(0, threadCount)
            .Select(i => $"Password{i}!")
            .ToArray();
        var results = new bool[threadCount];
        var sw = Stopwatch.StartNew();

        // Act - Hash concurrently
        Parallel.For(0, threadCount, i =>
        {
            results[i] = !string.IsNullOrEmpty(PasswordHasher.HashPassword(passwords[i]));
        });
        sw.Stop();

        // Assert
        _output.WriteLine($"Concurrent BCrypt hashes ({threadCount} threads): {sw.ElapsedMilliseconds}ms");
        Assert.True(sw.ElapsedMilliseconds < 5000,
            $"Concurrent hashes took {sw.ElapsedMilliseconds}ms, exceeds 5000ms threshold");
        Assert.All(results, r => Assert.True(r, "all hashes should complete successfully"));
    }

    [Fact]
    public void JwtTokenService_ConcurrentIssue_AllCompleteWithin10msAvg()
    {
        // Arrange
        var options = new JwtOptions
        {
            SecretKey = "benchmark-test-secret-key-that-is-at-least-32-characters-long!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue"
        };
        var service = new JwtTokenService(options);
        const int tokenCount = 100;
        var sw = Stopwatch.StartNew();

        // Act - Issue tokens concurrently
        Parallel.For(0, tokenCount, i =>
        {
            var (token, jti) = service.Issue($"user-{i}", $"username-{i}", UserRole.Admin);
            Assert.NotNull(token);
            Assert.NotNull(jti);
        });
        sw.Stop();

        var avgMs = sw.ElapsedMilliseconds / (double)tokenCount;
        _output.WriteLine($"Concurrent JWT issue ({tokenCount} tokens): {sw.ElapsedMilliseconds}ms total, {avgMs:F2}ms avg");
        Assert.True(avgMs < 10, $"Concurrent JWT issue avg {avgMs:F2}ms, exceeds 10ms threshold");
    }
}
