using HnVue.Common.Models;
using HnVue.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Repositories;

/// <summary>
/// EF Core repository for today's scheduled worklist items in the HnVue.Data layer.
/// Queries <see cref="HnVueDbContext.Studies"/> joined with <see cref="HnVueDbContext.Patients"/>
/// to produce <see cref="WorklistItem"/> results for today's date.
/// For DI registration, use <c>HnVue.PatientManagement.EfWorklistRepository</c> which implements <c>IWorklistRepository</c>.
/// REQ-COORD-002: SPEC-COORDINATOR-001 EF Core worklist query.
/// </summary>
public sealed class EfWorklistRepository(HnVueDbContext context)
{
    /// <summary>Returns all scheduled studies for today as worklist items.</summary>
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
