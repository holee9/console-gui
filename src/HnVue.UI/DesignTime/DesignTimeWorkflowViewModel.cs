using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HnVue.UI.DesignTime;

/// <summary>
/// Designer-only mock data for WorkflowView (Acquisition screen).
/// Keeps the VS2022 designer usable without running the application.
/// PPT Slides 9-11.
/// </summary>
public sealed partial class DesignTimeWorkflowViewModel : ObservableObject
{
    public DesignTimeWorkflowViewModel()
    {
        StartAcquisitionCommand = new RelayCommand(() => { });
        StopAcquisitionCommand = new RelayCommand(() => { });
        SaveCommand = new RelayCommand(() => { });
        CancelCommand = new RelayCommand(() => { });

        SelectedPatient = new DesignTimePatient
        {
            Name = "홍길동",
            PatientId = "P-2026-001",
            DateOfBirth = new DateTimeOffset(1980, 5, 15, 0, 0, 0, TimeSpan.FromHours(9)),
            Sex = "M",
            IsEmergency = false
        };

        ThumbnailList = new ObservableCollection<DesignTimeThumbnail>(CreateSampleThumbnails());
    }

    [ObservableProperty]
    private DesignTimePatient? _selectedPatient;

    public ObservableCollection<DesignTimeThumbnail> ThumbnailList { get; }

    [ObservableProperty]
    private int _selectedThumbnailIndex;

    [ObservableProperty]
    private bool _isAcquiring;

    [ObservableProperty]
    private string? _statusMessage = "준비 완료";

    [ObservableProperty]
    private string _acquisitionTime = DateTime.Now.ToString("HH:mm:ss");

    // Commands
    public ICommand StartAcquisitionCommand { get; }
    public ICommand StopAcquisitionCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private static List<DesignTimeThumbnail> CreateSampleThumbnails() =>
    [
        new() { Description = "Chest PA", ImageCount = 2 },
        new() { Description = "Abdomen AP", ImageCount = 3 },
        new() { Description = "Skull LAT", ImageCount = 1 },
        new() { Description = "Pelvis", ImageCount = 2 },
        new() { Description = "Spine", ImageCount = 4 },
        new() { Description = "Extremity", ImageCount = 1 },
    ];

    /// <summary>Mock patient record for WorkflowView design-time rendering.</summary>
    public sealed class DesignTimePatient
    {
        public required string Name { get; init; }
        public required string PatientId { get; init; }
        public required DateTimeOffset DateOfBirth { get; init; }
        public required string Sex { get; init; }
        public required bool IsEmergency { get; init; }
    }

    /// <summary>Mock thumbnail record for WorkflowView design-time rendering.</summary>
    public sealed class DesignTimeThumbnail
    {
        public required string Description { get; init; }
        public required int ImageCount { get; init; }
    }
}
