using System.IO;
using FluentAssertions;
using HnVue.CDBurning;
using HnVue.Common.Results;
using NSubstitute;
using Xunit;

namespace HnVue.CDBurning.Tests;

[Trait("SWR", "SWR-CD-010")]
public sealed class CDDVDBurnServiceTests : IDisposable
{
    private readonly IBurnSession _burnSession;
    private readonly IStudyRepository _studyRepo;
    private readonly CDDVDBurnService _sut;
    private readonly string _tempDir;
    private readonly List<string> _tempFiles = new();

    public CDDVDBurnServiceTests()
    {
        _burnSession = Substitute.For<IBurnSession>();
        _studyRepo = Substitute.For<IStudyRepository>();
        _sut = new CDDVDBurnService(_burnSession, _studyRepo);

        _tempDir = Path.Combine(Path.GetTempPath(), $"BurnTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
    }

    private string CreateTempFile(string name = "test.dcm", string content = "dicom data")
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullBurnSession_ThrowsArgumentNullException()
    {
        var act = () => new CDDVDBurnService(null!, _studyRepo);

        act.Should().Throw<ArgumentNullException>().WithParameterName("burnSession");
    }

    [Fact]
    public void Constructor_NullStudyRepository_ThrowsArgumentNullException()
    {
        var act = () => new CDDVDBurnService(_burnSession, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("studyRepository");
    }

    // ── BurnStudyAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task BurnStudy_ValidStudy_BurnsAndVerifiesSuccessfully()
    {
        var filePath = CreateTempFile();
        var files = (IReadOnlyList<string>)new[] { filePath };

        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _studyRepo.GetFilesForStudyAsync("1.2.3", Arg.Any<CancellationToken>())
            .Returns(Result.Success(files));
        _burnSession.BurnFilesAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), "CHEST_001",
            Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _burnSession.VerifyAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(true));

        var result = await _sut.BurnStudyAsync("1.2.3", "CHEST_001");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BurnStudy_NoDiscInserted_ReturnsBurnFailed()
    {
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(false));

        var result = await _sut.BurnStudyAsync("1.2.3", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.BurnFailed);
    }

    [Fact]
    public async Task BurnStudy_DiscNotBlank_ReturnsBurnFailed()
    {
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(false));

        var result = await _sut.BurnStudyAsync("1.2.3", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.BurnFailed);
    }

    [Fact]
    public async Task BurnStudy_NoFilesForStudy_ReturnsNotFound()
    {
        var emptyFiles = (IReadOnlyList<string>)Array.Empty<string>();
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _studyRepo.GetFilesForStudyAsync("1.2.3", Arg.Any<CancellationToken>())
            .Returns(Result.Success(emptyFiles));

        var result = await _sut.BurnStudyAsync("1.2.3", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task BurnStudy_LabelTooLong_ReturnsValidationFailure()
    {
        var result = await _sut.BurnStudyAsync("1.2.3", new string('X', 33));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task BurnStudy_EmptyStudyUid_ReturnsValidationFailure()
    {
        var result = await _sut.BurnStudyAsync("", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task BurnStudy_NullStudyUid_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.BurnStudyAsync(null!, "LABEL");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task BurnStudy_VerificationFails_ReturnsDiscVerificationFailed()
    {
        var filePath = CreateTempFile("verify_fail.dcm");
        var files = (IReadOnlyList<string>)new[] { filePath };

        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _studyRepo.GetFilesForStudyAsync("1.2.3", Arg.Any<CancellationToken>())
            .Returns(Result.Success(files));
        _burnSession.BurnFilesAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), Arg.Any<string>(),
            Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _burnSession.VerifyAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(false));

        var result = await _sut.BurnStudyAsync("1.2.3", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DiscVerificationFailed);
    }

    [Fact]
    public async Task BurnStudy_NullOutputLabel_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.BurnStudyAsync("1.2.3", null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task BurnStudy_WhitespaceStudyUid_ReturnsValidationFailure()
    {
        var result = await _sut.BurnStudyAsync("   ", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task BurnStudy_LabelExactly32Chars_PassesValidation()
    {
        var label32 = new string('X', 32);
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        var emptyFiles = (IReadOnlyList<string>)Array.Empty<string>();
        _studyRepo.GetFilesForStudyAsync("1.2.3", Arg.Any<CancellationToken>())
            .Returns(Result.Success(emptyFiles));

        var result = await _sut.BurnStudyAsync("1.2.3", label32);

        // Validation passes but returns NotFound (no files) — proves label length check allows 32
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task BurnStudy_IsDiscInsertedReturnsFailure_ReturnsBurnFailed()
    {
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<bool>(ErrorCode.BurnFailed, "Hardware error"));

        var result = await _sut.BurnStudyAsync("1.2.3", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.BurnFailed);
    }

    [Fact]
    public async Task BurnStudy_IsDiscBlankReturnsFailure_ReturnsBurnFailed()
    {
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<bool>(ErrorCode.BurnFailed, "Hardware error"));

        var result = await _sut.BurnStudyAsync("1.2.3", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.BurnFailed);
    }

    [Fact]
    public async Task BurnStudy_GetFilesReturnsFailure_PropagatesError()
    {
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _studyRepo.GetFilesForStudyAsync("1.2.3", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<string>>(ErrorCode.NotFound, "Study not found in DICOM store"));

        var result = await _sut.BurnStudyAsync("1.2.3", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task BurnStudy_BurnFilesReturnsFailure_PropagatesBurnError()
    {
        var filePath = CreateTempFile("burn_fail.dcm");
        var files = (IReadOnlyList<string>)new[] { filePath };

        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _studyRepo.GetFilesForStudyAsync("1.2.3", Arg.Any<CancellationToken>())
            .Returns(Result.Success(files));
        _burnSession.BurnFilesAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), Arg.Any<string>(),
            Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.BurnFailed, "Burn hardware error"));

        var result = await _sut.BurnStudyAsync("1.2.3", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.BurnFailed);
    }

    [Fact]
    public async Task BurnStudy_VerifyReturnsFailureResult_ReturnsDiscVerificationFailed()
    {
        var filePath = CreateTempFile("verify_fail2.dcm");
        var files = (IReadOnlyList<string>)new[] { filePath };

        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _studyRepo.GetFilesForStudyAsync("1.2.3", Arg.Any<CancellationToken>())
            .Returns(Result.Success(files));
        _burnSession.BurnFilesAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), Arg.Any<string>(),
            Arg.Any<IProgress<double>?>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _burnSession.VerifyAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<bool>(ErrorCode.DiscVerificationFailed, "Checksum mismatch"));

        var result = await _sut.BurnStudyAsync("1.2.3", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DiscVerificationFailed);
    }

    // ── VerifyDiscAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task VerifyDisc_DiscInserted_DelegatesToSession()
    {
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.VerifyAsync(Arg.Any<IEnumerable<BurnFileEntry>>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(true));

        var result = await _sut.VerifyDiscAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyDisc_NoDiscInserted_ReturnsFalse()
    {
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(false));

        var result = await _sut.VerifyDiscAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyDisc_IsDiscInsertedReturnsFailure_ReturnsDiscVerificationFailed()
    {
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<bool>(ErrorCode.DiscVerificationFailed, "Drive not responding"));

        var result = await _sut.VerifyDiscAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DiscVerificationFailed);
    }
}
