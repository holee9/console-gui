# RA Team — Regulatory Affairs Rules

Shared rules: see `team-common.md` (Philosophy, Self-Verification, Git Protocol)

## Ownership
- docs/regulatory/ (16 regulatory documents: FDA, CE, KFDA)
- docs/planning/ (SRS DOC-005, SAD DOC-006, SDS DOC-007, FRS DOC-004)
- docs/risk/ (FMEA DOC-009, RMP DOC-008, RMR DOC-010, Threat Model DOC-017)
- docs/verification/ (RTM DOC-032, SOUP DOC-033, V&V Master Plan DOC-011)
- docs/management/ (DOC-042 CMP — PRIORITY: complete from Draft)
- docs/development/ (development process documentation)
- docs/research/ (research notes and analysis)
- docs/archive/ (archived documentation)
- docs/docfx/ (DocFX build configuration)
- docs/deployment/ (co-owned with QA)
- docs/architecture/ (co-owned with Coordinator)
- scripts/ra/, docfx.json, CHANGELOG.md

## IEC 62304 Document Version Policy
- Major version (v1.0 -> v2.0): Significant change requiring regulatory re-review
- Minor version (v1.0 -> v1.1): Corrections, clarifications, non-substantive updates
- Date-based tagging: DOC-XXX_vY.Z (version in filename)

## RTM Tracking Standards
- Requirements hierarchy: MR-xxx -> PR-DOM-xxx -> SWR-DOM-xxx -> TC-DOM-xxx
- xUnit [Trait("SWR", "SWR-xxx")] annotations used for code-to-requirement tracing
- 100% SWR -> TC mapping required (per DOC-032 v2.0)
- Traceability gaps: create issue with `ra-update` + `priority-high` labels

## SBOM Management (DOC-019)
- Format: CycloneDX 1.5 JSON
- 42 components tracked (as of v1.0)
- 5 update triggers: new component, CVE found (CVSS>=7), component removed, quarterly review, release build
- Auto-generation: scripts/ra/Generate-SBOM.ps1

## Implementation Change to Document Update Mapping
- NuGet package add/remove -> DOC-019 SBOM + DOC-033 SOUP
- Interface contract change -> DOC-005 SRS + DOC-032 RTM
- Workflow state add/remove -> DOC-004 FRS + DOC-032 RTM
- Security feature change -> DOC-049 IEC81001 + DOC-045 VEX
- P1 bug fix -> DOC-044 Known Anomalies + FMEA review
- Architecture change -> DOC-006 SAD + DOC-007 SDS

## Priority Tasks
1. Complete DOC-042 CMP (Configuration Management Plan) — currently Draft
2. RMP v2.0 update (planned 2026-05) — integrate 4-Tier priority system + MR-072
3. FDA 510(k) submission package preparation (DOC-036 eSTAR)

## Issue Protocol
- Document update needed: create issue with `ra-update` label
- RTM traceability gap: create issue with `ra-update` + `priority-high` labels
- SBOM update: triggered by QA OWASP results via `soup-update` label
