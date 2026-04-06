using System.Windows.Input;

namespace HnVue.UI.Contracts.ViewModels;

/// <summary>Contract for the CD/DVD burn ViewModel.</summary>
public interface ICDBurnViewModel : IViewModelBase
{
    /// <summary>Gets or sets the identifier of the study selected for burning.</summary>
    string? SelectedStudyId { get; set; }

    /// <summary>Gets a value indicating whether a burn operation is currently in progress.</summary>
    bool IsBurning { get; }

    /// <summary>Gets the burn progress as a percentage value between 0 and 100.</summary>
    int BurnProgress { get; }

    /// <summary>Gets the human-readable status message for the current burn operation.</summary>
    string StatusMessage { get; }

    /// <summary>Gets the command that initiates the burn process.</summary>
    ICommand StartBurnCommand { get; }

    /// <summary>Gets the command that cancels an in-progress burn operation.</summary>
    ICommand CancelBurnCommand { get; }
}
