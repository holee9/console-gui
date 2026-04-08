# Team B — Medical Imaging Pipeline Rules

## Module Ownership
- HnVue.Dicom, HnVue.Detector, HnVue.Imaging, HnVue.Dose, HnVue.Incident
- HnVue.Workflow, HnVue.PatientManagement, HnVue.CDBurning

## Safety-Critical Module Standards
- Dose and Incident modules: 90%+ Branch coverage (per DOC-012)
- All safety-critical changes require characterization tests before modification
- Dose interlock 4-level logic is invariant — changes require RA risk assessment

## DICOM Standards (per DICOM-001 Guide)
- Use fo-dicom 5.1.3 API conventions
- DICOM tag references use standard group,element notation
- C-STORE SCP/SCU must handle association negotiation properly
- MWL (Modality Worklist) queries follow IHE Radiology Technical Framework

## FPD Detector SDK Adapter Pattern
- All detector interactions through IDetectorService abstraction
- Simulator adapter for testing (no hardware dependency)
- SDK adapter wraps vendor-specific API
- Connection lifecycle: Initialize -> Connect -> Configure -> Acquire -> Disconnect

## Workflow State Machine Rules
- 9-state transition model is authoritative
- State transitions must be validated against allowed transitions table
- Invalid transitions throw InvalidOperationException
- State change events must be published for UI notification

## Interface Change Protocol
- IDetectorService or IWorkflowEngine changes: Coordinator approval required
- Workflow state transition changes: create RA issue for RTM (DOC-032) update
- Patient data model changes: coordinate with Team A (Data layer)

## Issue Protocol
- Safety-critical changes: create issue with `team-b` + `priority-high` labels
- Workflow state changes: create issue + notify RA team for RTM update
