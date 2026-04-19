using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HnVue.UI.DesignTime;

/// <summary>
/// Designer-only mock data for AddPatientProcedureView.
/// Keeps the VS2022 designer usable without running the application.
/// PPT Slide 8 (Add Patient / Procedure).
/// </summary>
public sealed partial class DesignTimeAddPatientProcedureViewModel : ObservableObject
{
    public DesignTimeAddPatientProcedureViewModel()
    {
        // Patient fields
        PatientId = "P-2026-042";
        IsPatientIdAutoGenerate = true;
        PatientName = "김영희";
        BirthDate = "1985-03-15";
        Gender = "F";

        // Accession / Study fields
        AccessionNumber = "ACC-20260419-001";
        IsAccNoAutoGenerate = false;
        StudyDescription = "흉부 X선 (Chest PA)";

        // View Projection chips — pre-populated for designer preview
        SelectedProjections =
        [
            "PA",
            "LAT",
        ];

        AvailableProjections =
        [
            "PA",
            "AP",
            "LAT",
            "OBL",
            "LL",
            "RL",
        ];

        // Description chips — pre-populated for designer preview
        SelectedDescriptions =
        [
            "정면 (Frontal)",
            "측면 (Lateral)",
        ];

        AvailableDescriptions =
        [
            "정면 (Frontal)",
            "측면 (Lateral)",
            "사면 (Oblique)",
            "좌측 (Left)",
            "우측 (Right)",
            "기립 (Upright)",
            "와위 (Supine)",
        ];

        DescriptionInput = null;
        RisCode = "RIS-45012";

        // Commands — no-op for design-time
        AddProjectionCommand = new RelayCommand<string?>(_ => { });
        RemoveProjectionCommand = new RelayCommand<string?>(_ => { });
        AddDescriptionCommand = new RelayCommand<string?>(_ => { });
        RemoveDescriptionCommand = new RelayCommand<string?>(_ => { });
        ToggleAccNoAutoGenerateCommand = new RelayCommand(() => IsAccNoAutoGenerate = !IsAccNoAutoGenerate);
        TogglePatientIdAutoGenerateCommand = new RelayCommand(() => IsPatientIdAutoGenerate = !IsPatientIdAutoGenerate);
        SaveCommand = new RelayCommand(() => { });
        CancelCommand = new RelayCommand(() => { });
    }

    // ── IViewModelBase ─────────────────────────────────────────────────────

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    // ── Patient fields ─────────────────────────────────────────────────────

    [ObservableProperty]
    private string _patientId = string.Empty;

    [ObservableProperty]
    private bool _isPatientIdAutoGenerate;

    [ObservableProperty]
    private string _patientName = string.Empty;

    [ObservableProperty]
    private string _birthDate = string.Empty;

    [ObservableProperty]
    private string _gender = string.Empty;

    // ── Accession / Study fields ────────────────────────────────────────────

    [ObservableProperty]
    private string _accessionNumber = string.Empty;

    [ObservableProperty]
    private bool _isAccNoAutoGenerate;

    [ObservableProperty]
    private string _studyDescription = string.Empty;

    // ── View Projection (multi-select chips) ────────────────────────────────

    public ObservableCollection<string> SelectedProjections { get; }

    public IReadOnlyList<string> AvailableProjections { get; }

    // ── Description (multi-select chips) ────────────────────────────────────

    public ObservableCollection<string> SelectedDescriptions { get; }

    public IReadOnlyList<string> AvailableDescriptions { get; }

    [ObservableProperty]
    private string? _descriptionInput;

    // ── RIS Code ────────────────────────────────────────────────────────────

    [ObservableProperty]
    private string _risCode = string.Empty;

    // ── Commands ────────────────────────────────────────────────────────────

    public ICommand AddProjectionCommand { get; }
    public ICommand RemoveProjectionCommand { get; }
    public ICommand AddDescriptionCommand { get; }
    public ICommand RemoveDescriptionCommand { get; }
    public ICommand ToggleAccNoAutoGenerateCommand { get; }
    public ICommand TogglePatientIdAutoGenerateCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    // ── Events (not raised in design-time) ──────────────────────────────────

    public event EventHandler? SaveCompleted;
    public event EventHandler? Cancelled;
}
