using System.Windows.Input;

namespace HnVue.UI.Contracts.ViewModels;

/// <summary>Contract for the image viewer ViewModel.</summary>
/// <remarks>
/// ImageSource is intentionally not in this contract because it is a WPF-specific type (BitmapSource).
/// The View should bind to ImageSource on the concrete ViewModel via DataContext.
/// Future alternative UIs may use a different image representation.
/// </remarks>
public interface IImageViewerViewModel : IViewModelBase
{
    /// <summary>Gets the file-system path of the currently loaded image, or null if none is loaded.</summary>
    string? CurrentImagePath { get; }

    /// <summary>Gets or sets the window-level (centre) value for windowing.</summary>
    double WindowLevel { get; set; }

    /// <summary>Gets or sets the window-width value for windowing.</summary>
    double WindowWidth { get; set; }

    /// <summary>Gets a value indicating whether an image is currently loaded.</summary>
    bool IsImageLoaded { get; }

    /// <summary>Gets the current zoom factor applied to the viewport.</summary>
    double ZoomFactor { get; }

    /// <summary>Gets a value indicating whether a long-running image operation is in progress.</summary>
    bool IsBusy { get; }

    /// <summary>Gets the command that loads an image from a given path.</summary>
    ICommand LoadImageCommand { get; }

    /// <summary>Gets the command that increases the zoom level.</summary>
    ICommand ZoomInCommand { get; }

    /// <summary>Gets the command that decreases the zoom level.</summary>
    ICommand ZoomOutCommand { get; }

    /// <summary>Gets the command that resets windowing to the default values.</summary>
    ICommand ResetWindowCommand { get; }
}
