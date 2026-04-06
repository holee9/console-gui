using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Results;
using HnVue.UI.ViewModels;
using NSubstitute;
using Xunit;

namespace HnVue.UI.Tests;

public sealed class QuickPinLockViewModelTests
{
    private readonly ISecurityContext _securityContext;
    private readonly ISecurityService _securityService;
    private readonly QuickPinLockViewModel _sut;

    public QuickPinLockViewModelTests()
    {
        _securityContext = Substitute.For<ISecurityContext>();
        _securityService = Substitute.For<ISecurityService>();
        _securityContext.CurrentUserId.Returns("user-1");
        _securityContext.CurrentUsername.Returns("testuser");
        _securityContext.IsAuthenticated.Returns(true);
        _sut = new QuickPinLockViewModel(_securityContext, _securityService);
        _sut.Activate();
    }

    [Fact]
    public async Task VerifyPin_CorrectPin_RaisesSessionResumed()
    {
        _securityService.VerifyQuickPinAsync("user-1", "1234", Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _sut.Pin = "1234";
        var raised = false;
        _sut.SessionResumed += (_, _) => raised = true;

        await _sut.VerifyPinCommand.ExecuteAsync(null);

        raised.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPin_WrongPin_DecrementsRemainingAttempts()
    {
        _securityService.VerifyQuickPinAsync("user-1", "9999", Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.AuthenticationFailed, "Wrong PIN."));
        _sut.Pin = "9999";

        await _sut.VerifyPinCommand.ExecuteAsync(null);

        _sut.RemainingAttempts.Should().Be(2);
        _sut.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task VerifyPin_ThreeFailures_RaisesForceLogout()
    {
        _securityService.VerifyQuickPinAsync("user-1", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.AuthenticationFailed, "Wrong PIN."));
        var logoutRaised = false;
        _sut.ForceLogout += (_, _) => logoutRaised = true;

        for (var i = 0; i < 3; i++)
        {
            _sut.Pin = "9999";
            await _sut.VerifyPinCommand.ExecuteAsync(null);
        }

        logoutRaised.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPin_TooShort_ShowsErrorWithoutCallingService()
    {
        _sut.Pin = "12";

        await _sut.VerifyPinCommand.ExecuteAsync(null);

        _sut.ErrorMessage.Should().NotBeNullOrEmpty();
        await _securityService.DidNotReceive().VerifyQuickPinAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
