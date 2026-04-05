using HnVue.Common.Results;
using HnVue.Data;
using Microsoft.EntityFrameworkCore;

namespace HnVue.CDBurning;

/// <summary>
/// EF Core implementation of <see cref="IStudyRepository"/>.
/// Resolves DICOM image file paths for a study from the <see cref="HnVueDbContext.Images"/> table.
/// </summary>
public sealed class StudyRepository : IStudyRepository
{
    private readonly HnVueDbContext _dbContext;

    /// <summary>
    /// Initialises a new <see cref="StudyRepository"/>.
    /// </summary>
    /// <param name="dbContext">EF Core database context.</param>
    public StudyRepository(HnVueDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc/>
    /// <remarks>SWR-DA-050: All image file paths for a study must be retrieved before disc burn starts.</remarks>
    public async Task<Result<IReadOnlyList<string>>> GetFilesForStudyAsync(
        string studyInstanceUid, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studyInstanceUid);

        try
        {
            var paths = await _dbContext.Images
                .AsNoTracking()
                .Where(i => i.StudyInstanceUid == studyInstanceUid)
                .Select(i => i.FilePath)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            IReadOnlyList<string> result = paths;
            return Result.Success(result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<IReadOnlyList<string>>(ErrorCode.DatabaseError,
                $"Failed to retrieve image files for study '{studyInstanceUid}': {ex.Message}");
        }
    }
}
