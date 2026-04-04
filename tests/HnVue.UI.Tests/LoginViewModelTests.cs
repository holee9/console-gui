using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.UI.ViewModels;
using NSubstitute;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Unit tests for <see cref="LoginViewModel"/>.
/// </summary>
public sealed class LoginViewModelTests
{
    private static AuthenticationToken CreateToken() => new(
        UserId: "u1",
        Username: "admin",
        Role: UserRole.Admin,
        Token: "jwt.test.token",
        ExpiresAt: DateTimeOffset.UtcNow.AddHours(8));

    private static (LoginViewModel Vm, ISecurityService SecurityService, ISecurityContext SecurityContext) CreateSut()
    {
        var securityService = Substitute.For<ISecurityService>();
        var securityContext = Substitute.For<ISecurityContext>();
        var vm = new LoginViewModel(securityService, securityContext);
        return (vm, securityService, securityContext);
    }

    // ── Constructor guard tests ──────────────────────────────────────────────

    [Fact]
    public void Constructor_WhenSecurityServiceIsNull_ThrowsArgumentNullException()
    {
        var act = () => new LoginViewModel(null!, Substitute.For<ISecurityContext>());
        act.Should().Throw<ArgumentNullException>().WithParameterName("securityService");
    }

    [Fact]
    public void Constructor_WhenSecurityContextIsNull_ThrowsArgumentNullException()
    {
        var act = () => new LoginViewModel(Substitute.For<ISecurityService>(), null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("securityContext");
    }

    // ── CanLogin tests ────────────────────────────────────────────────────────

    [Fact]
    public void CanLogin_WhenUsernameEmpty_ReturnsFalse()
    {
        var (vm, _, _) = CreateSut();
        vm.Username = string.Empty;
        vm.Password = "pass";

        vm.LoginCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void CanLogin_WhenUsernameWhitespaceOnly_ReturnsFalse()
    {
        var (vm, _, _) = CreateSut();
        vm.Username = "   ";
        vm.Password = "pass";

        vm.LoginCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void CanLogin_WhenPasswordEmpty_ReturnsFalse()
    {
        var (vm, _, _) = CreateSut();
        vm.Username = "user";
        vm.Password = string.Empty;

        vm.LoginCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void CanLogin_WhenPasswordWhitespaceOnly_ReturnsFalse()
    {
        var (vm, _, _) = CreateSut();
        vm.Username = "user";
        vm.Password = "   ";

        vm.LoginCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void CanLogin_WhenBothProvided_ReturnsTrue()
    {
        var (vm, _, _) = CreateSut();
        vm.Username = "admin";
        vm.Password = "secret";

        vm.LoginCommand.CanExecute(null).Should().BeTrue();
    }

    // ── LoginAsync success path ───────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WhenAuthSucceeds_RaisesLoginSucceeded()
    {
        var (vm, securityService, _) = CreateSut();
        var token = CreateToken();
        securityService
            .AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(token));

        vm.Username = "admin";
        vm.Password = "secret";

        LoginSuccessEventArgs? receivedArgs = null;
        vm.LoginSucceeded += (_, e) => receivedArgs = e;

        await vm.LoginCommand.ExecuteAsync(null);

        receivedArgs.Should().NotBeNull();
        receivedArgs!.Token.Should().Be(token);
    }

    [Fact]
    public async Task LoginAsync_WhenAuthSucceeds_ErrorMessageRemainsNull()
    {
        var (vm, securityService, _) = CreateSut();
        securityService
            .AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(CreateToken()));

        vm.Username = "admin";
        vm.Password = "secret";

        await vm.LoginCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().BeNull();
    }

    // ── LoginAsync failure paths ──────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_WhenAuthFails_SetsGenericErrorMessage()
    {
        var (vm, securityService, _) = CreateSut();
        securityService
            .AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AuthenticationToken>(ErrorCode.AuthenticationFailed, "Bad credentials"));

        vm.Username = "admin";
        vm.Password = "wrong";

        await vm.LoginCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("사용자명 또는 비밀번호가 올바르지 않습니다.");
    }

    [Fact]
    public async Task LoginAsync_WhenAccountLocked_SetsLockedMessage()
    {
        var (vm, securityService, _) = CreateSut();
        securityService
            .AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AuthenticationToken>(ErrorCode.AccountLocked, "Locked"));

        vm.Username = "admin";
        vm.Password = "secret";

        await vm.LoginCommand.ExecuteAsync(null);

        vm.ErrorMessage.Should().Be("계정이 잠겼습니다. 관리자에게 문의하세요.");
    }

    [Fact]
    public async Task LoginAsync_WhenAuthFails_DoesNotRaiseLoginSucceeded()
    {
        var (vm, securityService, _) = CreateSut();
        securityService
            .AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AuthenticationToken>(ErrorCode.AuthenticationFailed, "Bad"));

        vm.Username = "admin";
        vm.Password = "wrong";

        var raised = false;
        vm.LoginSucceeded += (_, _) => raised = true;

        await vm.LoginCommand.ExecuteAsync(null);

        raised.Should().BeFalse();
    }

    // ── IsLoading behaviour ───────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_AfterCompletion_IsLoadingIsFalse()
    {
        var (vm, securityService, _) = CreateSut();
        securityService
            .AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(CreateToken()));

        vm.Username = "admin";
        vm.Password = "secret";

        await vm.LoginCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_AfterFailure_IsLoadingIsFalse()
    {
        var (vm, securityService, _) = CreateSut();
        securityService
            .AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AuthenticationToken>(ErrorCode.AuthenticationFailed, "Bad"));

        vm.Username = "admin";
        vm.Password = "wrong";

        await vm.LoginCommand.ExecuteAsync(null);

        vm.IsLoading.Should().BeFalse();
    }
}
