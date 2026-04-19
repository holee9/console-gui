using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.UI.Contracts.ViewModels;

namespace HnVue.UI.ViewModels;

// @MX:NOTE AddPatientProcedureViewModel — PPT slide 8: merged Add Patient + Procedure dialog.
//          Auto-generate logic for PatientId and AccessionNumber.
//          Projection/Description chips via ObservableCollection.
//          Persists via IPatientService.RegisterAsync.
/// <summary>
/// ViewModel for the merged Add Patient/Procedure dialog.
/// PPT slide 8: unified window with auto-generate toggles, projection chips, and description chips.
/// </summary>
public sealed partial class AddPatientProcedureViewModel : ObservableObject, IAddPatientProcedureViewModel
{
    private readonly IPatientService _patientService;
    private readonly ISecurityContext _securityContext;
    private readonly List<string> _availableProjections;
    private readonly List<string> _availableDescriptions;

    /// <summary>Initialises a new instance of <see cref="AddPatientProcedureViewModel"/>.</summary>
    /// <param name="patientService">Service used to register the new patient record.</param>
    /// <param name="securityContext">Security context providing the current operator ID for audit.</param>
    public AddPatientProcedureViewModel(IPatientService patientService, ISecurityContext securityContext)
    {
        _patientService = patientService;
        _securityContext = securityContext;

        _availableProjections = new List<string>
        {
            "Chest PA", "Chest Lateral", "Chest Series",
            "Hand PA", "Hand Lateral", "Hand Series",
            "Spine AP", "Spine Lateral",
            "Knee AP", "Knee Lateral",
            "Abdomen AP", "Pelvis AP",
            "Skull AP", "Skull Lateral",
            "Foot AP", "Ankle AP"
        };

        _availableDescriptions = new List<string>
        {
            "Routine", "Emergency", "Follow-up",
            "Pre-op", "Post-op", "Screening",
            "Trauma", "Pediatric"
        };

        // Apply initial auto-generate values
        _patientId = GeneratePatientId();
        _accessionNumber = GenerateAccNo();
    }

    // ── IViewModelBase explicit implementation ───────────────────────────────

    bool IViewModelBase.IsLoading => IsLoading;

    // ── IAddPatientProcedureViewModel command explicit mapping ──────────────

    ICommand IAddPatientProcedureViewModel.AddProjectionCommand => AddProjectionCommand;
    ICommand IAddPatientProcedureViewModel.RemoveProjectionCommand => RemoveProjectionCommand;
    ICommand IAddPatientProcedureViewModel.AddDescriptionCommand => AddDescriptionCommand;
    ICommand IAddPatientProcedureViewModel.RemoveDescriptionCommand => RemoveDescriptionCommand;
    ICommand IAddPatientProcedureViewModel.ToggleAccNoAutoGenerateCommand => ToggleAccNoAutoGenerateCommand;
    ICommand IAddPatientProcedureViewModel.TogglePatientIdAutoGenerateCommand => TogglePatientIdAutoGenerateCommand;
    ICommand IAddPatientProcedureViewModel.SaveCommand => SaveCommand;
    ICommand IAddPatientProcedureViewModel.CancelCommand => CancelCommand;

    // ── Patient fields ───────────────────────────────────────────────────────

    /// <summary>Gets or sets the patient identifier.</summary>
    [ObservableProperty]
    private string _patientId;

    /// <summary>Gets or sets a value indicating whether PatientId is auto-generated.</summary>
    [ObservableProperty]
    private bool _isPatientIdAutoGenerate = true;

    /// <summary>Gets or sets the patient full name. Required (*).</summary>
    [ObservableProperty]
    private string _patientName = string.Empty;

    /// <summary>Gets or sets the patient birth date (yyyy-MM-dd). Required (*).</summary>
    [ObservableProperty]
    private string _birthDate = string.Empty;

    /// <summary>Gets or sets the patient gender (M/F/Other). Required (*).</summary>
    [ObservableProperty]
    private string _gender = string.Empty;

    // ── Accession / Study fields ─────────────────────────────────────────────

    /// <summary>Gets or sets the accession number.</summary>
    [ObservableProperty]
    private string _accessionNumber;

    /// <summary>Gets or sets a value indicating whether AccessionNumber is auto-generated.</summary>
    [ObservableProperty]
    private bool _isAccNoAutoGenerate = true;

    /// <summary>Gets or sets the study description.</summary>
    [ObservableProperty]
    private string _studyDescription = string.Empty;

    // ── RIS Code ─────────────────────────────────────────────────────────────

    /// <summary>Gets or sets the RIS code for real-time inline add/update.</summary>
    [ObservableProperty]
    private string _risCode = string.Empty;

    // ── Description chip input ───────────────────────────────────────────────

    /// <summary>Gets or sets the current text entered in the description input field.</summary>
    [ObservableProperty]
    private string? _descriptionInput;

    // ── Loading / error state ────────────────────────────────────────────────

    /// <summary>Gets or sets a value indicating whether a save operation is in progress.</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>Gets or sets a message describing the most recent error, or <see langword="null"/> on success.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    // ── Collections ──────────────────────────────────────────────────────────

