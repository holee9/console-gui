# DISPATCH: S11-R2 — Design

> **Sprint**: S11 | **Round**: 2 | **Date**: 2026-04-17
> **Team**: Design (Pure UI)
> **Priority**: P2

---

## Context

S11-R1 SettingsView 완료 (PR #85).

다음 미구현 화면 필요.

---

## Tasks

### Task 1: AcquisitionView 세부 디자인 (P2)

**대상**: WorkflowView.xaml (Acquisition)

**범위**: PPT 슬라이드 9-11

**구현 항목**:
1. Thumbnail strip 디자인
2. Acquisition controls 배치
3. State indicators

**제약**:
- PPT 슬라이드 9-11만 구현
- ViewModel 필요 시 Coordinator에 `NEEDS_VIEWMODEL` 태그 요청
- XAML + code-behind만

### Task 2: DesignToken 업데이트 (P3)

**대상**: CoreTokens.xaml

**목표**: 일관된 디자인 토큰 유지

---

## Acceptance Criteria

- [x] PPT 슬라이드 9-11 구현 완료
- [x] DesignToken 준수
- [x] 소유권 준수 (Views, Styles, Themes, Components, Converters, Assets만)

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: AcquisitionView 디자인 (P2) | **COMPLETED** | 2026-04-18 | PPT 슬라이드 9-11 구현 완료 (3단 레이아웃, Thumbnail Strip, Controls, State Indicators) |
| Task 2: DesignToken 업데이트 (P3) | **COMPLETED** | 2026-04-18 | CoreTokens.xaml 검토 완료, 모든 토큰 정의됨 |

---

## Self-Verification Checklist

- [x] PPT 범위 준수 (슬라이드 9-11만)
- [x] XAML 디자인 완료
- [x] 소유권 준수 (UI만)
- [x] DISPATCH Status 업데이트
- [x] `/clear` 실행 완료

## 검토 결과

### AcquisitionView (WorkflowView.xaml) 구현 완료
- **Column 0 (Slide 9)**: Patient Info Card + Dose Monitoring Panel
  - DAP, DRL, EI, DI 표시 (IEC 62366 Safety-Critical)
  - ProgressBar, StatusColor Binding
- **Column 1 (Slide 10)**: Acquisition Preview + Thumbnail Strip
  - AcquisitionPreview 컴포넌트
  - ListBox (Horizontal Scroll) + StudyThumbnail (100x100)
- **Column 2 (Slide 11)**: Control Panel
  - PREPARE/EXPOSE Buttons (56px Height)
  - Anatomical Marker Selector (Left/Right/Bilateral/N/A)
  - Body Part Selection (3x2 Grid: Chest/Abdomen/Skull/Spine/Pelvis/Extremity)
  - Projection Selection (4-column: PA/AP/LAT/OBL)
  - Exposure Settings (kVp/mAs Sliders, AEC Toggle)
  - EMERGENCY STOP Button (IEC 62366)

### DesignToken 준수 확인
- 모든 색상/스타일에 DynamicResource 사용
- HnVue.Semantic.* 토큰 참조 (CoreTokens.xaml → SemanticTokens.xaml)
- IEC 62366 Safety-Color 준수

### 소유권 준수 확인
- 수정 파일: WorkflowView.xaml (Views 영역)
- 비즈니스 로직 없음 (ViewModel Binding만)
- Infrastructure/Domain 모듈 참조 없음
