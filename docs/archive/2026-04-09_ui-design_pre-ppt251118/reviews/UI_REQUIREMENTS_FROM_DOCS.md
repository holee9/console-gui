# UI Requirements Extracted from Planning Documents

## Executive Summary

Analysis of 10 planning documents (FRS, SRS, PRD v3.1, IEC 62366-1 Usability Engineering File, screen mockups) for HnVue X-Ray Console GUI reveals significant gaps between regulatory requirements and current screen designs.

**Key Metrics:**
- Screen Spec Coverage: Average 48% vs. requirements
- Critical Gaps Found: 6 items (blocking release)
- High Priority Gaps: 6 items (blocking Phase 2)
- Total Recommendations: 16 design actions

## Coverage by Screen

| Screen | Coverage | Gap Severity |
|--------|----------|-------------|
| Patient Management | 80% | MEDIUM |
| Acquisition Workflow | 60% | HIGH |
| Image Display/Viewer | 40% | HIGH |
| Dose Management | 0% | CRITICAL |
| DICOM/Network UI | 50% | MEDIUM |
| System Admin | 40% | HIGH |
| Cybersecurity/Login | 50% | MEDIUM |
| IEC 62366 Compliance | 30% | CRITICAL |

## Priority 1 — Blocking Release (Must Fix)

### P1-001: Dose Management Dashboard [CRITICAL]
**Source**: FR-DM-001, FR-DM-015, HRUS-006, HAZ-RAD
**Current state**: Zero dose UI in any mockup
**Required**:
- Real-time DAP display during exposure
- DRL progress gauge (0-100%), color coded:
  - <70%: #00C853 (safe)
  - 70-89%: #FFD600 (warning banner)
  - 90-99%: #FF6D00 (modal confirmation required)
  - >=100%: #D50000 (physician PIN required)
- EI/DI display with ±2/±3 thresholds
- Cumulative dose history link
**Impact**: Cannot meet IEC 62366-1 §5.7 usability goals (UG-002: safety-critical task 100% success)

### P1-002: Emergency Stop Button Definition [CRITICAL]
**Source**: FR-WF-023, PR-UX-026, HAZ-RAD, HRUS-001
**Current state**: Referenced in acquisition.md but NO position, size, or always-visible specification
**Required**: Fixed position in ALL screens (header/top-right), #D50000, 56px, Escape key
**Impact**: Regulatory requirement. Must be defined before any usability validation.

### P1-003: Patient Confirmation Modal [CRITICAL]
**Source**: HRUS-001 (patient misidentification hazard)
**Current state**: No confirmation modal design in any screen spec
**Required**: Photo + Name + DOB + Age mandatory confirmation before first exposure
**Impact**: Patient safety. HRUS-001 cannot be mitigated without this modal.

### P1-004: 5-Click Workflow Optimization [HIGH]
**Source**: MR-003, UEF §4.3, UG-001 (>=90% success rate)
**Current state**: Workflow not mapped across screens; no click count optimization
**Required**: Patient(1) -> Protocol(2) -> Confirm(3) -> Expose(4) -> Auto-transfer(5)
**Impact**: Cannot demonstrate UG-001 in summative usability testing.

### P1-005: CD Burning Screen Design [HIGH]
**Source**: MR-072, HRUS-009, UG-009
**Current state**: Zero UI design for CD/export burning feature
**Required**: 4-step wizard with PHI confirmation
**Impact**: Required feature (MR-072) completely undesigned.

### P1-006: Interlock Validation Feedback [HIGH]
**Source**: SWR-WF-045, FR-WF-025
**Current state**: No modal template for interlock failures
**Required**: Checklist showing each interlock state
**Impact**: RT cannot diagnose why Expose button is disabled.

## Priority 2 — High (Phase 2 Blockers)

### P2-001: Pediatric Warning System
**Source**: HRUS-002 (pediatric over-exposure)
**Required**: Auto-detect age < 18, warn if adult protocol selected, recommend pediatric alternative

### P2-002: Trauma/Emergency Mode Workflow
**Source**: FR-WF-035, PR-WF-016, MR-003
**Required**: One-button trauma mode, auto temp ID, 3-click to expose

### P2-003: Image Viewer Screen Spec
**Source**: FR-IP-001–047, PR-IP-020–037
**Required**: Window/Level, measurements, annotations, layout selector

### P2-004: Session Timeout Warning
**Source**: FR-CS-010, PR-CS-072
**Required**: 3-minute countdown modal, extend/logout options

### P2-005: Calibration Wizard UI
**Source**: FR-SA-010–015, PR-SA-063
**Required**: Multi-step wizard for Offset/Gain/Defect calibration

### P2-006: Language Switcher with Preview
**Source**: MR-045, HRUS-010, UG-010
**Required**: Preview before confirming language switch

## Priority 3 — Medium (Phase 3)

- TLS connection indicator in status bar (FR-DC-030)
- Audit trail viewer in settings (FR-SA-025)
- Defect pixel map visualization (FR-SA-020)
- Image stitching workflow (FR-IP-010–012)

## Usability Goal Compliance

| Goal | Target | Blocker if Missing |
|------|--------|-------------------|
| UG-001: 5-Click Workflow >=90% | Click optimization | P1-004 |
| UG-002: Safety Tasks 100% | Emergency Stop, Dose, Patient ID | P1-001, P1-002, P1-003 |
| UG-003: Zero catastrophic errors | Patient ID confirm, DRL warning | P1-001, P1-003 |
| UG-005: SUS >=78 | Overall usability | Multiple |
| UG-009: CD Burn >=95% | CD Burning screen | P1-005 |
| UG-010: Language Switch >=90% | Language preview | P2-006 |

## Token Consistency Issues Found

ALL screen specs and component_library.md use OLD tokens. Required mapping:
- #0066CC -> #1B4F8A (primary)
- #252542 -> #16213E (surface)
- #3E3E5E -> #2E4A6E (border)
- #E0E0E0 -> #FFFFFF (text)
- #A0A0B0 -> #B0BEC5 (text muted)
- #FF4757 -> #D50000 (error/emergency)
- #FFA502 -> #FFD600 (warning)
- #2ED573 -> #00C853 (success)
- #1E90FF -> #00AEEF (info/accent)

---
**Analysis Date**: 2026-04-06
**Version**: 1.0
**Regulatory Basis**: IEC 62366-1:2015+AMD1:2020, FDA HFE Guidance, IEC 60601-1-6
