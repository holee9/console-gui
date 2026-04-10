# Emergency Workflow Quick Reference

## TL;DR
Emergency workflow allows immediate X-ray exposure for trauma patients with minimal data, while still enforcing dose safety limits and access control.

## Usage

### Starting an Emergency Exposure

```csharp
// In your ViewModel or controller
var result = await _workflowEngine.StartEmergencyExposureAsync(
    patientName: "John Doe",  // Can be null for unknown patients
    parameters: new ExposureParameters(
        BodyPart: "CHEST",
        Kvp: 120.0,
        Mas: 10.0,
        StudyInstanceUid: "1.2.826.0.1.3680043.10.123.456.789.1234567890"
    )
);

if (result.IsFailure)
{
    // Handle dose interlock or authorization failure
    ShowError(result.ErrorMessage);
    return;
}

// Exposure is ready - generator is armed
var validation = result.Value;

if (validation.Level == DoseValidationLevel.Warn)
{
    // Show warning acknowledgment dialog
    ShowWarning(validation.Message);
}

// Proceed with exposure trigger
```

### Registering the Emergency Patient

```csharp
// After starting emergency workflow
var emergencyPatientId = $"EMERG-{DateTime.UtcNow:yyyyMMddHHmmss}";

var patient = await _patientService.QuickRegisterEmergencyAsync(
    emergencyPatientId: emergencyPatientId,
    patientName: "John Doe"  // Can be null
);

if (patient.IsSuccess)
{
    // Patient record created with IsEmergency=true
    // Full registration can be completed post-stabilization
}
```

## Emergency Workflow States

```
Normal Workflow:
Idle → PatientSelected → ProtocolLoaded → ReadyToExpose → Exposing

Emergency Workflow:
Idle → Exposing (bypasses intermediate states)
```

## Dose Interlock Behavior

| Level | Action | SafeState | Can Override? |
|-------|--------|-----------|---------------|
| Allow | Proceed | Idle | N/A |
| Warn | Proceed with warning | Warning | N/A |
| Block | BLOCK exposure | Blocked | NO |
| Emergency | BLOCK exposure | Emergency | NO |

**Key Point**: Even emergency exposures cannot override BLOCK or Emergency dose levels. This is a safety-critical constraint (IEC 60601-2-54).

## Emergency Patient ID Format

```
EMERG-{yyyyMMddHHmmss}

Examples:
- EMERG-20260106153045
- EMERG-20260106153046
```

## What Gets Bypassed

✅ **Bypassed in Emergency**:
- Full patient registration
- Duplicate patient detection
- Protocol selection
- Patient study creation
- Patient name validation (can be null)

❌ **NOT Bypassed (Still Enforced)**:
- RBAC authorization (must have PerformExposure permission)
- Dose interlock (Block/Emergency still block exposure)
- Audit trail logging (all emergencies logged)
- State machine validation (transitions still validated)

## Safety-Critical Paths

All emergency paths are marked with `@MX:NOTE` or `@MX:ANCHOR` tags:

```csharp
// @MX:ANCHOR StartEmergencyExposureAsync - Safety-critical emergency fast-path
// @MX:NOTE Emergency fast-path bypasses normal registration for trauma cases
// @MX:WARN Dose interlock still enforced - safety cannot be overridden
```

## Audit Trail Events

Emergency workflow logs the following audit events:

1. **EMERGENCY_EXPOSURE**
   - patientId: Auto-generated emergency ID
   - patientName: Provided name or null
   - studyUid: Auto-generated emergency study UID
   - level: Dose validation level (Allow/Warn)
   - kvp, mas: Exposure parameters

## Error Handling

```csharp
// Authentication required
ErrorCode.AuthenticationFailed
→ "User must be authenticated to perform emergency exposure."

// Authorization required
ErrorCode.InsufficientPermission
→ "User does not have PerformExposure permission."

// Dose interlock block
ErrorCode.DoseInterlock
→ "Dose BLOCKED: [reason]"
→ "EMERGENCY dose interlock: [reason]"

// Invalid state
ErrorCode.InvalidStateTransition
→ "Cannot start emergency exposure: system is in safe state 'Blocked'."
```

## Test Coverage

All emergency paths have 100% test coverage:

- **WorkflowEngineEmergencyTests.cs**: 9 tests
  - Authentication/authorization
  - Dose interlock levels (Allow/Warn/Block/Emergency)
  - Null patient names
  - Safe state transitions
  - Audit logging

- **PatientServiceEmergencyTests.cs**: 13 tests
  - Valid/invalid emergency IDs
  - Null/empty patient names
  - Duplicate detection bypass
  - Repository failure handling
  - Timestamp validation

## Integration Example

```csharp
public class EmergencyExposureViewModel
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IPatientService _patientService;

    public async Task<bool> StartEmergencyExposureAsync(
        string? patientName,
        ExposureParameters parameters)
    {
        // 1. Start emergency workflow (validates dose, transitions state)
        var result = await _workflowEngine.StartEmergencyExposureAsync(
            patientName,
            parameters);

        if (result.IsFailure)
        {
            ErrorMessage = result.ErrorMessage;
            return false;
        }

        // 2. Register emergency patient (persists record)
        var emergencyId = $"EMERG-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var patient = await _patientService.QuickRegisterEmergencyAsync(
            emergencyId,
            patientName);

        if (patient.IsFailure)
        {
            // Patient registration failed, but exposure is ready
            // Log warning but continue
            Logger.Warn($"Emergency patient registration failed: {patient.ErrorMessage}");
        }

        // 3. Check dose warning level
        if (result.Value.Level == DoseValidationLevel.Warn)
        {
            RequiresWarningAcknowledgment = true;
            WarningMessage = result.Value.Message;
        }

        // 4. Exposure is ready
        IsReadyToExpose = true;
        return true;
    }
}
```

## Post-Stabilization Workflow

After the patient is stabilized, complete full registration:

```csharp
// Retrieve emergency patient
var emergencyPatient = await _patientService.GetByIdAsync(emergencyPatientId);

// Update with full demographics
var fullRecord = new PatientRecord(
    PatientId: emergencyPatient.Value.PatientId,
    Name: "Smith^John^^Mr.",
    DateOfBirth: new DateOnly(1980, 5, 15),
    Sex: "M",
    IsEmergency: true,  // Keep emergency flag for audit trail
    CreatedAt: emergencyPatient.Value.CreatedAt,
    CreatedBy: emergencyPatient.Value.CreatedBy
);

await _patientService.UpdateAsync(fullRecord);
```

## Common Pitfalls

❌ **DON'T** forget RBAC - emergency still requires `PerformExposure` permission
❌ **DON'T** assume emergency overrides dose limits - Block/Emergency still block
❌ **DON'T** forget audit logging - all emergencies are logged for compliance
❌ **DON'T** use non-emergency IDs - must start with `EMERG-` prefix

✅ **DO** handle Warn dose level - show acknowledgment dialog
✅ **DO** log patient registration failures - don't block exposure
✅ **DO** keep IsEmergency flag - needed for audit trail
✅ **DO** complete full registration post-stabilization

## Regulatory References

- **IEC 62304 Class B**: Medical device software safety lifecycle
- **IEC 60601-2-54**: X-ray equipment dose interlock requirements
- **SWR-WF-026~027**: Emergency workflow requirements
- **SWR-PM-030~033**: Emergency patient registration requirements
- **SWR-IP-RBAC-001**: Role-based access control requirements
- **SWR-NF-SC-041**: Audit trail logging requirements

---

**Remember**: Emergency workflow = Fast path for trauma care, NOT a shortcut around safety.
