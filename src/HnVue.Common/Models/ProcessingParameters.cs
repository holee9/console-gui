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
/// <param name="RawImageWidth">
/// Explicit pixel width for raw (non-DICOM) image files.
/// When provided together with <see cref="RawImageHeight"/>, overrides the sqrt-estimation fallback.
/// Required for correct processing of DR detector output (e.g., 2560×2048). Issue #7.
/// </param>
/// <param name="RawImageHeight">
/// Explicit pixel height for raw (non-DICOM) image files. See <see cref="RawImageWidth"/>. Issue #7.
/// </param>
public sealed record ProcessingParameters(
    double? WindowCenter = null,
    double? WindowWidth = null,
    bool AutoWindow = true,
    int? RawImageWidth = null,
    int? RawImageHeight = null);
