namespace HnVue.Common.Models;

/// <summary>
/// Provides data for the <see cref="HnVue.Common.Abstractions.IDetectorInterface.ImageAcquired"/> event.
/// Raised by the detector driver when a full image frame has been read out from the panel.
/// </summary>
public sealed class ImageAcquiredEventArgs : EventArgs
{
    /// <summary>Gets the raw image data delivered by the detector.</summary>
    public RawDetectorImage Image { get; }

    /// <summary>Initializes a new instance with the acquired image.</summary>
    public ImageAcquiredEventArgs(RawDetectorImage image)
    {
        Image = image ?? throw new ArgumentNullException(nameof(image));
    }
}
