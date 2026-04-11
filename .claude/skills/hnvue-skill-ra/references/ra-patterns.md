# Regulatory Affairs Implementation Quality Guide

RA agent reads this file when updating regulatory documents, SBOM, RTM, or preparing submissions.

## Pre-Update Checklist

Before modifying any regulatory document:

1. Identify document version — apply IEC 62304 version policy (Major/Minor)
2. Check Implementation-to-Document mapping — which documents need updating
3. Verify RTM traceability — will this change create gaps?
4. For SBOM changes: confirm update trigger (new component, CVE, removal, quarterly, release)

## Document Update Quality Gate

### Version Policy Decision Tree

```
Is this a substantive change requiring regulatory re-review?
  YES -> Major version bump (v1.0 -> v2.0)
         - New requirements added
         - Risk classification changed
         - Architecture significantly modified
  NO  -> Minor version bump (v1.0 -> v1.1)
         - Corrections, clarifications
         - Non-substantive updates
         - Formatting fixes
```

### Document Header Template

Every document update must maintain this header structure:

```markdown
# DOC-XXX: Document Title
**Version:** vX.Y
**Status:** Draft | Review | Approved
**Author:** [name]
**Reviewer:** [name]
**Approver:** [name]
**Last Updated:** YYYY-MM-DD
**Classification:** IEC 62304 Class B

## Revision History
| Version | Date | Author | Changes |
|---------|------|--------|---------|
| vX.Y | YYYY-MM-DD | [name] | [description] |
```

### Anti-Patterns

- Missing revision history entry for changes
- Changing content without version bump
- Draft status on documents that should be Approved
- Missing Author/Reviewer/Approver fields
- Backdating revision entries

## RTM Traceability Quality Gate

### Requirements Hierarchy Verification

For every code change, verify the chain exists:

```
MR-xxx (Market Requirement)
  -> PR-DOM-xxx (Product Requirement)
    -> SWR-DOM-xxx (Software Requirement)
      -> TC-DOM-xxx (Test Case)
        -> [Trait("SWR", "SWR-DOM-xxx")] (xUnit annotation)
```

### Domain Prefix Reference

| Prefix | Domain | Modules |
|--------|--------|---------|
| CS | Common/Security | Common, Security |
| DC | DICOM | Dicom |
| DT | Detector | Detector |
| DS | Dose | Dose |
| WF | Workflow | Workflow |
| PM | PatientManagement | PatientManagement |
| IM | Imaging | Imaging |
| IN | Incident | Incident |
| UP | Update | Update |
| SA | SystemAdmin | SystemAdmin |
| CB | CDBurning | CDBurning |

### Traceability Gap Detection

```bash
# Find SWR references in test code
grep -r '\[Trait("SWR"' tests/ | sort | uniq

# Compare against DOC-032 RTM entries
# Any SWR in RTM without matching test = TRACEABILITY GAP
```

### When Gaps Are Found

1. Create issue with `ra-update` + `priority-high` labels
2. Specify exact SWR-xxx ID and missing TC-xxx mapping
3. Assign to responsible team (use domain prefix to determine)

## SBOM Management Quality Gate

### Update Trigger Verification

| Trigger | Source | Action |
|---------|--------|--------|
| New NuGet component | Team A adds to Directory.Packages.props | Run Generate-SBOM.ps1, update DOC-019 + DOC-033 |
| CVE found (CVSS >= 7.0) | QA OWASP scan | Update DOC-019, create security issue, assess impact |
| Component removed | Team cleanup | Run Generate-SBOM.ps1, update DOC-019 + DOC-033 |
| Quarterly review | Calendar trigger | Run Generate-SBOM.ps1, compare with previous |
| Release build | Release preparation | Run Generate-SBOM.ps1, final verification |

### SBOM Generation

```bash
powershell -File scripts/ra/Generate-SBOM.ps1
```

### SBOM Verification Checklist

