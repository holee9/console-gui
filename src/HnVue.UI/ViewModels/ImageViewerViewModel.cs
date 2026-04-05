using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;

namespace HnVue.UI.ViewModels;

/// <summary>
/// ViewModel for the radiographic image viewer.
/// Wraps <see cref="IImageProcessor"/> to load, window, and zoom images.
/// </summary>
public sealed partial class ImageViewerViewModel : ObservableObject
{
    private const double ZoomStep = 0.25;
    private const double DefaultZoom = 1.0;

    private readonly IImageProcessor _imageProcessor;
    private ProcessedImage? _currentImage;

    /// <summary>Initialises a new instance of <see cref="ImageViewerViewModel"/>.</summary>
    /// <param name="imageProcessor">Performs image loading and manipulation operations.</param>
    public ImageViewerViewModel(IImageProcessor imageProcessor)
    {
        _imageProcessor = imageProcessor;
    }

    /// <summary>Gets or sets the file-system path of the currently loaded image, or <see langword="null"/>.</summary>
    [ObservableProperty]
    private string? _currentImagePath;

    /// <summary>Gets or sets the DICOM window centre applied to the displayed image.</summary>
    [ObservableProperty]
    private double _windowLevel = 2048;

    /// <summary>Gets or sets the DICOM window width applied to the displayed image.</summary>
    [ObservableProperty]
    private double _windowWidth = 4096;

    /// <summary>Gets or sets a value indicating whether an image has been successfully loaded.</summary>
    [ObservableProperty]
    private bool _isImageLoaded;

    /// <summary>Gets or sets the current zoom factor (1.0 = 100%).</summary>
    [ObservableProperty]
    private double _zoomFactor = DefaultZoom;

    /// <summary>Gets or sets a message describing the most recent error, or <see langword="null"/> on success.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>Gets or sets a value indicating whether an operation is in progress.</summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Loads the image at the specified file path and applies default windowing.
    /// </summary>
    /// <param name="imagePath">Absolute path to the raw DICOM or proprietary image file.</param>
    [RelayCommand]
    private async Task LoadImageAsync(string imagePath)
    {
        IsBusy = true;
        ErrorMessage = null;
        IsImageLoaded = false;

        try
        {
            var parameters = new ProcessingParameters(AutoWindow: true);
            var result = await _imageProcessor.ProcessAsync(imagePath, parameters);

            if (result.IsSuccess)
            {
                _currentImage = result.Value;
                CurrentImagePath = imagePath;
                WindowLevel = result.Value.WindowCenter;
                WindowWidth = result.Value.WindowWidth;
                ZoomFactor = DefaultZoom;
                IsImageLoaded = true;
            }
            else
            {
                ErrorMessage = result.ErrorMessage;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Increases the zoom factor by <see cref="ZoomStep"/>.</summary>
    [RelayCommand]
    private void ZoomIn()
    {
        if (_currentImage is null) return;
        var zoomed = _imageProcessor.Zoom(_currentImage, ZoomFactor + ZoomStep);
        if (zoomed.IsSuccess)
        {
            _currentImage = zoomed.Value;
            ZoomFactor += ZoomStep;
        }
    }

    /// <summary>Decreases the zoom factor by <see cref="ZoomStep"/>, minimum 0.25.</summary>
    [RelayCommand]
    private void ZoomOut()
    {
        if (_currentImage is null) return;
        var newFactor = Math.Max(0.25, ZoomFactor - ZoomStep);
        var zoomed = _imageProcessor.Zoom(_currentImage, newFactor);
        if (zoomed.IsSuccess)
        {
            _currentImage = zoomed.Value;
            ZoomFactor = newFactor;
        }
    }

    /// <summary>Resets windowing to the default values of the loaded image.</summary>
    [RelayCommand]
    private void ResetWindow()
    {
        if (_currentImage is null) return;
        WindowLevel = _currentImage.WindowCenter;
        WindowWidth = _currentImage.WindowWidth;
    }

    partial void OnWindowLevelChanged(double value) => ApplyWindowLevel();
    partial void OnWindowWidthChanged(double value) => ApplyWindowLevel();

    private void ApplyWindowLevel()
    {
        // Guard: _currentImage is set during initial load; skip if not yet assigned.
        if (_currentImage is null || !IsImageLoaded) return;
        var result = _imageProcessor.ApplyWindowLevel(_currentImage, WindowLevel, WindowWidth);
        if (result.IsSuccess)
        {
            _currentImage = result.Value;
        }
    }
}
