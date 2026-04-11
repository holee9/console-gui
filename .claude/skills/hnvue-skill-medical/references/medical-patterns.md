# Medical Pipeline Implementation Quality Guide

Team B agent reads this file when implementing code in Dicom, Detector, Imaging, Dose, Incident, Workflow, PatientManagement, CDBurning modules.

## Pre-Implementation Checklist

Before writing any code in medical modules:

1. Identify safety classification: Dose/Incident = SAFETY-CRITICAL (90%+ coverage mandatory)
2. Check if change touches workflow state transitions — requires RA notification
3. Verify IDetectorService/IWorkflowEngine interface compatibility — Coordinator approval for changes
4. For Dose module: dose interlock 4-level logic is INVARIANT — RA risk assessment required for changes

## Workflow State Machine Implementation

### Correct: Adding a new state transition

```csharp
// 1. Add to allowed transitions dictionary
private static readonly Dictionary<WorkflowState, IReadOnlySet<WorkflowState>> AllowedTransitions = new()
{
    [WorkflowState.Idle] = new HashSet<WorkflowState> { WorkflowState.PatientSelected, WorkflowState.Error },
    [WorkflowState.PatientSelected] = new HashSet<WorkflowState> { WorkflowState.ProtocolLoaded, WorkflowState.Idle, WorkflowState.Error },
    // ... existing transitions ...
};

// 2. Validate transition before executing
public Result TryTransition(WorkflowState targetState)
{
    lock (_lock)
    {
        if (!AllowedTransitions.TryGetValue(CurrentState, out var allowed) || !allowed.Contains(targetState))
            return Result.Failure(ErrorCode.InvalidStateTransition);
        
        var previousState = CurrentState;
        CurrentState = targetState;
        StateChanged?.Invoke(this, new StateChangedEventArgs(previousState, targetState));
        return Result.Success();
    }
}
```

### Anti-Patterns

- Allowing state transitions without validation against the allowed table
- Missing lock() on state read/write (race condition in WPF UI thread vs background)
- Forgetting to fire StateChanged event after transition
- Direct property set bypassing TryTransition()
- Removing ForceError() abort path (must work from ANY state)

## Dose Interlock Quality Gate (INVARIANT)

### DRL Reference Values (DO NOT MODIFY without RA approval)

| Body Part | DRL (mGy-cm2) |
|-----------|---------------|
| CHEST | 10 |
| ABDOMEN | 25 |
| PELVIS | 25 |
| SPINE | 40 |
| SKULL | 30 |
| Default | 20 |

### 4-Level Classification Logic (INVARIANT)

```csharp
public DoseValidationLevel ValidateDose(double dapValue, string bodyPart)
{
    var drl = GetDrl(bodyPart);
    
    return dapValue switch
    {
        <= 1.0 * drl => DoseValidationLevel.Allow,
        <= 2.0 * drl => DoseValidationLevel.Warn,
        <= 5.0 * drl => DoseValidationLevel.Block,
        _ => DoseValidationLevel.Emergency
    };
}
```

### Dose Module Anti-Patterns (SAFETY-CRITICAL)

- NEVER suppress a dose interlock result — log and escalate immediately
- NEVER change DRL values without RA risk assessment
- NEVER skip Emergency level handling (auto-generate incident report)
- NEVER use floating-point equality comparison for dose thresholds (use <= comparisons)
- NEVER return Allow when calculation fails — default to Block

### Dose Test Template

```csharp
[Theory]
[Trait("SWR", "SWR-DS-001")]
[InlineData(9.9, "CHEST", DoseValidationLevel.Allow)]   // Just under 1x DRL
[InlineData(10.0, "CHEST", DoseValidationLevel.Allow)]   // Exactly 1x DRL
[InlineData(10.1, "CHEST", DoseValidationLevel.Warn)]    // Just over 1x DRL
[InlineData(20.0, "CHEST", DoseValidationLevel.Warn)]    // Exactly 2x DRL
[InlineData(20.1, "CHEST", DoseValidationLevel.Block)]   // Just over 2x DRL
[InlineData(50.0, "CHEST", DoseValidationLevel.Block)]   // Exactly 5x DRL
[InlineData(50.1, "CHEST", DoseValidationLevel.Emergency)] // Just over 5x DRL
public void ValidateDose_BoundaryValues_ReturnsCorrectLevel(
    double dap, string bodyPart, DoseValidationLevel expected)
{
    var sut = CreateDoseService();
    var result = sut.ValidateDose(dap, bodyPart);
    result.Should().Be(expected);
}
```

## DICOM Implementation Patterns

### Correct: C-STORE SCU with error handling

