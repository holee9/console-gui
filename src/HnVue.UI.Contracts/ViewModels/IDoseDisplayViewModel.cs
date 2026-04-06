using System.Collections.ObjectModel;
using System.Windows.Input;
using HnVue.Common.Models;

namespace HnVue.UI.Contracts.ViewModels;

/// <summary>Contract for the dose display panel ViewModel.</summary>
public interface IDoseDisplayViewModel : IViewModelBase
{
    /// <summary>Gets the current dose-area product value in mGy·cm².</summary>
    double CurrentDoseDap { get; }

    /// <summary>Gets the observable collection of dose records for the current session.</summary>
    ObservableCollection<DoseRecord> DoseHistory { get; }

    /// <summary>Gets or sets the diagnostic reference level (DRL) threshold in mGy·cm².</summary>
    double DrlReferenceLevel { get; set; }

    /// <summary>Gets a value indicating whether the current dose has exceeded the DRL threshold.</summary>
    bool IsDoseAlert { get; }

    /// <summary>Gets a value indicating whether a dose refresh operation is in progress.</summary>
    bool IsRefreshing { get; }

    /// <summary>Gets the command that refreshes dose data from the underlying service.</summary>
    ICommand RefreshCommand { get; }
}
