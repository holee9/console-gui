namespace HnVue.Common.Models;

/// <summary>
/// Controls image processing behaviour applied by <c>IImageProcessor</c>.
/// </summary>
/// <param name="WindowCenter">Override DICOM window centre value; null to use file metadata or auto.</param>
/// <param name="WindowWidth">Override DICOM window width value; null to use file metadata or auto.</param>
/// <param name="AutoWindow">
/// When true, the processor calculates the optimal window/level automatically from pixel statistics.
/// Ignored when both <see cref="WindowCenter"/> and <see cref="WindowWidth"/> are provided.
/// </param>
public sealed record ProcessingParameters(
    double? WindowCenter = null,
    double? WindowWidth = null,
    bool AutoWindow = true);
