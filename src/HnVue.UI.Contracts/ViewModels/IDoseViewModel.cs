using System.Windows.Input;
using HnVue.Common.Enums;
using HnVue.Common.Models;

namespace HnVue.UI.Contracts.ViewModels;

/// <summary>Contract for the dose monitor panel ViewModel.</summary>
public interface IDoseViewModel : IViewModelBase
{
    /// <summary>Gets the most recently retrieved dose record, or null if none is available.</summary>
    DoseRecord? CurrentDose { get; }

    /// <summary>Gets the validation level derived from the current dose against reference limits.</summary>
    DoseValidationLevel ValidationLevel { get; }

    /// <summary>Gets or sets the DICOM Study Instance UID used to scope dose queries.</summary>
    string? ActiveStudyInstanceUid { get; set; }

    /// <summary>Gets the command that triggers an on-demand dose refresh.</summary>
    ICommand RefreshCommand { get; }

    /// <summary>Loads dose data scoped to the specified DICOM study.</summary>
    /// <param name="studyInstanceUid">The Study Instance UID to query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task LoadForStudyAsync(string studyInstanceUid, CancellationToken cancellationToken = default);
}
