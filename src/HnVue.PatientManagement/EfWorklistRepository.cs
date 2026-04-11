using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using Microsoft.EntityFrameworkCore;

namespace HnVue.PatientManagement;

/// <summary>
/// EF Core implementation of <see cref="IWorklistRepository"/>.
/// Queries today's scheduled studies from the local database and maps to WorklistItem.
/// For DICOM MWL-based queries, a separate implementation would be used.
/// </summary>
public sealed class EfWorklistRepository(HnVueDbContext context) : IWorklistRepository
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<WorklistItem>>> QueryTodayAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTimeOffset.UtcNow.Date;
            var startTicks = new DateTimeOffset(today, TimeSpan.Zero).UtcTicks;
            var endTicks = new DateTimeOffset(today.AddDays(1), TimeSpan.Zero).UtcTicks;

            var studies = await context.Studies
                .AsNoTracking()
                .Where(s => s.StudyDateTicks >= startTicks && s.StudyDateTicks < endTicks)
                .Join(context.Patients.AsNoTracking(),
                    s => s.PatientId,
                    p => p.PatientId,
                    (s, p) => new { Study = s, Patient = p })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            IReadOnlyList<WorklistItem> items = studies.Select(x => new WorklistItem(
                AccessionNumber: x.Study.AccessionNumber ?? string.Empty,
                PatientId: x.Patient.PatientId,
                PatientName: x.Patient.Name,
                StudyDate: new DateTimeOffset(x.Study.StudyDateTicks,
                    TimeSpan.FromMinutes(x.Study.StudyDateOffsetMinutes)).Date is var d
                    ? DateOnly.FromDateTime(d) : (DateOnly?)null,
                BodyPart: x.Study.BodyPart,
                RequestedProcedure: x.Study.Description)).ToList();

            return Result.Success(items);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<IReadOnlyList<WorklistItem>>(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }
}
