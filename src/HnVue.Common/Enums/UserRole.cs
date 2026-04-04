namespace HnVue.Common.Enums;

/// <summary>
/// Defines the roles available to authenticated users of the HnVue console.
/// Role-based access control (RBAC) is enforced by <c>ISecurityService</c>.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Clinical radiographer. Permitted to: view and register patients, perform exposures.
    /// </summary>
    Radiographer,

    /// <summary>
    /// Radiologist. Permitted to: view patients, perform exposures, review images, burn study to CD.
    /// </summary>
    Radiologist,

    /// <summary>
    /// System administrator. Permitted to: view and register patients, burn CD,
    /// configure system settings, view audit log, apply software updates.
    /// </summary>
    Admin,

    /// <summary>
    /// Field-service engineer. Permitted to: configure system settings, view audit log,
    /// apply software updates. No access to clinical operations.
    /// </summary>
    Service,
}
