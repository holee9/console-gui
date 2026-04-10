# HnVue UI Design Plan 2026
## 의료기기 Console UI 현대화 계획서

---

## 1. 개요 (Overview)

### 1.1 프로젝트 배경
- **대상**: HnVue 의료영상 장비 Console UI
- **목적**: 기존 UI를 현대적 의료기기 표준에 맞춰 재설계
- **기간**: 2026년 1Q ~ 4Q (Console 내재화 일정과 연계)

### 1.2 현황 분석
기존 UI 구성 요소 (PPT 분석 기반):
| 화면 | 설명 | 우선순위 |
|------|------|----------|
| 로그인 창 | 사용자 인증 | High |
| Acquisition 창 | 영상 획득 제어 | Critical |
| Merge 창 | 영상 병합 기능 | Medium |
| Setting 창 | 시스템 설정 | High |
| Worklist 창 | 검사 작업 목록 | Critical |
| Studylist 창 | 검사 이력 목록 | Critical |
| Add Patient/Procedure 창 | 환자/검사 등록 | High |

---

## 2. 디자인 원칙 (Design Principles)

### 2.1 규정 준수 (Regulatory Compliance)
- **FDA Guidance**: Human Factors and Usability Engineering for Medical Devices
- **IEC 62366**: Medical devices - Application of usability engineering
- **IEC 60601-1-6**: Medical electrical equipment - Usability
- **AAMI HE75**: Human factors engineering design guidelines
- **ISO 9241**: Ergonomics of human-system interaction
- **SaMD**: Software as Medical Device guidelines

### 2.2 현대적 트렌드 반영 (2026 Trends)
1. **AI-Enhanced Interfaces**: 지능형 알림 및 예측 진단
2. **Touchless Interaction**: 음성 및 제스처 제어 지원
3. **Dark Mode First**: 저조도 의료환경 최적화
4. **Accessibility**: WCAG 2.2 준수
5. **Real-time Visualization**: 환자 모니터링 대시보드 강화

### 2.3 핵심 디자인 가치
- **Safety First**: 오류 방지 및 사용자 안전 최우선
- **Clarity**: 명확한 정보 계층 구조
- **Consistency**: 일관된 비주얼 랭귀지
- **Efficiency**: 의료진 워크플로우 최적화

---

## 3. Visual Design System

### 3.1 Color Palette
```yaml
Primary:
  - Main: #0066CC (Medical Blue - 신뢰, 전문성)
  - Light: #4D94FF (Interactive Elements)
  - Dark: #004080 (Pressed/Active States)

Secondary:
  - Teal: #00BFA5 (Success states, Healthy)
  - Coral: #FF6B6B (Critical alerts, Danger)
  - Amber: #FFC107 (Warning states)

Neutral:
  - Background: #1A1A2E (Dark Mode Base)
  - Surface: #252542 (Cards, Panels)
  - Border: #3E3E5E (Dividers)
  - Text: #E0E0E0 (Primary)
  - Text Muted: #A0A0B0 (Secondary)

Semantic:
  - Error: #FF4757
  - Warning: #FFA502
  - Success: #2ED573
  - Info: #1E90FF
```

### 3.2 Typography
```yaml
Font Family:
  - Primary: "Segoe UI", system-ui, sans-serif
  - Monospace: "Consolas", "SF Mono", monospace (numbers, data)
  - Korean: "Malgun Gothic", "Apple SD Gothic Neo"

Type Scale:
  - Display: 32px/48px (Screen titles, Patient ID)
  - H1: 24px/36px (Section headers)
  - H2: 18px/28px (Card titles)
  - H3: 16px/24px (Subheaders)
  - Body: 14px/22px (Default text)
  - Caption: 12px/18px (Labels, metadata)
  - Button: 14px Semibold

Font Weights:
  - Regular: 400
  - Medium: 500
  - Semibold: 600
  - Bold: 700
```

### 3.3 Spacing System
```yaml
Base Unit: 4px
Scale:
  - xs: 4px
  - sm: 8px
  - md: 16px
  - lg: 24px
  - xl: 32px
  - 2xl: 48px
  - 3xl: 64px
```

