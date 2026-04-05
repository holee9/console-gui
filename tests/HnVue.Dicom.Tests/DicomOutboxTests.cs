using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Results;
using HnVue.Dicom;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Unit tests for <see cref="DicomOutbox"/> covering enqueue behavior,
/// processing logic, retry, and cancellation.
/// </summary>
public sealed class DicomOutboxTests
{
    private static (DicomOutbox Outbox, IDicomService MockService) CreateOutbox()
    {
        var mockService = Substitute.For<IDicomService>();
        var outbox = new DicomOutbox(mockService, NullLogger<DicomOutbox>.Instance);
        return (outbox, mockService);
    }

    // ── EnqueueAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task EnqueueAsync_AddsItemToOutbox()
    {
        var (outbox, _) = CreateOutbox();

        await outbox.EnqueueAsync("file1.dcm");

        outbox.Count.Should().Be(1);
    }

    [Fact]
    public async Task EnqueueAsync_MultipleItems_AllAdded()
    {
        var (outbox, _) = CreateOutbox();

        await outbox.EnqueueAsync("file1.dcm");
        await outbox.EnqueueAsync("file2.dcm");
        await outbox.EnqueueAsync("file3.dcm");

        outbox.Count.Should().Be(3);
    }

    [Fact]
    public void EnqueueAsync_EmptyPath_ThrowsArgumentException()
    {
        var (outbox, _) = CreateOutbox();

        var act = async () => await outbox.EnqueueAsync(string.Empty);

        act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void EnqueueAsync_WhitespacePath_ThrowsArgumentException()
    {
        var (outbox, _) = CreateOutbox();

        var act = async () => await outbox.EnqueueAsync("   ");

        act.Should().ThrowAsync<ArgumentException>();
    }

    // ── ProcessAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_EmptyOutbox_DoesNothing()
    {
        var (outbox, mockService) = CreateOutbox();

        await outbox.ProcessAsync("PACS");

        await mockService.DidNotReceive().StoreAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        outbox.Count.Should().Be(0);
    }

    [Fact]
    public async Task ProcessAsync_SuccessfulStore_ClearsItem()
    {
        var (outbox, mockService) = CreateOutbox();
        mockService.StoreAsync("file1.dcm", "PACS", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        await outbox.EnqueueAsync("file1.dcm");
        await outbox.ProcessAsync("PACS");

        outbox.Count.Should().Be(0);
        await mockService.Received(1).StoreAsync("file1.dcm", "PACS", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_MultipleItems_AllProcessed()
    {
        var (outbox, mockService) = CreateOutbox();
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        await outbox.EnqueueAsync("a.dcm");
        await outbox.EnqueueAsync("b.dcm");
        await outbox.ProcessAsync("PACS");

        outbox.Count.Should().Be(0);
        await mockService.Received(2).StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_PermanentFailure_DiscardsItem()
    {
        var (outbox, mockService) = CreateOutbox();
        // Always fail → retries exhausted → dead-letter discard
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DicomStoreFailed, "server down"));

        await outbox.EnqueueAsync("bad.dcm");

        // No exception should propagate; item should be discarded after retries.
        await outbox.ProcessAsync("PACS");

        outbox.Count.Should().Be(0);
        // Called once for the initial attempt plus up to 3 retries = 4 total.
        await mockService.Received(4).StoreAsync("bad.dcm", "PACS", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_Cancellation_RequeuesCurrentItem()
    {
        var (outbox, mockService) = CreateOutbox();
        using var cts = new CancellationTokenSource();

        // Cancel immediately when StoreAsync is called.
        mockService.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Result>(new OperationCanceledException()));

        await cts.CancelAsync();
        await outbox.EnqueueAsync("x.dcm");

        // Should not throw; item should be re-queued.
        await outbox.ProcessAsync("PACS", cts.Token);

        outbox.Count.Should().Be(1);
    }

    // ── Count property ────────────────────────────────────────────────────────

    [Fact]
    public void Count_InitiallyZero()
    {
        var (outbox, _) = CreateOutbox();
        outbox.Count.Should().Be(0);
    }
}
