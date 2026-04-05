using HnVue.Common.Enums;
using HnVue.Common.Results;
using static HnVue.Common.Results.ErrorCode;

namespace HnVue.Security;

/// <summary>
/// Defines and enforces the Role-Based Access Control (RBAC) policy for the HnVue console.
/// Implements a hierarchical permission model for the four defined user roles.
/// </summary>
/// <remarks>
/// Role hierarchy (highest to lowest): Service ≈ Admin > Radiologist > Radiographer.
/// Permissions are additive; higher roles inherit lower-role permissions where applicable.
/// IEC 62304 Class B — safety-critical: access control for radiation-emitting system.
/// </remarks>
public static class RbacPolicy
{
    // ── Permission definitions per role ───────────────────────────────────────

    private static readonly Dictionary<UserRole, HashSet<string>> _permissions = new()
    {
        [UserRole.Radiographer] = new HashSet<string>(StringComparer.Ordinal)
        {
            Permissions.ViewPatients,
            Permissions.RegisterPatient,
            Permissions.PerformExposure,
        },
        [UserRole.Radiologist] = new HashSet<string>(StringComparer.Ordinal)
        {
            Permissions.ViewPatients,
            Permissions.RegisterPatient,
            Permissions.PerformExposure,
            Permissions.ReviewImages,
            Permissions.BurnStudyToCd,
        },
        [UserRole.Admin] = new HashSet<string>(StringComparer.Ordinal)
        {
            Permissions.ViewPatients,
            Permissions.RegisterPatient,
            Permissions.BurnStudyToCd,
            Permissions.ConfigureSystem,
            Permissions.ViewAuditLog,
            Permissions.ApplySoftwareUpdate,
        },
        [UserRole.Service] = new HashSet<string>(StringComparer.Ordinal)
        {
            Permissions.ConfigureSystem,
            Permissions.ViewAuditLog,
            Permissions.ApplySoftwareUpdate,
        },
    };

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Checks whether the specified role holds the given permission.
    /// </summary>
    /// <param name="role">User role to evaluate.</param>
    /// <param name="permission">Permission name constant from <see cref="Permissions"/>.</param>
    /// <returns>
    /// A successful <see cref="Result"/> if the role has the permission;
    /// otherwise a failure with <see cref="ErrorCode.InsufficientPermission"/>.
    /// </returns>
    public static Result Check(UserRole role, string permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        if (_permissions.TryGetValue(role, out var granted) && granted.Contains(permission))
            return Result.Success();

        return Result.Failure(
            InsufficientPermission,
            $"Role '{role}' does not have permission '{permission}'.");
    }

    /// <summary>
    /// Returns all permissions granted to the specified role.
    /// </summary>
    /// <param name="role">User role to query.</param>
    /// <returns>Read-only set of permission strings for the role.</returns>
    public static IReadOnlySet<string> GetPermissions(UserRole role)
    {
        return _permissions.TryGetValue(role, out var permissions)
            ? permissions
            : new HashSet<string>();
    }

    /// <summary>
    /// Checks whether a role is at least as privileged as the required role.
    /// Used for hierarchical role checks where a minimum role level is needed.
    /// </summary>
    /// <param name="userRole">The role held by the user.</param>
    /// <param name="requiredRole">The minimum role required.</param>
    /// <returns>True if the user's role grants at least the same privileges as the required role.</returns>
    public static bool HasRoleOrHigher(UserRole userRole, UserRole requiredRole)
    {
        return RoleHierarchy(userRole) >= RoleHierarchy(requiredRole);
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private static int RoleHierarchy(UserRole role) => role switch
    {
        UserRole.Radiographer => 1,
        UserRole.Radiologist  => 2,
        UserRole.Admin        => 3,
        UserRole.Service      => 3, // Admin and Service are peers at level 3
        _                     => 0,
    };
}

/// <summary>
/// Constant strings for all named permissions in the HnVue RBAC system.
/// </summary>
public static class Permissions
{
    /// <summary>Permission to view the patient list.</summary>
    public const string ViewPatients       = "patients.view";

    /// <summary>Permission to register a new patient.</summary>
    public const string RegisterPatient    = "patients.register";

    /// <summary>Permission to perform an X-ray exposure.</summary>
    public const string PerformExposure    = "workflow.expose";

    /// <summary>Permission to review acquired images.</summary>
    public const string ReviewImages       = "images.review";

    /// <summary>Permission to burn a study to CD/DVD.</summary>
    public const string BurnStudyToCd      = "cdburning.burn";

    /// <summary>Permission to configure system settings.</summary>
    public const string ConfigureSystem    = "sysadmin.configure";

    /// <summary>Permission to view the tamper-evident audit log.</summary>
    public const string ViewAuditLog       = "audit.view";

    /// <summary>Permission to apply a software update package.</summary>
    public const string ApplySoftwareUpdate = "update.apply";
}
