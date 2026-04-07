using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HnVue.UI.Contracts.ViewModels;

// @MX:NOTE IAddPatientProcedureViewModel — PPT slide 8: merged Add Patient + Procedure contract.
//          Required fields: PatientName(*), BirthDate(*), Gender(*), AccNo(*), Procedure(*).
//          Auto-generate toggles for PatientId and AccessionNumber.
/// <summary>
/// Contract for the merged Add Patient/Procedure ViewModel.
/// PPT slide 8: single unified window for patient registration and procedure selection.
/// </summary>
public interface IAddPatientProcedureViewModel : IViewModelBase
{
    // ── Patient fields ──────────────────────────────────────────────────────

    /// <summary>Gets or sets the patient identifier. Auto-generated when <see cref="IsPatientIdAutoGenerate"/> is true.</summary>
    string PatientId { get; set; }

    /// <summary>Gets or sets a value indicating whether PatientId is auto-generated.</summary>
    bool IsPatientIdAutoGenerate { get; set; }

    /// <summary>Gets or sets the patient full name. Required (*).</summary>
    string PatientName { get; set; }

    /// <summary>Gets or sets the patient birth date (format: yyyy-MM-dd). Required (*).</summary>
    string BirthDate { get; set; }

    /// <summary>Gets or sets the patient gender (M/F/Other). Required (*).</summary>
    string Gender { get; set; }

    // ── Accession / Study fields ─────────────────────────────────────────────

    /// <summary>Gets or sets the accession number. Auto-generated when <see cref="IsAccNoAutoGenerate"/> is true. Required (*).</summary>
    string AccessionNumber { get; set; }

    /// <summary>Gets or sets a value indicating whether AccessionNumber is auto-generated.</summary>
    bool IsAccNoAutoGenerate { get; set; }

    /// <summary>Gets or sets the study description.</summary>
    string StudyDescription { get; set; }

    // ── View Projection (multi-select chips) ─────────────────────────────────

    /// <summary>Gets the collection of currently selected View Projections shown as chips.</summary>
    ObservableCollection<string> SelectedProjections { get; }

    /// <summary>Gets the list of available projection options for the dropdown.</summary>
    IReadOnlyList<string> AvailableProjections { get; }

    // ── Description (multi-select chips) ────────────────────────────────────

    /// <summary>Gets the collection of selected description tags shown as chips.</summary>
    ObservableCollection<string> SelectedDescriptions { get; }

    /// <summary>Gets the list of available description options for the dropdown.</summary>
    IReadOnlyList<string> AvailableDescriptions { get; }

    /// <summary>Gets or sets the current manual description input value.</summary>
    string? DescriptionInput { get; set; }

    // ── RIS Code ─────────────────────────────────────────────────────────────

    /// <summary>Gets or sets the RIS code. Supports inline real-time add/update.</summary>
    string RisCode { get; set; }

    // ── Commands ─────────────────────────────────────────────────────────────

    /// <summary>Adds a projection string to <see cref="SelectedProjections"/>.</summary>
    ICommand AddProjectionCommand { get; }

    /// <summary>Removes a projection string from <see cref="SelectedProjections"/>.</summary>
    ICommand RemoveProjectionCommand { get; }

    /// <summary>Adds a description chip to <see cref="SelectedDescriptions"/>.</summary>
    ICommand AddDescriptionCommand { get; }

    /// <summary>Removes a description chip from <see cref="SelectedDescriptions"/>.</summary>
    ICommand RemoveDescriptionCommand { get; }

    /// <summary>Toggles auto-generation of the accession number.</summary>
    ICommand ToggleAccNoAutoGenerateCommand { get; }

    /// <summary>Toggles auto-generation of the patient identifier.</summary>
    ICommand TogglePatientIdAutoGenerateCommand { get; }

    /// <summary>Validates and persists the new patient and procedure. Raises <see cref="SaveCompleted"/> on success.</summary>
    ICommand SaveCommand { get; }

    /// <summary>Discards all input and raises <see cref="Cancelled"/>.</summary>
    ICommand CancelCommand { get; }

    // ── Events ───────────────────────────────────────────────────────────────

    /// <summary>Raised after the patient and procedure have been persisted successfully.</summary>
    event EventHandler? SaveCompleted;

    /// <summary>Raised when the user cancels without saving.</summary>
    event EventHandler? Cancelled;
}
