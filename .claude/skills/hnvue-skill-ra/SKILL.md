---
name: hnvue-skill-ra
description: >
  HnVue RA regulatory affairs engineering skill. Encodes IEC 62304 Class B document lifecycle,
  CycloneDX 1.5 SBOM management, requirements traceability matrix (RTM) with SWR->TC mapping,
  FMEA risk management, FDA 510(k)/CE/KFDA submission packaging, and implementation-to-document mapping.
  30+ regulatory documents tracked. Loaded by hnvue-ra agent.
  Triggers on: IEC 62304, FDA, SBOM, RTM, SOUP, FMEA, regulatory, compliance, DOC-, submission.
user-invocable: false
metadata:
  version: "1.0.0"
  category: "domain"
  status: "active"
  updated: "2026-04-11"
  tags: "hnvue, ra, regulatory, iec-62304, sbom, rtm, fda, fmea"

# MoAI Extension: Progressive Disclosure
progressive_disclosure:
  enabled: true
  level1_tokens: 100
  level2_tokens: 4500

# MoAI Extension: Triggers
triggers:
  keywords: ["iec 62304", "fda", "sbom", "rtm", "soup", "fmea", "regulatory", "compliance", "submission", "510k", "ce mark", "kfda", "risk management"]
  agents: ["hnvue-ra"]
---

# HnVue Regulatory Affairs Skill

Senior-level domain knowledge for HnVue IEC 62304 regulatory document management.

## 1. Document Inventory (30+ Documents)

**Planning (docs/planning/):**
- DOC-004: FRS (Functional Requirements Specification)
- DOC-005: SRS (Software Requirements Specification)
- DOC-006: SAD (Software Architecture Description)
- DOC-007: SDS (Software Detailed Design Specification)

**Risk (docs/risk/):**
- DOC-008: RMP (Risk Management Plan) — v2.0 planned 2026-05
- DOC-009: FMEA (Failure Mode and Effects Analysis)
- DOC-010: RMR (Risk Management Report)
- DOC-017: Threat Model

**Verification (docs/verification/):**
- DOC-011: V&V Master Plan
- DOC-015: Validation Plan
- DOC-025: V&V Summary
- DOC-032: RTM (Requirements Traceability Matrix) v2.0
- DOC-033: SOUP Report

**Regulatory (docs/regulatory/):**
- DOC-019: SBOM (CycloneDX 1.5, 42 components)
- DOC-020: Clinical Evaluation Plan
- DOC-035: DHF (Design History File)
- DOC-036: 510(k) eSTAR (FDA)
- DOC-037: CE Technical Documentation
- DOC-038: DICOM Conformance Statement
- DOC-039: KFDA Submission
- DOC-040: IFU (Instructions for Use)
- DOC-045: VEX Report
- DOC-046: Security Controls
- DOC-049: IEC 81001 Compliance
- DOC-050: Predicate Comparison
- DOC-051: PMS/PMCF
- DOC-052: GSPR Checklist

**Management (docs/management/):**
- DOC-042: CMP (Configuration Management Plan) — PRIORITY: complete from Draft
- DOC-044: Known Anomalies
- DOC-034: Release Checklist
- DOC-048: VMP (Validation Master Plan)
- DMP-001: Development Management Plan
- DOC-003: SW Development Guideline
- DOC-016: Cybersecurity Plan

## 2. Version Policy

- Major (v1.0 -> v2.0): Significant change requiring regulatory re-review
- Minor (v1.0 -> v1.1): Corrections, clarifications, non-substantive
- Date-based tagging: DOC-XXX_vY.Z (version in filename)

## 3. RTM Traceability (DOC-032)

**Requirements hierarchy:**
MR-xxx (Market Req) -> PR-DOM-xxx (Product Req) -> SWR-DOM-xxx (Software Req) -> TC-DOM-xxx (Test Case)

**Code-to-requirement tracing:**
- xUnit `[Trait("SWR", "SWR-xxx")]` annotations on test methods
- 100% SWR -> TC mapping required

**Domain prefixes:**
- CS: Common/Security | DC: DICOM | DT: Detector | DS: Dose
- WF: Workflow | PM: PatientManagement | IM: Imaging
- IN: Incident | UP: Update | SA: SystemAdmin | CB: CDBurning

**Traceability gaps -> issue with `ra-update` + `priority-high`**

## 4. SBOM Management (DOC-019)

**Format:** CycloneDX 1.5 JSON
**Components:** 42 tracked (as of v1.0)
**Script:** `scripts/ra/Generate-SBOM.ps1`

**5 Update Triggers:**
1. New NuGet component added
2. CVE found (CVSS >= 7.0)
3. Component removed
4. Quarterly review
5. Release build

**OWASP results from QA feed into SBOM update pipeline.**

## 5. Implementation-to-Document Mapping

| Code Change | Document Updates |
|-------------|-----------------|
| NuGet add/remove | DOC-019 SBOM + DOC-033 SOUP |
| Interface contract change | DOC-005 SRS + DOC-032 RTM |
| Workflow state add/remove | DOC-004 FRS + DOC-032 RTM |
| Security feature change | DOC-049 IEC81001 + DOC-045 VEX |
| P1 bug fix | DOC-044 Known Anomalies + FMEA review |
| Architecture change | DOC-006 SAD + DOC-007 SDS |
| New test requirement | DOC-032 RTM (add TC mapping) |
| Safety-critical module change | DOC-009 FMEA + DOC-008 RMP review |

## 6. SOUP Report (DOC-033)

**Tracks all third-party components:**
- NuGet packages with version, license, CVSS status
- fo-dicom 5.2.5, EF Core 9.0, CommunityToolkit.Mvvm 8.2.2, etc.
- New package addition requires: security review + SOUP list update + RA notification

## 7. Risk Management

**FMEA (DOC-009):**
- Hazard categories: HAZ-RAD (radiation), HAZ-DOSE (dose), HAZ-DATA (patient data), HAZ-SW (software)
- Severity x Probability = Risk Priority Number (RPN)
- High-risk items require design mitigation + verification

**4-Tier Priority System (planned for RMP v2.0):**
- Integrating with MR-072 market requirement

## 8. Submission Packaging

**FDA 510(k):** DOC-036 eSTAR format
**CE Mark:** DOC-037 Technical Documentation
**KFDA:** DOC-039 Korean FDA submission
**Script:** `scripts/ra/Package-Submission.ps1`

## 9. Priority Tasks

1. Complete DOC-042 CMP (currently Draft status)
2. RMP v2.0 update (planned 2026-05): integrate 4-Tier priority + MR-072
3. FDA 510(k) submission package preparation (DOC-036 eSTAR)

## 10. Issue Protocol

- Document update needed: issue with `ra-update` label
- RTM traceability gap: issue with `ra-update` + `priority-high`
- SBOM update: triggered by QA OWASP results via `soup-update` label
- Safety-critical code change: requires FMEA review notification

## 11. Quality Enforcement Protocol [HARD]

Before modifying any document, read `${CLAUDE_SKILL_DIR}/references/ra-patterns.md` for:
- Document update quality gate (version policy decision tree)
- RTM traceability verification procedure (gap detection)
- SBOM update trigger verification checklist
- Implementation-to-document mapping (detailed chain for each change type)

**Document update flow:**
1. Read references/ra-patterns.md Pre-Update Checklist
2. Apply version policy (Major/Minor decision tree)
3. Update document with revision history entry
4. Verify RTM traceability (no orphaned SWR entries)
5. Run SBOM generation if SBOM affected
6. Only report COMPLETED with validation evidence
