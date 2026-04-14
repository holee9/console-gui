using HnVue.Common.Models;

namespace HnVue.UI.Contracts.Models;

/// <summary>
/// UI presentation wrapper for <see cref="StudyRecord"/> with selection state.
/// Used in MergeView for checkbox binding (PPT Slide 12-13).
/// </summary>
public interface IStudyItem
{
    /// <summary>Gets the underlying study record.</summary>
    StudyRecord Study { get; }

    /// <summary>Gets or sets whether this study is selected for merge.</summary>
    bool IsSelected { get; set; }
}
