using HnVue.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Repositories;

/// <summary>
/// EF Core repository for CD burning study file paths in the HnVue.Data layer.
/// Queries <see cref="HnVueDbContext.Images"/> to resolve DICOM file paths for a given study.
/// For DI registration, use <c>HnVue.CDBurning.StudyRepository</c> which implements <c>HnVue.CDBurning.IStudyRepository</c>.
/// REQ-COORD-006: SPEC-COORDINATOR-001 EF Core CD study file path query.
/// </summary>
public sealed class EfCdStudyRepository(HnVueDbContext context)
{
    /// <summary>Returns absolute file paths to all DICOM images in the specified study.</summary>
    /// <remarks>SWR-DA-050: All image file paths for a study must be retrieved before disc burn starts.</remarks>
    public async Task<Result<IReadOnlyList<string>>> GetFilesForStudyAsync(
        string studyInstanceUid, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studyInstanceUid);

        try
        {
            var paths = await context.Images
                .AsNoTracking()
                .Where(i => i.StudyInstanceUid == studyInstanceUid)
                .Select(i => i.FilePath)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            IReadOnlyList<string> result = paths;
            return Result.Success(result);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<IReadOnlyList<string>>(
                ErrorCode.DatabaseError,
                $"Failed to retrieve image files for study '{studyInstanceUid}': {ex.Message}");
        }
    }
}