    /// <summary>Gets the collection of selected View Projection chips.</summary>
    public ObservableCollection<string> SelectedProjections { get; } = new();

    /// <summary>Gets the collection of selected Description chips.</summary>
    public ObservableCollection<string> SelectedDescriptions { get; } = new();

    /// <summary>Gets the list of all available projection options.</summary>
    public IReadOnlyList<string> AvailableProjections => _availableProjections;

    /// <summary>Gets the list of all available description options.</summary>
    public IReadOnlyList<string> AvailableDescriptions => _availableDescriptions;

    // ── Events ───────────────────────────────────────────────────────────────

    /// <summary>Raised after the patient has been persisted successfully.</summary>
    public event EventHandler? SaveCompleted;

    /// <summary>Raised when the user cancels the dialog.</summary>
    public event EventHandler? Cancelled;

    // ── Commands ─────────────────────────────────────────────────────────────

    /// <summary>Adds a projection to the chip list if not already present.</summary>
    [RelayCommand]
    private void AddProjection(string? projection)
    {
        if (!string.IsNullOrWhiteSpace(projection) && !SelectedProjections.Contains(projection))
            SelectedProjections.Add(projection);
    }

    /// <summary>Removes a projection from the chip list.</summary>
    [RelayCommand]
    private void RemoveProjection(string? projection)
    {
        if (!string.IsNullOrWhiteSpace(projection))
            SelectedProjections.Remove(projection);
    }

    /// <summary>Adds a description chip from the dropdown or the manual input field.</summary>
    [RelayCommand]
    private void AddDescription(string? description)
    {
        var desc = description ?? DescriptionInput;
        if (!string.IsNullOrWhiteSpace(desc) && !SelectedDescriptions.Contains(desc))
        {
            SelectedDescriptions.Add(desc);
            DescriptionInput = string.Empty;
        }
    }

    /// <summary>Removes a description chip.</summary>
    [RelayCommand]
    private void RemoveDescription(string? description)
    {
        if (!string.IsNullOrWhiteSpace(description))
            SelectedDescriptions.Remove(description);
    }

    /// <summary>Toggles AccessionNumber auto-generation on/off.</summary>
    [RelayCommand]
    private void ToggleAccNoAutoGenerate()
    {
        IsAccNoAutoGenerate = !IsAccNoAutoGenerate;
    }

    /// <summary>Toggles PatientId auto-generation on/off.</summary>
    [RelayCommand]
    private void TogglePatientIdAutoGenerate()
    {
        IsPatientIdAutoGenerate = !IsPatientIdAutoGenerate;
    }

    /// <summary>Validates required fields and saves the patient record asynchronously.</summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (!ValidateRequiredFields())
            return;

        IsLoading = true;
        try
        {
            // Map ViewModel fields to PatientRecord positional record.
            // DateOfBirth: parse yyyy-MM-dd; fall back to null on parse failure.
            DateOnly? dob = DateOnly.TryParseExact(BirthDate, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var parsed)
                ? parsed
                : null;

            var record = new PatientRecord(
                PatientId: PatientId,
                Name: PatientName,
                DateOfBirth: dob,
                Sex: Gender,
                IsEmergency: false,
                CreatedAt: DateTimeOffset.UtcNow,
                CreatedBy: _securityContext.CurrentUserId ?? string.Empty);

            var result = await _patientService.RegisterAsync(record);
            if (result.IsSuccess)
            {
                SaveCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Registration failed.";
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Cancels the dialog without saving.</summary>
    [RelayCommand]
    private void Cancel() => Cancelled?.Invoke(this, EventArgs.Empty);

    // ── Property change reactions ────────────────────────────────────────────

    partial void OnIsPatientIdAutoGenerateChanged(bool value)
    {
        if (value)
            PatientId = GeneratePatientId();
    }

    partial void OnIsAccNoAutoGenerateChanged(bool value)
    {
        if (value)
            AccessionNumber = GenerateAccNo();
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private bool ValidateRequiredFields()
    {
        if (string.IsNullOrWhiteSpace(PatientName))
        {
            ErrorMessage = "Patient Name is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(BirthDate))
        {
            ErrorMessage = "Birth Date is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Gender))
        {
            ErrorMessage = "Gender is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(AccessionNumber))
        {
            ErrorMessage = "Accession Number is required.";
            return false;
        }

        if (SelectedProjections.Count == 0)
        {
            ErrorMessage = "At least one Procedure (View Projection) is required.";
            return false;
        }

        return true;
    }

    private static string GenerateAccNo()
        => $"ACC{DateTime.Now:yyyyMMddHHmmss}";

    // Non-security use: generates a display-only patient ID suffix for UI purposes.
    // SCS0005 suppressed — Random is not used for cryptographic or security-sensitive operations.
#pragma warning disable SCS0005
    private static string GeneratePatientId()
        => $"PT{DateTime.Now:yyyyMMdd}{Random.Shared.Next(1000, 9999)}";
#pragma warning restore SCS0005
}