### 3.4 Component Specs
```yaml
Button:
  Height: 36px (Medium), 44px (Large)
  Padding: 0 16px (Medium), 0 24px (Large)
  Radius: 6px
  States: Default, Hover, Active, Disabled, Loading

Input:
  Height: 36px
  Padding: 8px 12px
  Radius: 6px
  Border: 1px solid #3E3E5E
  Focus: 2px solid #4D94FF (outline)

Card:
  Radius: 12px
  Padding: 16px
  Background: #252542
  Shadow: 0 4px 12px rgba(0, 0, 0, 0.3)

Modal:
  Radius: 16px
  Max Width: 600px (Medium), 900px (Large)
  Header Height: 56px
  Footer Height: 64px
```

---

## 4. Screen-by-Screen Design Plan

### 4.1 로그인 창 (Login Screen)
```yaml
Purpose: 사용자 인증 및 권한 확인

Layout:
  - Centered Card: 400px width
  - Logo Placement: Top center, 48px height
  - Input Fields: ID, Password
  - Additional Options: Language selector (bottom right)
  - Login Button: Full width, 44px height

Key Features:
  - Keyboard focus visible indicator
  - Error message inline display
  - Auto-fill support
  - Caps lock indicator

Accessibility:
  - Tab order: ID → Password → Login
  - Enter key submits form
  - ARIA labels for screen readers
  - High contrast mode support
```

### 4.2 Worklist 창 (Worklist Screen)
```yaml
Purpose: 검사 작업 목록 조회 및 선택

Layout:
  - Header: Title + Date Filter + Search
  - Filter Bar: Today | 3Days | 1Week | All | 1Month
  - Table: Accession No | Patient ID | Name | Exam Date | Procedure
  - Action Buttons: Refresh | Export | Settings

Key Features:
  - Row hover: Background highlight
  - Selected row: Primary color border
  - Double-click: Open study
  - Real-time updates
  - Sortable columns

Color Coding:
  - Waiting: #A0A0B0 (Gray)
  - In Progress: #1E90FF (Blue)
  - Completed: #2ED573 (Green)
  - Emergency: #FF4757 (Red)
```

### 4.3 Studylist 창 (Studylist Screen)
```yaml
Purpose: 검사 이력 조회 및 관리

Layout:
  - Header: Title + Search + Date Range Picker
  - Sidebar: Study Status Filters
  - Main Content: Study Cards / Table View Toggle
  - Detail Panel: Selected study details (slide-out)

Key Features:
  - Grid/List view toggle
  - Advanced search (Patient ID, Accession No, Date Range)
  - Export to DICOM/CD
  - Study comparison mode
  - Thumbnail preview
```

### 4.4 Acquisition 창 (Acquisition Screen)
```yaml
Purpose: 영상 획득 제어 - CRITICAL PATH

Layout:
  - Left Panel: Patient & Procedure Info
  - Center: Live Image Preview (Main)
  - Right Panel: Acquisition Controls
    - Exposure Settings
    - Body Part Selection
    - Projection Buttons
    - Acquire/Cancel Buttons
  - Bottom: Image Strip (Recent acquisitions)

Key Features:
  - Large touch targets (minimum 44x44px)
  - Emergency Stop: Always visible, red, dedicated button
  - Live exposure indicator (audible + visual)
  - Body part diagram selector
  - Quick presets (Chest AP, Hand PA, etc.)

Safety Elements:
  - Confirmation dialog before high-dose acquisition
  - Radiation warning indicators
  - Equipment status monitoring
  - Error prevention (double-check for conflicts)
```

### 4.5 Merge 창 (Merge Screen)
```yaml
Purpose: 영상 병합 기능

Layout:
  - Header: Study A + Study B Selection
  - Main Area:
    - Left: Patient A Images
    - Center: Merge Controls (Sync, Preview)
    - Right: Patient B Images
  - Bottom: Preview Result + Save/Cancel

Key Features:
  - Drag-and-drop support
  - Sync preview (real-time)
  - Before/After comparison
  - Merge options selector
```

