# HnVue Architecture Analysis & UI Redesign Strategy

## 📊 Executive Summary

**Project**: HnVue - 의료영상 장비 Console Application  
**Type**: WPF (Windows Presentation Foundation) Desktop Application (.NET 8)  
**Architecture**: Modular loosely-coupled design  
**UI (User Interface) Framework**: MahApps.Metro + Custom DesignSystem2026  

---

## 🔍 Module Architecture Analysis

### Current Module Structure (14 modules)

```
HnVue.App (Main Entry Point)
├── References 13 business modules
└── HnVue.UI (WPF UI Layer)
    ├── HnVue.UI.Contracts (Interfaces only - NO dependencies)
    ├── HnVue.UI.ViewModels (MVVM ViewModels)
    └── Components (Modal, Toast, AcquisitionPreview, etc.)

Business Modules (13):
├── HnVue.Common (공통 인프라)
├── HnVue.Data (데이터 액세스)
├── HnVue.Security (인증/보안)
├── HnVue.Dicom (DICOM 처리)
├── HnVue.Workflow (워크플로우 엔진)
├── HnVue.Imaging (영상 처리)
├── HnVue.Dose (선량 관리)
├── HnVue.PatientManagement (환자 정보)
├── HnVue.Incident (사건 관리)
├── HnVue.Update (업데이트)
├── HnVue.SystemAdmin (시스템 관리)
└── HnVue.CDBurning (CD 굽기)
```

### ✅ UI Independence Verification

**결론**: **HnVue.UI는 모듈 독립적 구조를 가집니다**

**증거**:
1. **인터페이스 분리**: `HnVue.UI.Contracts` 프로젝트가 순수 인터페이스만 정의
2. **직접 참조 없음**: HnVue.UI.csproj는 다른 비즈니스 모듈을 직접 참조하지 않음
3. **의존성 주입**: CommunityToolkit.Mvvm의 ObservableObject로 MVVM 패턴 구현
4. **이벤트 기반 통신**: ViewModels는 인터페이스(ILoginViewModel, IMainViewModel 등)를 통해 비즈니스 로직과 통신

