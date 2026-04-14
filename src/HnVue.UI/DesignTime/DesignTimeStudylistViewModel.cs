using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Models;

namespace HnVue.UI.DesignTime;

/// <summary>
/// Designer-only mock data for StudylistView.
/// Keeps the VS2022 designer usable without running the application.
/// PPT Slides 5-7 (Studylist).
/// </summary>
public sealed partial class DesignTimeStudylistViewModel : ObservableObject
{
    public DesignTimeStudylistViewModel()
    {
        NavigatePreviousCommand = new RelayCommand(() => { });
        NavigateNextCommand = new RelayCommand(() => { });
        FilterByPeriodCommand = new RelayCommand<string?>(_ => { });
        LoadStudiesCommand = new RelayCommand(() => { });
        SelectStudyCommand = new RelayCommand<StudyRecord?>(_ => { });

        Studies = new ObservableCollection<StudyRecord>(CreateSampleStudies());
    }

    // Study list
    public ObservableCollection<StudyRecord> Studies { get; }

    [ObservableProperty]
    private StudyRecord? _selectedStudy;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _activePeriodFilter = "All";

    [ObservableProperty]
    private string? _selectedPacsServer = "LOCAL";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    public IReadOnlyList<string> PacsServers { get; } = new[] { "LOCAL", "PACS-01", "PACS-02" };

    // Summary counts (Design Team addition)
    public int CompletedCount { get; } = 18;
    public int InProgressCount { get; } = 5;
    public int CancelledCount { get; } = 2;
    public int ReportedCount { get; } = 9;

    // Commands
    public ICommand NavigatePreviousCommand { get; }
    public ICommand NavigateNextCommand { get; }
    public ICommand FilterByPeriodCommand { get; }
    public ICommand LoadStudiesCommand { get; }
    public ICommand SelectStudyCommand { get; }

    private static List<StudyRecord> CreateSampleStudies() =>
    [
        new("1.2.840.113619.2.55.1.1", "P-2026-001", new DateTimeOffset(2026, 4, 14, 9, 30, 0, TimeSpan.FromHours(9)), "Chest PA", "ACC-001", "CHEST"),
        new("1.2.840.113619.2.55.1.2", "P-2026-001", new DateTimeOffset(2026, 4, 14, 10, 15, 0, TimeSpan.FromHours(9)), "Abdomen", "ACC-002", "ABDOMEN"),
        new("1.2.840.113619.2.55.1.3", "P-2026-002", new DateTimeOffset(2026, 4, 13, 14, 0, 0, TimeSpan.FromHours(9)), "Skull LAT", "ACC-003", "SKULL"),
        new("1.2.840.113619.2.55.1.4", "P-2026-003", new DateTimeOffset(2026, 4, 12, 11, 45, 0, TimeSpan.FromHours(9)), "Pelvis AP", "ACC-004", "PELVIS"),
        new("1.2.840.113619.2.55.1.5", "P-2026-003", new DateTimeOffset(2026, 4, 11, 8, 0, 0, TimeSpan.FromHours(9)), "Spine LAT", "ACC-005", "SPINE"),
    ];
}