### 4.6 Setting 창 (Settings Screen)
```yaml
Purpose: 시스템 구성 설정

Layout:
  - Sidebar: Navigation (Priority, ID, Display, etc.)
  - Main Content: Setting panels
  - Bottom: Apply/Cancel/Reset buttons

Setting Categories:
  1. Priority: User privileges, Access control
  2. Display: Language, Theme, Font size
  3. Network: DICOM settings, Port configuration
  4. Storage: Database, Export paths
  5. Devices: Detector, Generator calibration
  6. RIS: HL7 interface settings

Key Features:
  - Tabbed navigation
  - Search settings
  - Import/Export config
  - Reset to defaults
  - Validation indicators
```

### 4.7 Add Patient/Procedure 창 (Patient Registration)
```yaml
Purpose: 신규 환자/검사 등록

Layout:
  - Left: Patient Info Form
  - Right: Procedure Selection
  - Bottom: Register/Cancel Buttons

Form Fields:
  - Patient ID (auto-generated toggle)
  - Name (required)
  - Date of Birth (required)
  - Sex (required)
  - Accession No
  - Referring Physician
  - Procedure Step (multiple selection)

Key Features:
  - Required field indicators
  - Auto-complete for physicians
  - Procedure search with filters
  - Patient duplicate check warning
  - Modality default presets
```

---

## 5. Interaction Design Patterns

### 5.1 Navigation
```yaml
Primary Nav:
  - Worklist (Alt+1)
  - Studylist (Alt+2)
  - Acquisition (Alt+3) - Always accessible
  - Settings (Alt+4)
  - Help (F1)

Keyboard Shortcuts:
  - Ctrl+N: New Patient/Procedure
  - Ctrl+S: Save
  - Ctrl+F: Search
  - F5: Refresh
  - ESC: Close/Cancel
  - Space: Acquire (in Acquisition screen)
```

### 5.2 Data Entry
```yaml
Best Practices:
  - Auto-focus first field
  - Default values where safe
  - Validation on blur (not on type)
  - Clear error messages
  - Undo/Redo support
  - Auto-save draft (if applicable)
```

### 5.3 Feedback Mechanisms
```yaml
Types:
  - Success: Green toast, bottom-right, 3s auto-dismiss
  - Error: Red toast, bottom-center, requires action
  - Warning: Amber toast, bottom-center, 5s auto-dismiss
  - Info: Blue toast, bottom-right, 3s auto-dismiss
  - Loading: Spinner with progress text
```

---

## 6. Component Library Structure

```
src/
├── Components/
│   ├── Common/
│   │   ├── Button.xaml/cs
│   │   ├── Input.xaml/cs
│   │   ├── Card.xaml/cs
│   │   ├── Modal.xaml/cs
│   │   └── Toast.xaml/cs
│   ├── Medical/
│   │   ├── PatientInfoCard.xaml/cs
│   │   ├── StudyThumbnail.xaml/cs
│   │   ├── AcquisitionPreview.xaml/cs
│   │   └── ExposureSettingsPanel.xaml/cs
│   └── Layout/
│       ├── Sidebar.xaml/cs
│       ├── Header.xaml/cs
│       └── StatusBar.xaml/cs
├── Themes/
│   ├── Colors.xaml
│   ├── Typography.xaml
│   └── Spacing.xaml
├── Styles/
│   ├── ButtonStyles.xaml
│   ├── InputStyles.xaml
│   └── CardStyles.xaml
└── Converters/
    ├── BoolToVisibilityConverter.cs
    └── NullToEmptyConverter.cs
```

---

## 7. Implementation Roadmap

### Phase 1: Foundation (2026 Q1, 4주)
- [x] Design system documentation
- [ ] Base component library (Button, Input, Card)
- [ ] Theme infrastructure (colors, typography, spacing)
- [ ] Accessibility audit framework

### Phase 2: Core Screens (2026 Q2, 12주)
- [ ] Login screen
- [ ] Worklist screen
- [ ] Studylist screen
- [ ] Navigation framework

### Phase 3: Critical Screens (2026 Q2-Q3, 12주)
- [ ] Acquisition screen (multiple iterations)
- [ ] Patient registration
- [ ] Settings screen

### Phase 4: Advanced Features (2026 Q3, 8주)
- [ ] Merge functionality
- [ ] Advanced search
- [ ] Export/import features
- [ ] Keyboard shortcuts