**아키텍처 다이어그램**:
```
┌─────────────────────────────────────────────────────────────┐
│                    HnVue.App (Main)                       │
│  ┌────────────────────────────────────────────────────────┐  │
│  │              HnVue.UI (WPF Layer)                     │  │
│  │  ┌──────────────────────────────────────────────────┐  │  │
│  │  │   UI.Contracts (Interfaces)                     │  │  │
│  │  └──────────────────────────────────────────────────┘  │  │
│  │  ┌──────────────────────────────────────────────────┐  │  │
│  │  │   UI.ViewModels (ObservableObject)             │  │  │
│  │  └──────────────────────────────────────────────────┘  │  │
│  │  ┌──────────────────────────────────────────────────┐  │  │
│  │  │   Components (XAML + code-behind)               │  │  │
│  │  │   - DesignSystem2026.xaml                    │  │  │
│  │  │   - Modal, Toast, AcquisitionPreview           │  │  │
│  │  └──────────────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                             │
│  Business Modules (through Interfaces)                      │
│  ┌─────────────┬──────────────┬──────────────┬─────────┐ │
│  │ HnVue.Data │ HnVue.Security│ HnVue.Workflow│  ...    │ │
│  └─────────────┴──────────────┴──────────────┴─────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎨 Existing Design System Analysis

### DesignSystem2026.xaml Status

**위치**: `src/HnVue.UI/Themes/DesignSystem2026.xaml`

**구현된 리소스**:
- ✅ Color Palette (Primary, Secondary, Neutral, Semantic)
- ✅ Typography Scale (Display → Caption, 7 sizes)
- ✅ Spacing System (4px base unit, XS~XXXL)
- ✅ Corner Radius (SM~XL, 4~16px)
- ✅ Brushes (16 SolidColorBrush)

**컬러 팔레트** (의료기기 최적화):
```xml
Primary: #1B4F8A (MahApps.Metro Blue - 신뢰, 전문성)
Secondary: Teal(#00BFA5), Coral(#FF6B6B), Amber(#FFC107)
Neutral (Dark Mode): BG(#1A1A2E), Surface(#252542), Border(#3E3E5E)
Semantic: Error(#FF4757), Warning(#FFA502), Success(#2ED573), Info(#1E90FF)
```

### Component Library Status

**기존 컴포넌트** (src/HnVue.UI/Components/):
- ✅ Common: Modal.xaml, Toast.xaml, ViewModelBase.cs
- ✅ Medical: AcquisitionPreview.xaml, PatientInfoCard.xaml, StudyThumbnail.xaml
- ✅ Layout: Header.xaml, StatusBar.xaml

**스타일** (src/HnVue.UI/Styles/):
- ✅ ButtonStyles.xaml
- ✅ InputStyles.xaml  
- ✅ CardStyles.xaml

**통합 방식** (ComponentLibrary.xaml):
```xml
<ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source="DesignSystem2026.xaml" />
    <ResourceDictionary Source="Styles/ButtonStyles.xaml" />
    <ResourceDictionary Source="Styles/InputStyles.xaml" />
    <ResourceDictionary Source="Styles/CardStyles.xaml" />
    <ResourceDictionary Source="Components/Common/Modal.xaml" />
    <!-- ... more components ... -->
</ResourceDictionary.MergedDictionaries>
```

---

## 🚨 Previous Mistake Analysis

### What Went Wrong

1. **Project Type Misidentification**
   - **Assumed**: Web browser application
   - **Actual**: WPF (Windows Presentation Foundation) desktop application (WinExe)
   - **Result**: Created HTML (Hypertext Markup Language) mockups instead of XAML prototypes

2. **Missed Pencil/Figma Requirement**
   - **Instruction**: "Pencil or Figma free medical UI design"
   - **Action**: Created HTML mockups without using design tools
   - **Result**: Lost opportunity for proper visual design iteration

3. **Insufficient Architecture Analysis**
   - **Missed**: Existing DesignSystem2026.xaml already implemented
   - **Missed**: MahApps.Metro already integrated
   - **Missed**: Module independence already achieved

---

## 🎯 Corrected Strategy

### 1. WPF XAML Prototypes (Proper Approach)

**Create Instead of HTML**:
- `LoginWindow.xaml` + `LoginViewModel.cs`
- `WorklistView.xaml` + `WorklistViewModel.cs`
- `StudylistView.xaml` + `StudylistViewModel.cs`
- `AcquisitionView.xaml` + `AcquisitionViewModel.cs`
- `MergeView.xaml` + `MergeViewModel.cs`
- `SettingsView.xaml` + `SettingsViewModel.cs`
- `AddPatientView.xaml` + `AddPatientViewModel.cs`

**Location**: `src/HnVue.UI/Views/`

### 2. Pencil/Figma Integration

**Pencil** (Free, Open Source):
- Download: https://pencil.evolus.vn/
- Use for: Wireframes, basic mockups
- Export: PNG/PDF for documentation

**Figma Community** (Free tier):
- Search: "medical", "healthcare", "dashboard"
- Popular kits:
  - Healthcare Dashboard Kit (if available)
  - Medical UI components
- Benefit: Modern design patterns, accessibility built-in

**Workflow**:
1. Research Figma Community kits
2. Create mockup in Pencil/Figma
3. Export design tokens (colors, spacing)
4. Convert to WPF XAML resources

### 3. Usability Evaluation Framework

**Heuristic Evaluation Rubric** (0-10 scale):
| Screen | Visibility | Consistency | Match Real-World | Error Prevention | Flexibility | Total |
|-------|-----------|-------------|------------------|-----------------|------------|-------|
| Login | ? | ? | ? | ? | ? | ? |
| Worklist | ? | ? | ? | ? | ? | ? |
| Acquisition | ? | ? | ? | ? | ? | ? |

**Task Completion Time Measurement**:
- Baseline: Current UI task times
- Target: After redesign
- Metric: Time to complete common workflows

**Error Rate Tracking**:
- Critical: Patient ID mismatch
- High: Acquisition errors
- Medium: Navigation errors
- Low: Preference changes

**SUS (System Usability Scale) Satisfaction Survey**:
- 10 questions, 0-4 scale
- Target: >70 (good)

### 4. Agile UI Iteration Process

```
┌─────────────────────────────────────────────────────────────┐
│                    UI Change Request                      │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
              ┌───────────────────────┐
              │  Usability Evaluation   │
              │  (Heuristic + Task Time)│
              └───────────────────────┘
                          │
                ┌─────────────┴─────────────┐
                ▼                             │
        ┌─────────────┐              ┌──────────────┐
        │ PASS        │              │ FAIL         │
        └──────┬──────┘              └──────┬───────┘
               │                              │
               ▼                              ▼
        ┌──────────────┐              ┌──────────────┐
        │ Implement   │              │ Investigate  │
        │ Change      │              │ Root Cause   │
        └──────┬───────┘              └──────┬───────┘
               │                              │
               ▼                              ▼
        ┌──────────────────────────────────────┐
        │      Deploy to Production            │
        └──────────────────────────────────────┘
```

**Safety Gates**:
- CRITICAL PATH changes require extra validation
- Patient identification components require 100% error-free rate
- Emergency controls must remain visible at all times

---

## 📋 Recommended Next Steps

### Phase 1: Discovery (Week 1)
1. ✅ Architecture analysis (COMPLETED)
2. Research Figma Community medical kits
3. Download and evaluate Pencil templates
4. Document current UI component inventory

### Phase 2: Design (Week 2)
1. Create Pencil/Figma mockups for 7 screens
2. Document design tokens (colors, spacing, typography)
3. Create heuristic evaluation rubric
4. Establish approval workflow

### Phase 3: Prototyping (Week 3)
1. Convert mockups to WPF XAML Views
2. Create ViewModels with CommunityToolkit.Mvvm
3. Wire up navigation between screens
4. Integrate with existing DesignSystem2026

### Phase 4: Testing (Week 4)
1. Heuristic evaluation
2. Task completion time measurement
3. SUS satisfaction survey
4. Accessibility audit (WAVE, keyboard nav)

### Phase 5: Deployment (Week 5)
1. Pilot deployment
2. Monitor error rates
3. Collect feedback
4. Full rollout

---

## 🎁 Key Insights

### 1. Modularity Already Achieved
- HnVue.UI is independent of business modules
- UI can be updated without touching business logic
- Well-designed contract interfaces enable this

### 2. Design System Already Exists
- DesignSystem2026.xaml is comprehensive
- Matches docs/ui_design_plan_2026.md
- Just need to extend it for new screens

### 3. Modern Foundation Ready
- MahApps.Metro provides modern WPF controls
- CommunityToolkit.Mvvm enables clean MVVM
- LiveChartsCore.SkiaSharpView for data viz

### 4. Agile Iteration Possible
- Loose coupling enables independent UI updates
- Heuristic evaluation provides quick feedback
- Rollback mechanisms can be designed

---

## 📌 Critical Success Factors

1. **Use Pencil/Figma for visual design** - NOT HTML
2. **Create WPF XAML Views** - NOT web mockups
3. **Reuse existing DesignSystem2026** - don't recreate
4. **Follow MahApps.Metro patterns** - already integrated
5. **Maintain module independence** - proven architecture
6. **Implement usability framework** - enables agile iteration

---

## 📊 Architecture Independence Score

| Criterion | Score | Evidence |
|-----------|-------|----------|
| Interface Segregation | 10/10 | HnVue.UI.Contracts exists |
| Direct Dependencies | 10/10 | UI has zero direct biz module refs |
| Technology Swappable | 9/10 | MahApps.Metro can be replaced |
| UI Theme Independent | 10/10 | DesignSystem2026 is externalized |
| Deploy Independently | 9/10 | UI can be updated separately |
| **TOTAL** | **9.6/10** | **Excellent Modularity** |

---

**Version**: 1.0  
**Date**: 2026-04-06  
**Author**: MoAI Strategic Analysis (with Ultrathink)  
**Status**: Architecture analysis complete - Ready for UI redesign phase
