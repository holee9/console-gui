---
name: hnvue-skill-medical
description: >
  HnVue Team B medical imaging pipeline engineering skill. Encodes fo-dicom 5.x DICOM networking,
  FPD detector adapter pattern, 4-level dose interlock (IEC 60601-2-54), 9-state workflow FSM,
  patient management, and CD burning. Safety-critical module expertise for Dose/Incident (Class B).
  Loaded by hnvue-medical agent. Triggers on: DICOM, detector, dose, workflow, patient, imaging, incident.
user-invocable: false
metadata:
  version: "1.0.0"
  category: "domain"
  status: "active"
  updated: "2026-04-11"
  tags: "hnvue, medical, dicom, detector, dose, workflow, patient, safety-critical"

# MoAI Extension: Progressive Disclosure
progressive_disclosure:
  enabled: true
  level1_tokens: 100
  level2_tokens: 4500

# MoAI Extension: Triggers
triggers:
  keywords: ["dicom", "detector", "dose", "workflow", "patient", "imaging", "incident", "state machine", "interlock", "fo-dicom", "C-STORE", "MWL", "FPD", "acquisition"]
  agents: ["hnvue-medical"]
---

# HnVue Medical Imaging Pipeline Skill

Senior-level domain knowledge for HnVue medical pipeline modules (Dicom, Detector, Imaging, Dose, Incident, Workflow, PatientManagement, CDBurning).

## 1. DICOM Networking (fo-dicom 5.x)

**DicomService (sealed partial):**
- C-STORE SCU: Send images to PACS
- C-FIND: Modality Worklist queries with pending response handling
- DICOM Print: N-CREATE/N-ACTION for film printing
- Storage Commitment: N-ACTION for archival confirmation
- MPPS: N-CREATE (SWR-DC-055) / N-SET (SWR-DC-056) for procedure status

**DicomOutbox:** Polly exponential backoff retry (3 retries: 2s/4s/8s)
**DicomFileIO:** Static API for file I/O and tag queries
**IDicomNetworkConfig:** Per-service configuration (PACS/MWL/Printer/MPPS AE titles, hosts, ports)

**Rules:**
- Always use async client with OnResponseReceived callbacks
- Handle pending responses in C-FIND worklist queries
- TLS is optional but configurable
- Standard group,element tag notation per IHE Radiology Technical Framework

## 2. FPD Detector Adapter Pattern

**IDetectorInterface abstraction:**
- ConnectAsync(), ArmAsync(DetectorTriggerMode), AbortAsync(), GetStatusAsync()
- Events: StateChanged, ImageAcquired (raw pixel data)

**State lifecycle:** Disconnected -> Idle -> Armed -> Acquiring -> ImageReady

**Adapters:**
- DetectorSimulator: configurable arm/readout delays for testing
- OwnDetectorAdapter: production SDK wrapper (src/HnVue.Detector/OwnDetector/)
- Third-party adapters: vendor SDKs (src/HnVue.Detector/ThirdParty/)

**Safety requirements:**
- SWR-WF-030: detector arming
- SWR-WF-031: image acquisition
- SWR-WF-032: abort command
- HAZ-RAD: radiation safety interlock integration

## 3. Dose Interlock (SAFETY-CRITICAL - IEC 60601-2-54)

**4-Level Validation (INVARIANT - changes require RA risk assessment):**

| Level | Condition | Action |
|-------|-----------|--------|
| Allow | DAP <= 1x DRL | Proceed normally |
| Warn | DAP <= 2x DRL | Show warning, operator can override |
| Block | DAP <= 5x DRL | Block exposure, supervisor override required |
| Emergency | DAP > 5x DRL | Hard block, incident report auto-generated |

**DRL lookup by body part (mGy-cm2):**
- CHEST=10, ABDOMEN=25, PELVIS=25, SPINE=40, SKULL=30, default=20

**Calculation formulas:**
- DAP estimation: `(kVp^2 x mAs) / 500,000` (simplified linear per IEC 60601-2-54)
- ESD: `(DAP / FieldArea) x 1.35` (backscatter factor per IAEA TECDOC-1423)
- Exposure Index: `(MeanPixelValue / TargetPixelValue) x 1000` (per IEC 62494-1)

