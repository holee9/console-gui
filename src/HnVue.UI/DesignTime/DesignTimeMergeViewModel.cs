using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HnVue.UI.DesignTime;

/// <summary>
/// Designer-only mock data for MergeView (Sync Study dialog).
/// Keeps the VS2022 designer usable without running the application.
/// PPT Slides 12-13.
/// </summary>
public sealed partial class DesignTimeMergeViewModel : ObservableObject
{
    public DesignTimeMergeViewModel()
    {
        SearchACommand = new RelayCommand(() => { });
        SearchBCommand = new RelayCommand(() => { });
        CancelCommand = new RelayCommand(() => { });
        SyncStudyCommand = new RelayCommand(() => { });

        PatientsA = new ObservableCollection<DesignTimeMergePatient>(CreateSamplePatientsA());
        PatientsB = new ObservableCollection<DesignTimeMergePatient>(CreateSamplePatientsB());

        SelectedPatientA = PatientsA.FirstOrDefault();
        SelectedPatientB = PatientsB.Skip(1).FirstOrDefault();
    }

    // Patient A (left column)
    public ObservableCollection<DesignTimeMergePatient> PatientsA { get; }

    [ObservableProperty]
    private DesignTimeMergePatient? _selectedPatientA;

    [ObservableProperty]
    private string _searchQueryA = string.Empty;

    // Patient B (right column)
    public ObservableCollection<DesignTimeMergePatient> PatientsB { get; }

    [ObservableProperty]
    private DesignTimeMergePatient? _selectedPatientB;

    [ObservableProperty]
    private string _searchQueryB = string.Empty;

    // Commands
    public ICommand SearchACommand { get; }
    public ICommand SearchBCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SyncStudyCommand { get; }

    private static List<DesignTimeMergePatient> CreateSamplePatientsA() =>
    [
        new() { Name = "홍^길동", PatientId = "P-2026-001" },
        new() { Name = "김^철수", PatientId = "P-2026-002" },
        new() { Name = "이^영희", PatientId = "P-2026-003" },
        new() { Name = "박^민수", PatientId = "P-2026-004" },
        new() { Name = "최^동현", PatientId = "P-2026-005" },
    ];

    private static List<DesignTimeMergePatient> CreateSamplePatientsB() =>
    [
        new() { Name = "홍^길동", PatientId = "P-2026-001" },
        new() { Name = "김^철수", PatientId = "P-2026-002" },
        new() { Name = "이^영희", PatientId = "P-2026-003" },
        new() { Name = "박^민수", PatientId = "P-2026-004" },
        new() { Name = "최^동현", PatientId = "P-2026-005" },
    ];

    /// <summary>Mock patient record for MergeView design-time rendering.</summary>
    /// <remarks>
    /// Name format: DICOM PN (Person Name) = family^given^middle^prefix^suffix
    /// Example: "홍^길동" (Hong^Gildong)
    /// </remarks>
    public sealed class DesignTimeMergePatient
    {
        public required string Name { get; init; }
        public required string PatientId { get; init; }
    }
}
