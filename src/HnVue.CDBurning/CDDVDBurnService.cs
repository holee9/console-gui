using System.IO;
using HnVue.Common.Abstractions;
using HnVue.Common.Results;

namespace HnVue.CDBurning;

/// <summary>
/// Implements optical disc burning for DICOM study export.
/// </summary>
/// <remarks>
/// Retrieves DICOM files for the study, checks disc readiness, burns, and verifies.
/// IEC 62304 Class B — disc integrity affects patient data portability.
/// </remarks>
public sealed class CDDVDBurnService : ICDDVDBurnService
{
    private readonly IBurnSession _burnSession;
    private readonly IStudyRepository _studyRepository;

    /// <summary>
    /// Initialises a new <see cref="CDDVDBurnService"/>.
    /// </summary>
    public CDDVDBurnService(IBurnSession burnSession, IStudyRepository studyRepository)
    {
        _burnSession = burnSession ?? throw new ArgumentNullException(nameof(burnSession));
        _studyRepository = studyRepository ?? throw new ArgumentNullException(nameof(studyRepository));
    }

    /// <inheritdoc/>
    public async Task<Result> BurnStudyAsync(
        string studyInstanceUid,
        string outputLabel,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studyInstanceUid);
        ArgumentNullException.ThrowIfNull(outputLabel);

        if (string.IsNullOrWhiteSpace(studyInstanceUid))
            return Result.Failure(ErrorCode.ValidationFailed, "Study Instance UID is required.");

        if (outputLabel.Length > 32)
            return Result.Failure(ErrorCode.ValidationFailed,
                "Volume label must be 32 characters or fewer (ISO 9660 limit).");

        // Check disc readiness
        var discCheck = await _burnSession.IsDiscInsertedAsync(cancellationToken).ConfigureAwait(false);
        if (discCheck.IsFailure || !discCheck.Value)
            return Result.Failure(ErrorCode.BurnFailed, "No disc inserted. Please insert a blank disc.");

        var blankCheck = await _burnSession.IsDiscBlankAsync(cancellationToken).ConfigureAwait(false);
        if (blankCheck.IsFailure || !blankCheck.Value)
            return Result.Failure(ErrorCode.BurnFailed, "Disc is not blank.");

        // Get study files
        var filesResult = await _studyRepository.GetFilesForStudyAsync(studyInstanceUid, cancellationToken)
            .ConfigureAwait(false);

        if (filesResult.IsFailure)
            return Result.Failure(filesResult.Error!.Value, filesResult.ErrorMessage!);

        if (!filesResult.Value.Any())
            return Result.Failure(ErrorCode.NotFound,
                $"No files found for study '{studyInstanceUid}'.");

        var burnEntries = filesResult.Value
            .Select((f, i) => new BurnFileEntry(
                SourcePath: f,
                DiscPath: $"DICOM\\{Path.GetFileName(f)}",
                SizeBytes: new System.IO.FileInfo(f).Length))
            .ToList();

        // Burn
        var burnResult = await _burnSession.BurnFilesAsync(
            burnEntries, outputLabel, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (burnResult.IsFailure)
            return Result.Failure(burnResult.Error!.Value, burnResult.ErrorMessage!);

        // Verify
        var verifyResult = await _burnSession.VerifyAsync(burnEntries, cancellationToken).ConfigureAwait(false);

        return verifyResult.IsSuccess && verifyResult.Value
            ? Result.Success()
            : Result.Failure(ErrorCode.DiscVerificationFailed, "Post-burn disc verification failed.");
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> VerifyDiscAsync(
        CancellationToken cancellationToken = default)
    {
        var insertedResult = await _burnSession.IsDiscInsertedAsync(cancellationToken).ConfigureAwait(false);

        if (insertedResult.IsFailure)
            return Result.Failure<bool>(ErrorCode.DiscVerificationFailed,
                "Cannot verify disc: unable to detect disc presence.");

        if (!insertedResult.Value)
            return Result.Success(false);

        return await _burnSession.VerifyAsync(
            Enumerable.Empty<BurnFileEntry>(), cancellationToken).ConfigureAwait(false);
    }
}