**RDSR:** GenerateRdsrSummaryAsync() computes enriched dose records
**History:** GetDoseHistoryAsync(patientId, from, until) for regulatory tracking

**Testing: 90%+ branch coverage mandatory. Never suppress dose interlock violations.**

## 4. Workflow 9-State FSM

**States:** Idle -> PatientSelected -> ProtocolLoaded -> ReadyToExpose -> Exposing -> ImageAcquiring -> ImageProcessing -> ImageReview -> Completed; Error (abort path)

**WorkflowStateMachine:**
- Allowed transitions: `Dictionary<WorkflowState, IReadOnlySet<WorkflowState>>`
- TryTransition(targetState): returns `Result<ErrorCode.InvalidStateTransition>`
- ForceError(): emergency abort (always succeeds from any state)
- ForceExposing(): trauma fast-path (SWR-WF-026~027)

**WorkflowEngine orchestration:**
- PrepareExposureAsync(): dose validation (4-level interlock) -> arm detector
- TransitionAsync(): RBAC enforcement (Radiographer/Radiologist only for Exposing)
- AbortAsync(reason): calls generator + detector abort, escalates to Emergency if needed

**SafeState enum:** Idle/Warning/Degraded/Blocked/Emergency (synced with DoseValidationLevel)

**Rules:**
- Invalid transitions throw InvalidOperationException
- State change events published for UI notification
- Lock-based synchronization (_lock object) for thread safety
- Abort path always available from any state

## 5. Incident Reporting (SAFETY-CRITICAL)

**IncidentService:** Thread-safe via ConcurrentDictionary
- ReportAsync(): creates IncidentRecord with severity (Critical/High/Medium/Low)
- Critical incidents get `CRITICAL_INCIDENT` audit tag
- Every mutation produces IAuditService entry (tamper-evident)
- Event sourcing: GUID + timestamp + resolved flag

## 6. Patient Management

**PatientService:** Registration, search, update with validation
- Duplicate detection on registration
- DICOM worklist integration for patient lookup
- Full CRUD with audit trail
- Patient data integrity affects dose attribution (IEC 62304 Class B)

## 7. CD Burning

**CDDVDBurnService:** IMAPI2 COM interop (STA thread required)
- Disc readiness check (inserted/blank)
- ISO 9660 volume label limit (32 chars)
- BurnStudyAsync(studyInstanceUid, outputLabel) with file validation

## 8. Error Handling

**ErrorCode ranges for medical domain:**
- Workflow: 4xxx (InvalidStateTransition=4022, DoseInterlock=4005, DetectorNotReady=4012)
- DICOM: 5xxx (DicomStoreFailed=4008)

**Patterns:**
- Detector: retry with exponential backoff, fall to simulator
- DICOM: log DIMSE status, report negotiation details
- Dose interlock: NEVER suppress, log and escalate immediately
- Structured logging: ILogger<T> + LoggerMessage attributes (CA1848)

## 9. Cross-Module Protocol

- IDetectorService/IWorkflowEngine interface changes -> Coordinator approval
- Workflow state transition changes -> RA issue for RTM (DOC-032) update
- Patient data model changes -> coordinate with Team A (Data layer)
- Safety-critical changes -> issue with priority-high label

## 10. Quality Enforcement Protocol [HARD]

Before writing any code, read `${CLAUDE_SKILL_DIR}/references/medical-patterns.md` for:
- Pre-implementation checklist (safety classification FIRST)
- Dose interlock boundary value test template (MANDATORY for Dose changes)
- State machine anti-patterns (common bugs from past sprints)
- Post-implementation verification script (MUST run before COMPLETED)

**Implementation flow:**
1. Read references/medical-patterns.md Pre-Implementation Checklist
2. Identify safety classification (Dose/Incident = 90%+ coverage)
3. Write code following correct patterns (anti-patterns are explicitly listed)
4. For Dose changes: add boundary value tests using the provided template
5. Run Post-Implementation Verification Script (all 4 steps)
6. Only report COMPLETED with build evidence
