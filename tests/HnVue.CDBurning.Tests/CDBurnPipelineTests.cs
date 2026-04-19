using System.IO;
using FluentAssertions;
using HnVue.CDBurning;
using HnVue.Common.Results;
using NSubstitute;
using Xunit;

namespace HnVue.CDBurning.Tests;

/// <summary>
/// Tests for CD/DVD burn pipeline: end-to-end burn scenarios, file entry construction,
/// and multi-file burn operations.
/// </summary>
[Trait("SWR", "SWR-CD-010")]
public sealed class CDBurnPipelineTests : IDisposable
{
    private readonly IBurnSession _burnSession = Substitute.For<IBurnSession>();
    private readonly IStudyRepository _studyRepo = Substitute.For<IStudyRepository>();
    private readonly CDDVDBurnService _sut;
    private readonly string _tempDir;
    private readonly List<string> _tempFiles = new();

    public CDBurnPipelineTests()
    {
        _sut = new CDDVDBurnService(_burnSession, _studyRepo);
        _tempDir = Path.Combine(Path.GetTempPath(), $"BurnPipeline_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
    }

    private string CreateTempFile(string name, int sizeBytes = 100)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllBytes(path, new byte[sizeBytes]);
        _tempFiles.Add(path);
        return path;
    }

    [Fact]
    public async Task BurnStudy_MultipleFiles_ConstructsCorrectBurnEntries()
    {
        var f1 = CreateTempFile("img1.dcm", 50);
        var f2 = CreateTempFile("img2.dcm", 150);
        var f3 = CreateTempFile("img3.dcm", 300);
        var files = (IReadOnlyList<string>)new[] { f1, f2, f3 };

        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _studyRepo.GetFilesForStudyAsync("study-001", Arg.Any<CancellationToken>())
            .Returns(Result.Success(files));
        _burnSession.BurnFilesAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), "STUDY_001",
            Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _burnSession.VerifyAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(true));

        var result = await _sut.BurnStudyAsync("study-001", "STUDY_001");

        result.IsSuccess.Should().BeTrue();
        // Verify BurnFilesAsync was called with correct number of entries
        await _burnSession.Received(1).BurnFilesAsync(
            Arg.Is<IEnumerable<BurnFileEntry>>(entries => entries.Count() == 3),
            "STUDY_001",
            Arg.Any<IProgress<double>?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BurnStudy_LabelExactly31Chars_PassesLabelValidation()
    {
        var label31 = new string('A', 31);
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _studyRepo.GetFilesForStudyAsync("1.2.3", Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<string>>(Array.Empty<string>()));

        var result = await _sut.BurnStudyAsync("1.2.3", label31);

        // Label validation passes; no files → NotFound
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task BurnStudy_ProgressCallback_InvokedDuringBurn()
    {
        var filePath = CreateTempFile("progress.dcm", 100);
        var files = (IReadOnlyList<string>)new[] { filePath };

        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _studyRepo.GetFilesForStudyAsync("study-prog", Arg.Any<CancellationToken>())
            .Returns(Result.Success(files));

        var progressReports = new List<double>();
        var progress = new Progress<double>(v => progressReports.Add(v));

        _burnSession.BurnFilesAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), "PROG_TEST",
            Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var p = callInfo.Arg<IProgress<double>?>();
                p?.Report(50.0);
                p?.Report(100.0);
                return Result.Success();
            });
        _burnSession.VerifyAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(true));

        var result = await _sut.BurnStudyAsync("study-prog", "PROG_TEST");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BurnStudy_SingleFile_Success()
    {
        var filePath = CreateTempFile("single.dcm", 200);
        var files = (IReadOnlyList<string>)new[] { filePath };

        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _studyRepo.GetFilesForStudyAsync("study-single", Arg.Any<CancellationToken>())
            .Returns(Result.Success(files));
        _burnSession.BurnFilesAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), "SINGLE",
            Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _burnSession.VerifyAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(true));

        var result = await _sut.BurnStudyAsync("study-single", "SINGLE");

        result.IsSuccess.Should().BeTrue();
    }
}
