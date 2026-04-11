using FluentAssertions;
using HnVue.Update;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// Coverage tests for <see cref="UpdateOptions"/>.
/// Targets Validate(), ResolvedBackupDirectory, ResolvedApplicationDirectory.
/// </summary>
public sealed class UpdateOptionsCoverageTests
{
    // ── Validate ─────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyUrl_ThrowsInvalidOperationException()
    {
        var options = new UpdateOptions { UpdateServerUrl = string.Empty, RequireAuthenticodeSignature = false };
        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Validate_WhitespaceUrl_ThrowsInvalidOperationException()
    {
        var options = new UpdateOptions { UpdateServerUrl = "   ", RequireAuthenticodeSignature = false };
        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Validate_HttpUrl_ThrowsInvalidOperationException()
    {
        var options = new UpdateOptions { UpdateServerUrl = "http://update.example.com", RequireAuthenticodeSignature = false };
        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*HTTPS*");
    }

    [Fact]
    public void Validate_FtpUrl_ThrowsInvalidOperationException()
    {
        var options = new UpdateOptions { UpdateServerUrl = "ftp://update.example.com", RequireAuthenticodeSignature = false };
        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*valid HTTPS*");
    }

    [Fact]
    public void Validate_ValidHttpsUrl_DoesNotThrow()
    {
        var options = new UpdateOptions { UpdateServerUrl = "https://update.example.com", RequireAuthenticodeSignature = false };
        var act = () => options.Validate();

        act.Should().NotThrow();
    }

    // ── ResolvedBackupDirectory ──────────────────────────────────────────────

    [Fact]
    public void ResolvedBackupDirectory_WhenSet_ReturnsValue()
    {
        var options = new UpdateOptions { BackupDirectory = "/custom/backup" };

        options.ResolvedBackupDirectory.Should().Be("/custom/backup");
    }

    [Fact]
    public void ResolvedBackupDirectory_WhenEmpty_ReturnsDefaultPath()
    {
        var options = new UpdateOptions { BackupDirectory = string.Empty };

        options.ResolvedBackupDirectory.Should().Contain("HnVue");
        options.ResolvedBackupDirectory.Should().Contain("backup");
    }

    [Fact]
    public void ResolvedBackupDirectory_WhenNull_ReturnsDefaultPath()
    {
        var options = new UpdateOptions { BackupDirectory = null! };

        options.ResolvedBackupDirectory.Should().Contain("HnVue");
    }

    // ── ResolvedApplicationDirectory ─────────────────────────────────────────

    [Fact]
    public void ResolvedApplicationDirectory_WhenSet_ReturnsValue()
    {
        var options = new UpdateOptions { ApplicationDirectory = "/custom/app" };

        options.ResolvedApplicationDirectory.Should().Be("/custom/app");
    }

    [Fact]
    public void ResolvedApplicationDirectory_WhenEmpty_ReturnsBaseDirectory()
    {
        var options = new UpdateOptions { ApplicationDirectory = string.Empty };

        options.ResolvedApplicationDirectory.Should().Be(AppContext.BaseDirectory);
    }

    // ── Defaults ─────────────────────────────────────────────────────────────

    [Fact]
    public void Default_CurrentVersion_Is100()
    {
        var options = new UpdateOptions();
        options.CurrentVersion.Should().Be("1.0.0");
    }

    [Fact]
    public void Default_RequireAuthenticodeSignature_IsTrue()
    {
        var options = new UpdateOptions();
        options.RequireAuthenticodeSignature.Should().BeTrue();
    }
}
