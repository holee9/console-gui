# UI Design Changelog

Design system changes, updates, and improvements for HnVue GUI application.

---

## Version History

| Version | Date | Description | Author |
|---------|------|-------------|--------|
| 1.0.0 | 2026-04-07 | Initial UISPEC documentation system | MoAI |

---

## [1.0.0] - 2026-04-07

### Added

#### UISPEC (UI Specification) Documentation System
- Created 9 UISPEC documents bridging PPT design and XAML implementation:
  - **UISPEC-001_Login**: Login screen (Slide 1-3, LoginView.xaml)
  - **UISPEC-002_Worklist**: Patient worklist (Slide 2-4, PatientListView.xaml)
  - **UISPEC-003_Studylist**: Study list (Slide 5-7, StudylistView.xaml)
  - **UISPEC-004_Acquisition**: Image acquisition (Slide 9-11, safety-critical)
  - **UISPEC-005_AddPatient**: Patient/Procedure add (Slide 8)
  - **UISPEC-006_Merge**: Image merge (Slide 12-13, 3-column layout)
  - **UISPEC-007_Settings**: Settings screen (Slide 14-22, 10 tabs)
  - **UISPEC-008_ImageViewer**: Medical image viewer
  - **UISPEC-009_SystemAdmin**: System administration with RBAC

#### Design Reference Documents
- **UI_DESIGN_MASTER_REFERENCE.md**: WPF implementation patterns, IEC 62366 compliance checklist
- **PPT slide XML files**: Extracted design source from ★HnVUE UI 변경 최종안_251118.pptx

#### Traceability
- MRD v3.0 Appendix F: UISPEC traceability matrix
- PRD v2.0 Appendix D: FR/NFR → UISPEC mapping
- SPEC-UI-001 Section 0: UISPEC reference catalog

### Changed

#### Color System Unification
- Standardized primary color to **#1B4F8A** (MahApps.Metro Blue) across all documents
- Previous: **#0066CC** (Medical Blue) in legacy documents
- Rationale: Consistency with MahApps.Metro framework and WPF theming

#### Design Token Architecture
```
CoreTokens.xaml → SemanticTokens.xaml → ComponentTokens.xaml
```
- Background: #242424 (Surface.Page), #2A2A2A (Surface.Panel)
- Primary: #1B4F8A (Brand.Primary), #2E6DB4 (Brand.PrimaryLight)
- Accent: #00AEEF (Brand.Accent)
- Status: Safe #00C853, Warning #FFD600, Emergency #D50000

### Deprecated

#### HTML Mockups
- HTML prototype files in `docs/ui_mockups/` are now reference only
- No new HTML mockups will be created
- Use UISPEC documents and PPT design source for implementation
- Reason: Direct XAML implementation from PPT reduces translation overhead

### Technical Notes

#### IEC 62366 Compliance
- Touch targets: 44×40px minimum (button height 48px recommended)
- Color contrast: All combinations pass WCAG 2.2 AA (4.5:1 minimum)
- Safety-critical UI: Emergency stop (Acquisition), patient ID mismatch protection

#### 3-Tier Architecture
```
Layer 1: MRD/PRD (Requirements)
Layer 2: UISPEC (Design Specifications)
Layer 3: SPEC/Code (Implementation)
```

#### Implementation Status
| UISPEC | XAML | Status | Compliance |
|--------|------|--------|------------|
| 001_Login | LoginView.xaml | 95% | High |
| 002_Worklist | PatientListView.xaml | 44% | Medium |
| 003_Studylist | StudylistView.xaml | 63% | Medium |
| 004_Acquisition | TBA | 0% | Pending |
| 005_AddPatient | TBA | 0% | Pending |
| 006_Merge | MergeView.xaml (planned) | 0% | Pending |
| 007_Settings | SettingsView.xaml (planned) | 0% | Pending |
| 008_ImageViewer | ImageViewer.xaml (planned) | 0% | Pending |
| 009_SystemAdmin | AdminView.xaml (planned) | 0% | Pending |

---

## Future Plans

### Phase 1 (Q2 2026)
- Complete Acquisition screen (UISPEC-004) - safety-critical priority
- Implement AddPatient screen (UISPEC-005)
- Create MergeView.xaml with drag-drop (UISPEC-006)

### Phase 2 (Q3 2026)
- Settings screen with 10 tabs (UISPEC-007)
- Medical image viewer with W/L/Zoom (UISPEC-008)
- RBAC system admin (UISPEC-009)

### Phase 3 (Q4 2026)
- Complete Worklist modality/status badges
- Studylist pagination implementation
- Full IEC 62366 validation

---

**Document Version**: 1.0.0
**Last Updated**: 2026-04-07
**Maintained By**: Design Team
