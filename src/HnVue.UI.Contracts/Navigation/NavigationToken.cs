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
    Emergency,

    // ── team-design 요청 신규 추가 (DESIGN_PLAN_v2.md) ──────────────────────

    /// <summary>Study list full-screen view. PPT slides 5-7.</summary>
    Studylist,

    /// <summary>Sync Study (merge) full-screen view. PPT slides 12-13.</summary>
    Merge,

    /// <summary>Settings full-screen view. PPT slides 14-22.</summary>
    Settings
}
