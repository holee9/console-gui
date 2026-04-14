using FluentAssertions;
using HnVue.CDBurning;
using HnVue.Common.Results;
using Xunit;

namespace HnVue.CDBurning.Tests;

/// <summary>
/// Unit tests for <see cref="IMAPIComWrapper"/> — the simulation/stub implementation
/// of <see cref="IBurnSession"/> that also serves as documentation for IMAPI2 behaviour.
/// </summary>
[Trait("SWR", "SWR-CD-020")]
public sealed class IMAPIComWrapperTests
{
    private readonly IMAPIComWrapper _sut = new();

    // ── Initial state ──────────────────────────────────────────────────────────

    [Fact]
    public async Task IsDiscInserted_InitialState_ReturnsFalse()
    {
        var result = await _sut.IsDiscInsertedAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsDiscBlank_InitialState_ReturnsTrue()
    {
        var result = await _sut.IsDiscBlankAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task GetDiscCapacity_InitialState_Returns700MbDefault()
    {
        var result = await _sut.GetDiscCapacityBytesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(700L * 1024 * 1024);
    }

    // ── SimulateDiscInserted ──────────────────────────────────────────────────

    [Fact]
    public async Task SimulateDiscInserted_Default_SetsInsertedAndBlank()
    {
        _sut.SimulateDiscInserted();

        var inserted = await _sut.IsDiscInsertedAsync();
        var blank = await _sut.IsDiscBlankAsync();

        inserted.Value.Should().BeTrue();
        blank.Value.Should().BeTrue();
    }

    [Fact]
    public async Task SimulateDiscInserted_NonBlankDisc_SetsBlankFalse()
    {
        _sut.SimulateDiscInserted(blank: false);

        var blank = await _sut.IsDiscBlankAsync();

        blank.Value.Should().BeFalse();
    }

    [Fact]
    public async Task SimulateDiscInserted_CustomCapacity_ReturnsCustomCapacity()
    {
        const long customCapacity = 4L * 1024 * 1024 * 1024; // 4 GB DVD
        _sut.SimulateDiscInserted(blank: true, capacityBytes: customCapacity);

        var result = await _sut.GetDiscCapacityBytesAsync();

        result.Value.Should().Be(customCapacity);
    }

    // ── BurnFilesAsync — guard conditions ──────────────────────────────────────

    [Fact]
    public async Task BurnFiles_NoDiscInserted_ReturnsBurnFailed()
    {
        var files = new[] { new BurnFileEntry("src/a.dcm", "DICOM\\a.dcm", 100) };

        var result = await _sut.BurnFilesAsync(files, "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.BurnFailed);
        result.ErrorMessage.Should().Contain("No disc");
    }

    [Fact]
    public async Task BurnFiles_DiscNotBlank_ReturnsBurnFailed()
    {
        _sut.SimulateDiscInserted(blank: false);
        var files = new[] { new BurnFileEntry("src/a.dcm", "DICOM\\a.dcm", 100) };

        var result = await _sut.BurnFilesAsync(files, "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.BurnFailed);
        result.ErrorMessage.Should().Contain("not blank");
    }

    [Fact]
    public async Task BurnFiles_NullFiles_ThrowsArgumentNullException()
    {
        _sut.SimulateDiscInserted();

        var act = async () => await _sut.BurnFilesAsync(null!, "LABEL");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task BurnFiles_NullVolumeLabel_ThrowsArgumentNullException()
    {
        _sut.SimulateDiscInserted();
        var files = new[] { new BurnFileEntry("src/a.dcm", "DICOM\\a.dcm", 100) };

        var act = async () => await _sut.BurnFilesAsync(files, null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── BurnFilesAsync — capacity check ────────────────────────────────────────

    [Fact]
    public async Task BurnFiles_FilesExceedDiscCapacity_ReturnsBurnFailed()
    {
        // 10 bytes capacity, file is 1 KB — always exceeds
        _sut.SimulateDiscInserted(blank: true, capacityBytes: 10);
        var files = new[] { new BurnFileEntry("src/big.dcm", "DICOM\\big.dcm", 1024) };

        var result = await _sut.BurnFilesAsync(files, "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.BurnFailed);
        result.ErrorMessage.Should().Contain("capacity");
    }

    // ── BurnFilesAsync — success path ─────────────────────────────────────────

    [Fact]
    public async Task BurnFiles_ValidFiles_ReturnsSuccessAndMarksDiscNotBlank()
    {
        _sut.SimulateDiscInserted();
        var files = new[] { new BurnFileEntry("src/a.dcm", "DICOM\\a.dcm", 100) };

        var result = await _sut.BurnFilesAsync(files, "TEST_LABEL");

        result.IsSuccess.Should().BeTrue();

        // After burning, disc should no longer be blank
        var blankResult = await _sut.IsDiscBlankAsync();
        blankResult.Value.Should().BeFalse();
    }

    [Fact]
    public async Task BurnFiles_EmptyFileList_SucceedsWithNoFilesToBurn()
    {
        _sut.SimulateDiscInserted();

        var result = await _sut.BurnFilesAsync(Array.Empty<BurnFileEntry>(), "EMPTY_LABEL");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BurnFiles_WithProgressCallback_ReportsProgressForEachFile()
    {
        _sut.SimulateDiscInserted();
        var progressReports = new List<double>();
        var progress = new Progress<double>(v => progressReports.Add(v));

        var files = new[]
        {
            new BurnFileEntry("src/a.dcm", "DICOM\\a.dcm", 100),
            new BurnFileEntry("src/b.dcm", "DICOM\\b.dcm", 200),
        };

        var result = await _sut.BurnFilesAsync(files, "LABEL", progress);
        // Allow progress callbacks to be invoked on the thread pool
        // Progress<T> posts to SynchronizationContext or ThreadPool;
        // retry with backoff to handle timing variance in CI/parallel runs.
        var retries = 0;
        while (progressReports.Count < 2 && retries < 20)
        {
            await Task.Delay(50);
            retries++;
        }

        result.IsSuccess.Should().BeTrue();
        progressReports.Should().HaveCount(2);
        progressReports[1].Should().BeApproximately(100.0, precision: 0.1);
    }

    // ── BurnFilesAsync — cancellation ──────────────────────────────────────────

    [Fact]
    public async Task BurnFiles_CancelledToken_ThrowsOperationCanceledException()
    {
        _sut.SimulateDiscInserted();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var files = new[] { new BurnFileEntry("src/a.dcm", "DICOM\\a.dcm", 100) };

        var act = async () => await _sut.BurnFilesAsync(files, "LABEL", cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── VerifyAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Verify_NoDiscInserted_ReturnsDiscVerificationFailed()
    {
        var result = await _sut.VerifyAsync(Array.Empty<BurnFileEntry>());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DiscVerificationFailed);
    }

    [Fact]
    public async Task Verify_DiscInserted_ReturnsTrue()
    {
        _sut.SimulateDiscInserted();

        var result = await _sut.VerifyAsync(Array.Empty<BurnFileEntry>());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Verify_NullExpectedFiles_ThrowsArgumentNullException()
    {
        _sut.SimulateDiscInserted();

        var act = async () => await _sut.VerifyAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Verify_CancelledToken_ThrowsOperationCanceledException()
    {
        _sut.SimulateDiscInserted();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await _sut.VerifyAsync(Array.Empty<BurnFileEntry>(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