```csharp
public async Task<Result> SendImageAsync(DicomFile dicomFile, CancellationToken ct)
{
    var client = DicomClientFactory.Create(
        _config.PacsHost, _config.PacsPort, false, _config.LocalAeTitle, _config.PacsAeTitle);
    
    var request = new DicomCStoreRequest(dicomFile);
    DicomStatus responseStatus = null!;
    
    request.OnResponseReceived += (req, resp) => responseStatus = resp.Status;
    
    await client.AddRequestAsync(request);
    await client.SendAsync(ct);
    
    return responseStatus == DicomStatus.Success
        ? Result.Success()
        : Result.Failure(ErrorCode.DicomStoreFailed);
}
```

### Anti-Patterns

- Ignoring OnResponseReceived callback — always check response status
- Missing CancellationToken propagation to DICOM client
- Hardcoding AE titles or ports (use IDicomNetworkConfig)
- Not handling association rejection gracefully
- Using synchronous DICOM operations (deadlock risk in WPF)

## Detector Adapter Pattern

### Adding a New Detector Adapter

1. Implement IDetectorInterface
2. Register in DI with named/keyed service
3. Add DetectorSimulator test coverage for the new adapter's behavior
4. State lifecycle must match: Disconnected -> Idle -> Armed -> Acquiring -> ImageReady

### Anti-Patterns

- Direct hardware calls outside IDetectorInterface abstraction
- Missing simulator adapter for test environments
- Not handling connection loss gracefully (retry with backoff)
- Blocking UI thread during detector operations

## Incident Module Quality Gate

### Correct: Reporting an incident

```csharp
public async Task<Result<Guid>> ReportAsync(IncidentRecord record, CancellationToken ct)
{
    ArgumentNullException.ThrowIfNull(record);
    
    var id = Guid.NewGuid();
    var entry = new IncidentEntry(id, record, DateTimeOffset.UtcNow);
    
    _incidents.TryAdd(id, entry);
    
    // MANDATORY: audit trail for every incident
    await _auditService.LogAsync(new AuditEntry
    {
        Action = record.Severity == Severity.Critical ? "CRITICAL_INCIDENT" : "INCIDENT_REPORTED",
        Details = $"Incident {id}: {record.Category} - {record.Description}",
        Timestamp = DateTimeOffset.UtcNow
    }, ct);
    
    return Result<Guid>.Success(id);
}
```

### Anti-Patterns

- Missing audit entry on incident creation
- Not tagging Critical incidents with CRITICAL_INCIDENT
- Swallowing exceptions in incident reporting (must always succeed or escalate)
- Using non-thread-safe collections (use ConcurrentDictionary)

## Testing Quality Requirements

### Coverage Targets by Module

| Module | Target | Reason |
|--------|--------|--------|
| Dose | 90%+ | Safety-critical: dose interlock |
| Incident | 90%+ | Safety-critical: incident reporting |
| Workflow | 85%+ | State machine completeness |
| Dicom | 85% | DICOM compliance |
| Detector | 85% | Hardware abstraction |
| Imaging | 85% | Image quality |
| PatientManagement | 85% | Data integrity |
| CDBurning | 85% | Standard |

### Mandatory Test Categories

For safety-critical modules (Dose, Incident):
- Boundary value tests (exact DRL multiples)
- Error path tests (every Result.Failure case)
- Concurrency tests (thread safety verification)
- Characterization tests before ANY modification

### SWR Trait Mapping

Every test in safety-critical modules MUST include [Trait("SWR", "SWR-xxx")]:
- DS: Dose (SWR-DS-xxx)
- IN: Incident (SWR-IN-xxx)
- WF: Workflow (SWR-WF-xxx)
- DC: DICOM (SWR-DC-xxx)
- DT: Detector (SWR-DT-xxx)

## Post-Implementation Verification Script

```bash
# 1. Build owned modules
dotnet build src/HnVue.Dicom/ src/HnVue.Detector/ src/HnVue.Imaging/ src/HnVue.Dose/ src/HnVue.Incident/ src/HnVue.Workflow/ src/HnVue.PatientManagement/ src/HnVue.CDBurning/

# 2. Run owned tests
dotnet test tests/HnVue.Dicom.Tests/ tests/HnVue.Detector.Tests/ tests/HnVue.Imaging.Tests/ tests/HnVue.Dose.Tests/ tests/HnVue.Incident.Tests/ tests/HnVue.Workflow.Tests/ tests/HnVue.PatientManagement.Tests/ tests/HnVue.CDBurning.Tests/ --verbosity normal

# 3. Full solution build
dotnet build HnVue.sln -c Release

# 4. Architecture tests
dotnet test tests/HnVue.Architecture.Tests/ --verbosity normal
```

## Cross-Module Change Protocol

| Change Type | Notify | Issue Label | Timing |
|-------------|--------|-------------|--------|
| IDetectorService change | Coordinator | interface-contract | Before implementation |
| IWorkflowEngine change | Coordinator | interface-contract | Before implementation |
| Workflow state transition add/remove | RA team | ra-update | After implementation |
| Patient data model change | Team A (Data) | team-b | Before implementation |
| Safety-critical module change | RA team | team-b + priority-high | Before implementation |
| Dose DRL value change | RA team | FORBIDDEN without risk assessment | Never without approval |
