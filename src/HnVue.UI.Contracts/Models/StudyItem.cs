using CommunityToolkit.Mvvm.ComponentModel;
using HnVue.Common.Models;

namespace HnVue.UI.Contracts.Models;

/// <summary>
/// UI presentation wrapper for <see cref="StudyRecord"/> with selection state.
/// Used in MergeView for checkbox binding (PPT Slide 12-13).
/// </summary>
public sealed partial class StudyItem : ObservableObject
{
    /// <summary>Gets the underlying study record.</summary>
    public StudyRecord Study { get; }

    /// <summary>Initializes a new instance with the specified study record.</summary>
    public StudyItem(StudyRecord study) => Study = study;

    /// <summary>Gets or sets whether this study is selected for merge.</summary>
    [ObservableProperty]
    private bool _isSelected;
}
