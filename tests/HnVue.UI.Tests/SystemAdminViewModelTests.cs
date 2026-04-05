using FluentAssertions;
using HnVue.Common.Abstractions;
using Xunit;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.UI.ViewModels;
using NSubstitute;

namespace HnVue.UI.Tests;

/// <summary>
/// Tests for <see cref="SystemAdminViewModel"/>.
/// </summary>
public sealed class SystemAdminViewModelTests
{
    private readonly ISystemAdminService _adminService = Substitute.For<ISystemAdminService>();
    private readonly ISecurityContext _securityContext = Substitute.For<ISecurityContext>();

    private SystemAdminViewModel CreateSut() => new(_adminService, _securityContext);

    [Fact]
    public void IsAdminUser_FalseWhenUserHasNoAdminRole()
    {
        _securityContext.HasRole(UserRole.Admin).Returns(false);
        _securityContext.HasRole(UserRole.Service).Returns(false);

        var sut = CreateSut();

        sut.IsAdminUser.Should().BeFalse();
    }

    [Fact]
    public void IsAdminUser_TrueWhenUserIsAdmin()
    {
        _securityContext.HasRole(UserRole.Admin).Returns(true);
        _securityContext.HasRole(UserRole.Service).Returns(false);

        var sut = CreateSut();

        sut.IsAdminUser.Should().BeTrue();
    }

    [Fact]
    public void IsAdminUser_TrueWhenUserIsService()
    {
        _securityContext.HasRole(UserRole.Admin).Returns(false);
        _securityContext.HasRole(UserRole.Service).Returns(true);

        var sut = CreateSut();

        sut.IsAdminUser.Should().BeTrue();
    }

    [Fact]
    public void IsAdminUser_FalseForRadiographer()
    {
        _securityContext.HasRole(UserRole.Radiographer).Returns(true);
        _securityContext.HasRole(UserRole.Admin).Returns(false);
        _securityContext.HasRole(UserRole.Service).Returns(false);

        var sut = CreateSut();

        sut.IsAdminUser.Should().BeFalse();
    }

    [Fact]
    public async Task LoadSettingsCommand_OnSuccess_SetsSettings()
    {
        var settings = new SystemSettings
        {
            Dicom = new DicomSettings { PacsHost = "192.168.1.10" }
        };
        _securityContext.HasRole(UserRole.Admin).Returns(true);
        _adminService
            .GetSettingsAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success<SystemSettings>(settings));

        var sut = CreateSut();
        await sut.LoadSettingsCommand.ExecuteAsync(null);

        sut.Settings.Dicom.PacsHost.Should().Be("192.168.1.10");
        sut.StatusMessage.Should().Contain("loaded");
    }

    [Fact]
    public async Task LoadSettingsCommand_OnFailure_SetsErrorMessage()
    {
        _securityContext.HasRole(UserRole.Admin).Returns(true);
        _adminService
            .GetSettingsAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<SystemSettings>(ErrorCode.DatabaseError, "Disk error"));

        var sut = CreateSut();
        await sut.LoadSettingsCommand.ExecuteAsync(null);

        sut.ErrorMessage.Should().Be("Disk error");
    }

    [Fact]
    public async Task SaveSettingsCommand_OnSuccess_SetsStatusMessage()
    {
        _securityContext.HasRole(UserRole.Admin).Returns(true);
        _adminService
            .UpdateSettingsAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = CreateSut();
        await sut.SaveSettingsCommand.ExecuteAsync(null);

        sut.StatusMessage.Should().Contain("saved");
    }

    [Fact]
    public async Task SaveSettingsCommand_OnFailure_SetsErrorMessage()
    {
        _securityContext.HasRole(UserRole.Admin).Returns(true);
        _adminService
            .UpdateSettingsAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.ValidationFailed, "Invalid COM port."));

        var sut = CreateSut();
        await sut.SaveSettingsCommand.ExecuteAsync(null);

        sut.ErrorMessage.Should().Be("Invalid COM port.");
    }
}