After generation:
- [ ] CycloneDX 1.5 JSON format valid
- [ ] Component count matches expected (currently 42)
- [ ] All components have: name, version, license, supplier
- [ ] No components with CVSS >= 7.0 unaddressed
- [ ] SOUP report (DOC-033) matches SBOM entries

## Implementation-to-Document Mapping (Detailed)

### NuGet Package Change

```
Code: Directory.Packages.props modified
  -> DOC-019 SBOM: Add/remove component entry
  -> DOC-033 SOUP: Update third-party component list
  -> Run: scripts/ra/Generate-SBOM.ps1
  -> Notify: QA for OWASP re-scan if new component
```

### Interface Contract Change

```
Code: src/HnVue.UI.Contracts/ modified
  -> DOC-005 SRS: Update software requirements if behavior changed
  -> DOC-032 RTM: Verify SWR-xxx mapping exists for new interface
  -> DOC-007 SDS: Update detailed design if interface shape changed
  -> Notify: All teams consuming the interface
```

### Workflow State Machine Change

```
Code: src/HnVue.Workflow/ state transitions modified
  -> DOC-004 FRS: Update functional requirements
  -> DOC-032 RTM: Add/update SWR-WF-xxx -> TC-WF-xxx mapping
  -> DOC-009 FMEA: Assess new failure modes for state transitions
  -> Notify: QA for test coverage update
```

### Security Feature Change

```
Code: src/HnVue.Security/ modified
  -> DOC-049 IEC81001: Update cybersecurity compliance
  -> DOC-045 VEX: Update vulnerability exploitability assessment
  -> DOC-046 Security Controls: Update control descriptions
  -> DOC-016 Cybersecurity Plan: Review if plan changes needed
```

### Safety-Critical Module Change

```
Code: src/HnVue.Dose/ or src/HnVue.Incident/ modified
  -> DOC-009 FMEA: Review/add failure modes
  -> DOC-008 RMP: Verify risk mitigations still adequate
  -> DOC-032 RTM: Verify test case coverage
  -> Notify: QA for mutation testing on changed code
```

## Risk Management Quality Gate

### FMEA Entry Template

```markdown
| ID | Hazard | Cause | Effect | Severity | Probability | RPN | Mitigation | Residual Risk |
|------|--------|-------|--------|----------|------------|-----|------------|---------------|
| HAZ-XXX | [hazard] | [cause] | [effect] | [1-5] | [1-5] | [SxP] | [mitigation] | [acceptable/not] |
```

### Severity Classification

| Score | Description | Example |
|-------|-------------|---------|
| 5 | Catastrophic | Wrong patient receives radiation |
| 4 | Critical | Dose exceeds 5x DRL |
| 3 | Serious | Dose between 2-5x DRL undetected |
| 2 | Minor | Workflow interruption, no patient impact |
| 1 | Negligible | Cosmetic UI issue |

## Submission Package Verification

### FDA 510(k) (DOC-036 eSTAR)

```bash
powershell -File scripts/ra/Package-Submission.ps1 -Target FDA
```

Verify package includes:
- [ ] DOC-036 eSTAR completed
- [ ] DOC-038 DICOM Conformance Statement
- [ ] DOC-050 Predicate Comparison
- [ ] DOC-035 DHF (Design History File)
- [ ] All referenced documents at correct versions

### CE Mark (DOC-037)

Verify:
- [ ] DOC-052 GSPR Checklist completed
- [ ] DOC-020 Clinical Evaluation Plan
- [ ] DOC-051 PMS/PMCF Plan

## Post-Update Verification

Before reporting RA task as COMPLETED:

1. All updated documents follow IEC 62304 version policy
2. Revision history entries added for all changes
3. No orphaned SWR entries in modified documents
4. RTM traceability verified (no gaps introduced)
5. SBOM generation script runs without errors (if SBOM modified)
6. Cross-references between documents are consistent
7. DISPATCH.md Status updated with validation evidence
