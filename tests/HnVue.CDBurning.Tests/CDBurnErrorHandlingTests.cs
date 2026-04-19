using FluentAssertions;
using HnVue.CDBurning;
using HnVue.Common.Results;
using NSubstitute;
using Xunit;

namespace HnVue.CDBurning.Tests;

/// <summary>
/// Tests for burn error handling: disc errors, verification failures, cancellation.
/// </summary>
[Trait("SWR", "SWR-CD-010")]
public sealed class CDBurnErrorHandlingTests
{
    private readonly IBurnSession _burnSession = Substitute.For<IBurnSession>();
    private readonly IStudyRepository _studyRepo = Substitute.For<IStudyRepository>();
    private readonly CDDVDBurnService _sut;

    public CDBurnErrorHandlingTests()
    {
        _sut = new CDDVDBurnService(_burnSession, _studyRepo);
    }

    [Fact]
    public async Task BurnStudy_StudyUidWhitespaceOnly_ReturnsValidationFailed()
    {
        var result = await _sut.BurnStudyAsync("\t\n ", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task BurnStudy_Label33Chars_ReturnsValidationFailed()
    {
        var result = await _sut.BurnStudyAsync("1.2.3", new string('Y', 33));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task BurnStudy_Label32Chars_PassesLabelCheck()
    {
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(false));
        var label = new string('Z', 32);

        var result = await _sut.BurnStudyAsync("1.2.3", label);

        // Label check passes, but disc not inserted → BurnFailed
        result.Error.Should().Be(ErrorCode.BurnFailed);
    }

    [Fact]
    public async Task VerifyDisc_CalledOnNewService_DoesNotThrow()
    {
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(false));

        var result = await _sut.VerifyDiscAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task BurnStudy_DiscInsertedCheckReturnsFalse_ReturnsBurnFailed()
    {
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(false));

        var result = await _sut.BurnStudyAsync("1.2.3", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.BurnFailed);
        result.ErrorMessage.Should().Contain("No disc");
    }

    [Fact]
    public async Task BurnStudy_DiscNotBlank_ReturnsBurnFailedWithNotBlankMessage()
    {
        _burnSession.IsDiscInsertedAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(true));
        _burnSession.IsDiscBlankAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(false));

        var result = await _sut.BurnStudyAsync("1.2.3", "LABEL");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.BurnFailed);
        result.ErrorMessage.Should().Contain("not blank");
    }
}
