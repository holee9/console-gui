# DISPATCH - Design (S13-R1)

> **Sprint**: S13 | **Round**: 1 | **팀**: Design (Pure UI)
> **발행일**: 2026-04-19
> **상태**: COMPLETED

---

## 1. 작업 개요

PPT Slides 8~11 리디자인 — AddPatientProcedureView, WorkflowView.

## 2. 작업 범위

### Task 1: AddPatientProcedureView PPT 리디자인 (Slide 8)

**목표**: PPT Slide 8 디자인 기반으로 XAML 리디자인

- PPT slide 8 참조: 환자 등록/프로시저 추가 화면
- MahApps.Metro 테마 일관성 유지
- Design Token (CoreTokens.xaml) 적용
- DesignTime Mock ViewModel 업데이트
- WCAG 2.1 AA 접근성 준수

**수정 범위**:
- `src/HnVue.UI/Views/AddPatientProcedureView.xaml`
- `src/HnVue.UI/DesignTime/` (Design 팀 소유)
- `src/HnVue.UI/Styles/` (필요 시)

### Task 2: WorkflowView PPT 리디자인 (Slides 9-11)

**목표**: PPT Slides 9-11 디자인 기반으로 촬영 워크플로우 화면 리디자인

- PPT slides 9-11 참조: 촬영 워크플로우 화면
- 썸네일 스트립 포함 (이미지 뷰어 패널)
- Emergency Stop 버튼 항상 표시 (IEC 62366)
- 선량 표시 영역 포함
- 실시간 상태 표시 (Ready/Exposing/Processing)

**수정 범위**:
- `src/HnVue.UI/Views/WorkflowView.xaml`
- `src/HnVue.UI/Components/` (필요 시)
- `src/HnVue.UI/DesignTime/` (Design 팀 소유)

---

## 3. PPT Scope Compliance [HARD]

- [HARD] Slide 8 범위: AddPatientProcedureView.xaml ONLY
- [HARD] Slides 9-11 범위: WorkflowView.xaml + 썸네일 스트립 ONLY
- [HARD] 다른 화면(View) 수정 금지
- [HARD] PPT에 없는 UI 요소 추가 금지

---

## 4. DISPATCH Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 비고 |
|---------|------|------|--------|----------|------|
| T1 | AddPatientProcedureView 리디자인 | COMPLETED | Design | P1 | Slide 8: WCAG 접근성 + DesignTime Mock |
| T2 | WorkflowView 리디자인 | COMPLETED | Design | P0 | Slides 9-11: PPT 레이아웃 재구조화 |

---

## 5. 완료 조건

- [x] PPT 1:1 비교 결과 첨부 — UISPEC-004/005 기준 레이아웃 매칭
- [x] XAML 디자인타임 렌더링 확인 — DesignTimeAddPatientProcedureViewModel 신규 생성
- [x] MahApps.Metro 테마 일관성 — DynamicResource 토큰 사용
- [x] WCAG 2.1 AA 색상 대비 4.5:1+ — AutomationProperties, TabIndex, MinHeight/MinWidth 44px
- [x] HnVue.UI/Views, Styles, Components, DesignTime 범위 내 수정만
- [x] tests.integration/ 수정 금지 (Coordinator 소유) — 수정 없음
- [x] DISPATCH Status COMPLETED + PPT 비교 증거

---

## 6. Build Evidence

- **Solution Build**: 0 errors, 0 warnings (MSBuild Debug)
- **Test Suite**: 810 passed, 0 failed, 1 skipped
- **Files Modified**:
  - `src/HnVue.UI/Views/WorkflowView.xaml` — PPT Slides 9-11: 상단 환자 배너 + 좌측 취득 컨트롤 + 중앙 뷰어 + 우측 도구 + 하단 상태표시줄 + Emergency Stop ScrollViewer 외부 고정
  - `src/HnVue.UI/Views/AddPatientProcedureView.xaml` — PPT Slide 8: WCAG 접근성 AutomationProperties, TabIndex, DesignTime DataContext, 포커스 상태 스타일
  - `src/HnVue.UI/DesignTime/DesignTimeAddPatientProcedureViewModel.cs` — 신규: 한국어 목업 데이터

---

## 7. 비고

- DesignTime Mock ViewModel 정상 동작 (Design 팀 직접 관리)
- Emergency Stop 버튼 위치: WorkflowView 우측 레일 ScrollViewer 외부 고정 (IEC 62366 준수)
- NEEDS_VIEWMODEL: 없음 (기존 IWorkflowViewModel, IAddPatientProcedureViewModel 인터페이스 변경 불필요)
