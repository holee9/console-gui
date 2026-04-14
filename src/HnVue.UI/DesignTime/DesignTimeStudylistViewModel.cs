using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Models;

namespace HnVue.UI.DesignTime;

/// <summary>
/// Designer-only mock data for StudylistView.
/// Keeps the VS designer usable without running the application.
/// </summary>
public sealed class DesignTimeStudylistViewModel
{
    public DesignTimeStudylistViewModel()
    {
        Studies = new ObservableCollection<StudyRecord>
        {
            new(
                "1.2.840.113619.2.55.3.604688123.20260411.1",
                "P2026-0142",
                new DateTimeOffset(2026, 4, 11, 8, 35, 0, TimeSpan.FromHours(9)),
                "Chest PA Follow-up",
                "ACC-20260411-0142",
                "CHEST"),
            new(
                "1.2.840.113619.2.55.3.604688123.20260410.7",
                "P2026-0142",
                new DateTimeOffset(2026, 4, 10, 15, 10, 0, TimeSpan.FromHours(9)),
                "L-Spine AP/LAT",
                "ACC-20260410-0098",
                "L-SPINE"),
            new(
                "1.2.840.113619.2.55.3.604688123.20260408.4",
                "P2026-0218",
                new DateTimeOffset(2026, 4, 8, 11, 20, 0, TimeSpan.FromHours(9)),
                "Knee Both AP/LAT",
                "ACC-20260408-0218",
                "KNEE"),
            new(
                "1.2.840.113619.2.55.3.604688123.20260406.2",
                "P2026-0305",
                new DateTimeOffset(2026, 4, 6, 10, 5, 0, TimeSpan.FromHours(9)),
                "Abdomen Supine",
                "ACC-20260406-0305",
                "ABDOMEN"),
        };

        SelectedStudy = Studies[0];

        NavigatePreviousCommand = new RelayCommand(() => { });
        NavigateNextCommand = new RelayCommand(() => { });
        FilterByPeriodCommand = new RelayCommand<string?>(period =>
        {
            if (!string.IsNullOrWhiteSpace(period))
            {
                ActivePeriodFilter = period;
            }
        });
        LoadStudiesCommand = new RelayCommand(() => { });
        SelectStudyCommand = new RelayCommand<StudyRecord?>(study =>
        {
            if (study is not null)
            {
                SelectedStudy = study;
            }
        });
    }

    public ObservableCollection<StudyRecord> Studies { get; }

    public StudyRecord? SelectedStudy { get; set; }

    public string SearchQuery { get; set; } = "P2026";

    public string ActivePeriodFilter { get; set; } = "Today";

    public IReadOnlyList<string> PacsServers { get; } = new[] { "LOCAL", "PACS-01", "PACS-02" };

    public string? SelectedPacsServer { get; set; } = "LOCAL";

    public bool IsLoading { get; set; }

    public string? ErrorMessage { get; set; }

    public int CompletedCount { get; } = 18;

    public int InProgressCount { get; } = 5;

    public int CancelledCount { get; } = 2;

    public int ReportedCount { get; } = 9;

    public ICommand NavigatePreviousCommand { get; }

    public ICommand NavigateNextCommand { get; }

    public ICommand FilterByPeriodCommand { get; }

    public ICommand LoadStudiesCommand { get; }

    public ICommand SelectStudyCommand { get; }
}