### Phase 5: Validation (2026 Q4, 8주)
- [ ] Usability testing
- [ ] Accessibility validation
- [ ] Performance optimization
- [ ] Documentation

---

## 8. Quality Assurance

### 8.1 Usability Testing
```yaml
Methods:
  - Heuristic evaluation (Nielsen's 10 principles)
  - Cognitive walkthrough
  - Think-aloud protocol
  - A/B testing for critical flows

Participants:
  - Radiographers (primary users)
  - Radiologists
  - IT administrators
  - 5 participants per iteration

Metrics:
  - Task completion rate: >95%
  - Error rate: <5%
  - Task time: <baseline or justified
  - Satisfaction: SUS score >70
```

### 8.2 Accessibility Testing
```yaml
Tools:
  - WAVE (Web Accessibility Evaluation Tool)
  - Screen reader testing (NVDA, JAWS)
  - Keyboard-only navigation
  - Color contrast analyzer

Standards:
  - WCAG 2.2 Level AA
  - FDA Human Factors Engineering
```

### 8.3 Performance Targets
```yaml
Response Times:
  - Screen load: <1s
  - Search results: <500ms
  - Image preview: <200ms
  - Button response: <100ms

Resource Usage:
  - Memory: <500MB base
  - CPU: <10% idle
  - GPU acceleration enabled
```

---

## 9. Deliverables

### 9.1 Design Artifacts
1. **Design System Document** (본 문서)
2. **Component Library** (WPF/XAML)
3. **Screen Mockups** (Figma/Pencil)
4. **Interactive Prototype** (Figma)
5. **Style Guide** (PDF/HTML)

### 9.2 Development Artifacts
1. **WPF Control Library** (.dll)
2. **Theme Resource Dictionary** (.xaml)
3. **Sample Application** (Demo)
4. **Unit Tests** (Component library)
5. **Documentation** (API docs)

### 9.3 Documentation
1. **Developer Guide**: Component usage
2. **Designer Guide**: Design system
3. **User Guide**: Screen workflows
4. **Accessibility Guide**: A11y patterns

---

## 10. Resources & References

### Design Resources
- **Figma Community**: Healthcare/Medical UI kits
- **Material Design 3**: Design principles
- **Fluent for WinUI 3**: Microsoft design system

### Regulatory References
- FDA: "Human Factors Engineering Usability"
- IEC 62366-1:2015
- IEC 60601-1-6:2013
- AAMI HE75:2009

### Books & Articles
- "Design for HCI" - Ben Shneiderman
- "Medical Device Usability" - Michael Wiklund
- "User Interface Design for Medical Devices" - Weinschenk

---

## 11. Appendix

### 11.1 Color Palette Preview
```
┌─────────────────────────────────────────────────────┐
│ Primary     │ Secondary  │ Neutral     │ Semantic    │
├─────────────────────────────────────────────────────┤
│ ■ #0066CC   │ ■ #00BFA5  │ ■ #1A1A2E   │ ■ #FF4757   │
│ ■ #4D94FF   │ ■ #FF6B6B  │ ■ #252542   │ ■ #FFA502   │
│ ■ #004080   │ ■ #FFC107  │ ■ #3E3E5E   │ ■ #2ED573   │
│             │            │ ■ #E0E0E0   │ ■ #1E90FF   │
│             │            │ ■ #A0A0B0   │             │
└─────────────────────────────────────────────────────┘
```

### 11.2 Typography Preview
```
Display 32px: 환자 ID 20251013002
H1 24px: 검사 설정
H2 18px: 영상 획득 제어
H3 16px: 검사 이력
Body 14px: 홍길동 (1988-01-01, M)
Caption 12px: 마지막 수정: 2026-04-06 10:30
```

### 11.3 Icon System
```yaml
Icon Set: Material Design Symbols (Outlined)
Size: 24px (default), 20px (small), 32px (large)
Color: Inherit (default), semantic colors for states

Key Icons:
  - worklist: view_list
  - studylist: folder_open
  - acquire: center_focus_strong
  - settings: settings
  - patient: person
  - save: save
  - cancel: close
  - delete: delete
  - export: file_download
  - refresh: refresh
```

---

**Version**: 1.0
**Date**: 2026-04-06
**Author**: MoAI Design System Team
**Status**: Draft for Review
