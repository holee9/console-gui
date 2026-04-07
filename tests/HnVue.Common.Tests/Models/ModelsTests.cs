using FluentAssertions;
using HnVue.Common.Configuration;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using Xunit;

namespace HnVue.Common.Tests.Models;

public sealed class ModelsTests
{
    // ── AuditEntry ────────────────────────────────────────────────────────────

    [Fact]
    public void AuditEntry_PrimaryConstructor_SetsAllProperties()
    {
        var ts = DateTimeOffset.UtcNow;
        var entry = new AuditEntry("id-1", ts, "usr-1", "LOGIN", null, null, "sha256abc");

        entry.EntryId.Should().Be("id-1");
        entry.Timestamp.Should().Be(ts);
        entry.UserId.Should().Be("usr-1");
        entry.Action.Should().Be("LOGIN");
        entry.Details.Should().BeNull();
        entry.PreviousHash.Should().BeNull();
        entry.CurrentHash.Should().Be("sha256abc");
    }

    [Fact]
    public void AuditEntry_ConvenienceConstructor_GeneratesEntryId()
    {
        var ts = DateTimeOffset.UtcNow;
        var entry = new AuditEntry(ts, "usr-2", "EXPOSE", "sha256xyz", "details", "prevhash");

        entry.EntryId.Should().NotBeNullOrEmpty();
        entry.CurrentHash.Should().Be("sha256xyz");
        entry.PreviousHash.Should().Be("prevhash");
        entry.Details.Should().Be("details");
    }

    [Fact]
    public void AuditEntry_ConvenienceConstructor_OptionalParams_AreNull()
    {
        var entry = new AuditEntry(DateTimeOffset.UtcNow, "usr-3", "LOGOUT", "sha256def");

        entry.Details.Should().BeNull();
        entry.PreviousHash.Should().BeNull();
    }

    // ── DoseValidationResult ──────────────────────────────────────────────────

    [Fact]
    public void DoseValidationResult_Allow_IsAllowedTrue()
    {
        var result = new DoseValidationResult(true, DoseValidationLevel.Allow, null, 0.0, 0.0, 0.0);

        result.IsAllowed.Should().BeTrue();
        result.Level.Should().Be(DoseValidationLevel.Allow);
        result.Message.Should().BeNull();
    }

    [Fact]
    public void DoseValidationResult_Block_IsAllowedFalse()
    {
        var result = new DoseValidationResult(false, DoseValidationLevel.Block, "Dose exceeds limit", 0.0, 0.0, 0.0);

        result.IsAllowed.Should().BeFalse();
        result.Level.Should().Be(DoseValidationLevel.Block);
        result.Message.Should().Be("Dose exceeds limit");
    }

    // ── WorkflowStateChangedEventArgs ─────────────────────────────────────────

    [Fact]
    public void WorkflowStateChangedEventArgs_SetsProperties()
    {
        var args = new WorkflowStateChangedEventArgs(
            WorkflowState.Idle, WorkflowState.PatientSelected, "patient registered");

        args.PreviousState.Should().Be(WorkflowState.Idle);
        args.NewState.Should().Be(WorkflowState.PatientSelected);
        args.Reason.Should().Be("patient registered");
    }

    [Fact]
    public void WorkflowStateChangedEventArgs_ReasonIsOptional()
    {
        var args = new WorkflowStateChangedEventArgs(WorkflowState.Exposing, WorkflowState.ImageAcquiring);

        args.Reason.Should().BeNull();
    }

    // ── GeneratorStateChangedEventArgs ────────────────────────────────────────

    [Fact]
    public void GeneratorStateChangedEventArgs_SetsProperties()
    {
        var args = new GeneratorStateChangedEventArgs(
            GeneratorState.Idle, GeneratorState.Preparing, "prep started");

        args.PreviousState.Should().Be(GeneratorState.Idle);
        args.NewState.Should().Be(GeneratorState.Preparing);
        args.Reason.Should().Be("prep started");
    }

    [Fact]
    public void GeneratorStateChangedEventArgs_ReasonIsOptional()
    {
        var args = new GeneratorStateChangedEventArgs(GeneratorState.Ready, GeneratorState.Exposing);

        args.Reason.Should().BeNull();
    }

    // ── ProcessedImage ────────────────────────────────────────────────────────

    [Fact]
    public void ProcessedImage_SetsAllProperties()
    {
        var pixels = new byte[4];
        var img = new ProcessedImage(512, 512, 16, pixels, 2048.0, 4096.0, "/tmp/img.dcm");

        img.Width.Should().Be(512);
        img.Height.Should().Be(512);
        img.BitsPerPixel.Should().Be(16);
        img.PixelData.Should().BeSameAs(pixels);
        img.WindowCenter.Should().Be(2048.0);
        img.WindowWidth.Should().Be(4096.0);
        img.FilePath.Should().Be("/tmp/img.dcm");
    }

    [Fact]
    public void ProcessedImage_FilePathIsOptional()
    {
        var img = new ProcessedImage(256, 256, 12, new byte[2], 1024.0, 2048.0);

        img.FilePath.Should().BeNull();
    }

    // ── HnVueOptions ─────────────────────────────────────────────────────────

    [Fact]
    public void HnVueOptions_DefaultValues_AreCorrect()
    {
        var opts = new HnVueOptions();

        opts.Security.SessionTimeoutMinutes.Should().Be(15);
        opts.Security.MaxFailedLoginAttempts.Should().Be(5);
        opts.Security.LockoutDurationMinutes.Should().Be(30);
        opts.Dicom.LocalAeTitle.Should().Be("HNVUE");
        opts.Dicom.ListenPort.Should().Be(104);
    }

    [Fact]
    public void HnVueOptions_SectionName_IsHnVue()
    {
        HnVueOptions.SectionName.Should().Be("HnVue");
    }

    [Fact]
    public void HnVueOptions_CanOverrideSecuritySettings()
    {
        var opts = new HnVueOptions
        {
            Security = new HnVueOptions.SecurityOptions
            {
                SessionTimeoutMinutes = 30,
                MaxFailedLoginAttempts = 3,
                LockoutDurationMinutes = 60,
            }
        };

        opts.Security.SessionTimeoutMinutes.Should().Be(30);
        opts.Security.MaxFailedLoginAttempts.Should().Be(3);
        opts.Security.LockoutDurationMinutes.Should().Be(60);
    }
}
