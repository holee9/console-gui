using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.PatientManagement;

/// <summary>
/// Implements DICOM Modality Worklist (MWL) polling and patient import.
/// </summary>
/// <remarks>
/// Polls the MWL SCP on a 10-second interval when background polling is active.
/// Supports emergency patient registration with auto-generated IDs.
/// IEC 62304 Class B — worklist data accuracy is safety-relevant for correct patient identification.
/// </remarks>
public sealed class WorklistService : IWorklistService
{
    private const string EmergencyIdPrefix = "EMRG-";
    private const string EmergencyPatientName = "Emergency^Patient";

    private readonly IWorklistRepository _worklistRepository;
    private readonly IPatientService _patientService;

    /// <summary>
    /// Initialises a new <see cref="WorklistService"/>.
    /// </summary>
    /// <param name="worklistRepository">Transport layer for MWL C-FIND queries.</param>
    /// <param name="patientService">Patient registration service for import.</param>
    public WorklistService(IWorklistRepository worklistRepository, IPatientService patientService)
    {
        _worklistRepository = worklistRepository
            ?? throw new ArgumentNullException(nameof(worklistRepository));
        _patientService = patientService
            ?? throw new ArgumentNullException(nameof(patientService));
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<WorklistItem>>> PollAsync(
        CancellationToken cancellationToken = default)
    {
        return await _worklistRepository.QueryTodayAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<PatientRecord>> ImportFromMwlAsync(
        WorklistItem item,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        // Check if patient already exists locally
        var existing = await _patientService.GetByIdAsync(item.PatientId, cancellationToken)
            .ConfigureAwait(false);

        if (existing.IsSuccess && existing.Value is not null)
            return Result.Success(existing.Value);

        // Register patient from worklist data
        var patient = new PatientRecord(
            PatientId: item.PatientId,
            Name: item.PatientName,
            DateOfBirth: null,
            Sex: null,
            IsEmergency: false,
            CreatedAt: DateTimeOffset.UtcNow,
            CreatedBy: "WORKLIST");

        return await _patientService.RegisterAsync(patient, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates and registers an emergency patient with an auto-generated ID.
    /// Emergency patients bypass normal worklist workflow.
    /// </summary>
    /// <param name="operatorId">User ID of the operator creating the emergency entry.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>The created emergency <see cref="PatientRecord"/>.</returns>
    public async Task<Result<PatientRecord>> CreateEmergencyPatientAsync(
        string operatorId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operatorId);

        var emergencyId = $"{EmergencyIdPrefix}{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

        var patient = new PatientRecord(
            PatientId: emergencyId,
            Name: EmergencyPatientName,
            DateOfBirth: null,
            Sex: null,
            IsEmergency: true,
            CreatedAt: DateTimeOffset.UtcNow,
            CreatedBy: operatorId);

        return await _patientService.RegisterAsync(patient, cancellationToken).ConfigureAwait(false);
    }
}
