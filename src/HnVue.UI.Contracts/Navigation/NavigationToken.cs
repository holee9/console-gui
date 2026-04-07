// @MX:NOTE Type-safe navigation enum replaces magic strings; prevents typos in navigation calls
namespace HnVue.UI.Contracts.Navigation;

/// <summary>
/// Type-safe navigation identifiers for shell region content switching.
/// </summary>
public enum NavigationToken
{
    /// <summary>Login screen.</summary>
    Login,

    /// <summary>Patient list / worklist screen.</summary>
    PatientList,

    /// <summary>Acquisition workflow control panel.</summary>
    Workflow,

    /// <summary>DICOM image viewer.</summary>
    ImageViewer,

    /// <summary>Dose display panel.</summary>
    DoseDisplay,

    /// <summary>CD/DVD burn dialog.</summary>
    CDBurn,

    /// <summary>System administration settings screen.</summary>
    SystemAdmin,

    /// <summary>Quick PIN lock overlay.</summary>
    QuickPinLock,

    /// <summary>Emergency acquisition bypass screen.</summary>
    Emergency
}
