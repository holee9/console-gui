using System.ComponentModel;

namespace HnVue.UI.Contracts.ViewModels;

/// <summary>
/// Base contract for all ViewModels. Provides common loading and error state.
/// </summary>
public interface IViewModelBase : INotifyPropertyChanged
{
    /// <summary>Gets a value indicating whether an operation is in progress.</summary>
    bool IsLoading { get; }

    /// <summary>Gets the current error message, or null if no error.</summary>
    string? ErrorMessage { get; }
}
