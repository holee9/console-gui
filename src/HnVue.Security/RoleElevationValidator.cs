using HnVue.Common.Enums;
using HnVue.Common.Results;

namespace HnVue.Security;

/// <summary>
/// Validates that role changes comply with the security hierarchy.
/// STRIDE 'E' (Elevation of Privilege) control per WBS 5.1.17.
/// </summary>
public static class RoleElevationValidator
{
    /// <summary>
    /// Validates that a user can assign the target role to another user.
    /// Users cannot assign roles equal to or higher than their own.
    /// </summary>
    /// <param name="assignerRole">Role of the user performing the assignment.</param>
    /// <param name="targetRole">Role being assigned to the target user.</param>
    /// <returns>Success if the assignment is allowed; failure if it violates the hierarchy.</returns>
    public static Result ValidateRoleAssignment(UserRole assignerRole, UserRole targetRole)
    {
        var assignerLevel = RoleLevel(assignerRole);
        var targetLevel = RoleLevel(targetRole);

        if (targetLevel >= assignerLevel)
            return Result.Failure(ErrorCode.RoleElevationBlocked,
                $"Role '{assignerRole}' cannot assign role '{targetRole}'. " +
                "Only higher-privileged users can assign this role.");

        return Result.Success();
    }

    /// <summary>
    /// Validates that a user is not attempting self-elevation.
    /// </summary>
    /// <param name="currentRole">User's current role.</param>
    /// <param name="requestedRole">Role being requested.</param>
    /// <returns>Success if no self-elevation; failure otherwise.</returns>
    public static Result ValidateNoSelfElevation(UserRole currentRole, UserRole requestedRole)
    {
        if (RoleLevel(requestedRole) > RoleLevel(currentRole))
            return Result.Failure(ErrorCode.RoleElevationBlocked,
                $"Self-elevation from '{currentRole}' to '{requestedRole}' is prohibited.");

        return Result.Success();
    }

    /// <summary>
    /// Returns true if the operation requires re-authentication based on role sensitivity.
    /// </summary>
    public static bool RequiresReauthentication(UserRole targetRole)
        => targetRole is UserRole.Admin or UserRole.Service;

    private static int RoleLevel(UserRole role) => role switch
    {
        UserRole.Radiographer => 1,
        UserRole.Radiologist => 2,
        UserRole.Admin => 3,
        UserRole.Service => 3,
        _ => 0,
    };
}
