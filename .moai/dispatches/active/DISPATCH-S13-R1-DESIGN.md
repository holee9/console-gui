# DISPATCH - Design (S13-R1)

> **Sprint**: S13 | **Round**: 1 | **팀**: Design (Pure UI)
> **발행일**: 2026-04-19
> **상태**: NOT_STARTED

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
| T1 | AddPatientProcedureView 리디자인 | NOT_STARTED | Design | P1 | Slide 8 |
| T2 | WorkflowView 리디자인 | NOT_STARTED | Design | P0 | Slides 9-11 |

---

## 5. 완료 조건

- [ ] PPT 1:1 비교 결과 첨부
- [ ] XAML 디자인타임 렌더링 확인
- [ ] MahApps.Metro 테마 일관성
- [ ] WCAG 2.1 AA 색상 대비 4.5:1+
- [ ] HnVue.UI/Views, Styles, Components, DesignTime 범위 내 수정만
- [ ] tests.integration/ 수정 금지 (Coordinator 소유)
- [ ] DISPATCH Status COMPLETED + PPT 비교 증거

---

## 6. Build Evidence

_(작업 완료 후 기록)_

---

## 7. 비고

- NEEDS_VIEWMODEL 필요 시 DISPATCH에 태그 기재
- DesignTime Mock ViewModel은 Design 팀이 직접 관리
- Emergency Stop 버튼 위치: WorkflowView 우상단 고정
