# Console-GUI

HnVue - Medical Diagnostic X-Ray Console Software

## Overview

IEC 62304 Class B compliant console software for diagnostic X-ray imaging devices. Manages patient workflow, image acquisition/processing, DICOM communication, dose management, and cybersecurity.

## Documentation

All project documentation is organized under `docs/`:

| Category | Path | Description |
|----------|------|-------------|
| Planning | `docs/planning/` | MRD, PRD, FRS, SRS, SAD, SDS |
| Management | `docs/management/` | DMP, WBS, Development Guidelines, Cybersecurity Plan |
| Risk | `docs/risk/` | Risk Management Plan, FMEA, Threat Model |
| Testing | `docs/testing/` | Unit/Integration/System/Cyber Test Plans & Reports |
| Regulatory | `docs/regulatory/` | SBOM, DHF, 510(k), CE, KFDA, DICOM Conformance |
| Verification | `docs/verification/` | V&V Plans, RTM, Cross-Verification Reports |

### Key Documents

- **DOC-001** MRD v1.1 - Market Requirements (62 MR items, P1-P4 prioritized)
- **DOC-002** PRD v1.2 - Product Requirements (104 PR items + 18 User Stories)
- **DOC-004** FRS v1.0 - Functional Requirements Specification
- **DOC-005** SRS v1.0 - Software Requirements Specification
- **DOC-006** SAD v1.0 - Software Architecture Document
- **DOC-007** SDS v1.0 - Software Design Specification

### Priority Classification

Requirements use a 4-tier regulatory-risk-based priority system:

| Level | Name | Description |
|-------|------|-------------|
| P1 | Regulatory Mandatory | Required by law/certification (FDA, CE, KFDA) |
| P2 | Safety-Critical | Risk control measures per ISO 14971 |
| P3 | Clinically Important | Clinical workflow and diagnostic quality |
| P4 | Desirable | UX enhancements, deferrable to future releases |

## Regulatory Standards

- IEC 62304 (Medical Device Software Lifecycle)
- IEC 62366-1 (Usability Engineering)
- ISO 14971 (Risk Management)
- FDA 21 CFR 820.30 (Design Controls)
- ISO 13485 (Quality Management)
- DICOM 3.0 / IHE SWF
- HIPAA / GDPR
