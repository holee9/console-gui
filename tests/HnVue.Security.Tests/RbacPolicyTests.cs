using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Results;
using HnVue.Security;
using Xunit;

namespace HnVue.Security.Tests;

[Trait("SWR", "SWR-SEC-020")]
public sealed class RbacPolicyTests
{
    // ── Radiographer permissions ──────────────────────────────────────────────

    [Theory]
    [InlineData(Permissions.ViewPatients)]
    [InlineData(Permissions.RegisterPatient)]
    [InlineData(Permissions.PerformExposure)]
    public void Check_RadioGrapher_HasClinicalPermissions(string permission)
    {
        var result = RbacPolicy.Check(UserRole.Radiographer, permission);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(Permissions.ReviewImages)]
    [InlineData(Permissions.BurnStudyToCd)]
    [InlineData(Permissions.ConfigureSystem)]
    [InlineData(Permissions.ViewAuditLog)]
    [InlineData(Permissions.ApplySoftwareUpdate)]
    public void Check_Radiographer_LacksElevatedPermissions(string permission)
    {
        var result = RbacPolicy.Check(UserRole.Radiographer, permission);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InsufficientPermission);
    }

    // ── Radiologist permissions ───────────────────────────────────────────────

    [Theory]
    [InlineData(Permissions.ViewPatients)]
    [InlineData(Permissions.RegisterPatient)]
    [InlineData(Permissions.PerformExposure)]
    [InlineData(Permissions.ReviewImages)]
    [InlineData(Permissions.BurnStudyToCd)]
    public void Check_Radiologist_HasAllClinicalPermissions(string permission)
    {
        var result = RbacPolicy.Check(UserRole.Radiologist, permission);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(Permissions.ConfigureSystem)]
    [InlineData(Permissions.ViewAuditLog)]
    [InlineData(Permissions.ApplySoftwareUpdate)]
    public void Check_Radiologist_LacksAdminPermissions(string permission)
    {
        var result = RbacPolicy.Check(UserRole.Radiologist, permission);

        result.IsFailure.Should().BeTrue();
    }

    // ── Admin permissions ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(Permissions.ViewPatients)]
    [InlineData(Permissions.RegisterPatient)]
    [InlineData(Permissions.BurnStudyToCd)]
    [InlineData(Permissions.ConfigureSystem)]
    [InlineData(Permissions.ViewAuditLog)]
    [InlineData(Permissions.ApplySoftwareUpdate)]
    public void Check_Admin_HasAllAdminPermissions(string permission)
    {
        var result = RbacPolicy.Check(UserRole.Admin, permission);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(Permissions.PerformExposure)]
    [InlineData(Permissions.ReviewImages)]
    public void Check_Admin_LacksClinicalOperationPermissions(string permission)
    {
        // Admin cannot perform clinical operations (exposures/review) — by design
        var result = RbacPolicy.Check(UserRole.Admin, permission);

        result.IsFailure.Should().BeTrue();
    }

    // ── Service permissions ───────────────────────────────────────────────────

    [Theory]
    [InlineData(Permissions.ConfigureSystem)]
    [InlineData(Permissions.ViewAuditLog)]
    [InlineData(Permissions.ApplySoftwareUpdate)]
    public void Check_Service_HasServicePermissions(string permission)
    {
        var result = RbacPolicy.Check(UserRole.Service, permission);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(Permissions.ViewPatients)]
    [InlineData(Permissions.RegisterPatient)]
    [InlineData(Permissions.PerformExposure)]
    [InlineData(Permissions.ReviewImages)]
    [InlineData(Permissions.BurnStudyToCd)]
    public void Check_Service_LacksAllClinicalPermissions(string permission)
    {
        var result = RbacPolicy.Check(UserRole.Service, permission);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.InsufficientPermission);
    }

    // ── GetPermissions ────────────────────────────────────────────────────────

    [Fact]
    public void GetPermissions_Radiographer_ReturnsExactThreePermissions()
    {
        var permissions = RbacPolicy.GetPermissions(UserRole.Radiographer);

        permissions.Should().HaveCount(3);
    }

    [Fact]
    public void GetPermissions_Radiologist_ReturnsSixPermissions()
    {
        var permissions = RbacPolicy.GetPermissions(UserRole.Radiologist);

        permissions.Should().HaveCount(6);
    }

    // ── HasRoleOrHigher ───────────────────────────────────────────────────────

    [Theory]
    [InlineData(UserRole.Admin, UserRole.Radiographer, true)]
    [InlineData(UserRole.Admin, UserRole.Radiologist, true)]
    [InlineData(UserRole.Admin, UserRole.Admin, true)]
    [InlineData(UserRole.Radiographer, UserRole.Admin, false)]
    [InlineData(UserRole.Radiologist, UserRole.Admin, false)]
    [InlineData(UserRole.Service, UserRole.Admin, true)]
    [InlineData(UserRole.Service, UserRole.Radiologist, true)]
    public void HasRoleOrHigher_VariousCombinations_ReturnsExpected(
        UserRole userRole, UserRole required, bool expected)
    {
        var result = RbacPolicy.HasRoleOrHigher(userRole, required);

        result.Should().Be(expected);
    }

    // ── Error message quality ─────────────────────────────────────────────────

    [Fact]
    public void Check_Denied_ErrorMessageContainsRoleAndPermission()
    {
        var result = RbacPolicy.Check(UserRole.Radiographer, Permissions.ConfigureSystem);

        result.ErrorMessage.Should().Contain("Radiographer");
        result.ErrorMessage.Should().Contain(Permissions.ConfigureSystem);
    }

    // ── Null guard ────────────────────────────────────────────────────────────

    [Fact]
    public void Check_NullPermission_ThrowsArgumentNullException()
    {
        var act = () => RbacPolicy.Check(UserRole.Admin, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
