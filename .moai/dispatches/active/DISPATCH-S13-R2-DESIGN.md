# DISPATCH - Design (S13-R2)

> **Sprint**: S13 | **Round**: 2 | **팀**: Design (Pure UI)
> **발행일**: 2026-04-19
> **상태**: NOT_STARTED

---

## 1. 작업 개요

StudylistView(Slides 5-7) 접근성 개선 + DoseDisplayView 리디자인 검토.

## 2. 작업 범위

### Task 1: StudylistView 접근성 개선 (Slides 5-7)

**목표**: 기존 StudylistView에 WCAG 2.1 AA 접근성 개선 적용

- AutomationProperties 속성 추가 (모든 대화형 요소)
- TabIndex 논리적 순서 설정
- 색상 대비 4.5:1+ 검증
- 스크린 리더 호환성 검증
- MahApps.Metro 테마 일관성 유지

**수정 범위**:
- `src/HnVue.UI/Views/StudylistView.xaml`
- `src/HnVue.UI/DesignTime/` (필요 시 Mock 업데이트)

### Task 2: DoseDisplayView 레이아웃 개선

**목표**: DoseDisplayView (210 lines) 레이아웃 개선

- 선량 표시 가독성 향상
- IEC 60601-2-54 준수 표시 형식 검토
- Design Token (CoreTokens.xaml) 적용
- WCAG 2.1 AA 준수

**수정 범위**:
- `src/HnVue.UI/Views/DoseDisplayView.xaml`
- `src/HnVue.UI/Styles/` (필요 시)

---

## 3. PPT Scope Compliance [HARD]

- [HARD] Slides 5-7 범위: StudylistView.xaml ONLY
- [HARD] DoseDisplayView는 의료영상 도메인 UI — 선량 표시 표준 준수
- [HARD] 다른 화면(View) 수정 금지
- [HARD] PPT에 없는 UI 요소 추가 금지

---

## 4. DISPATCH Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 비고 |
|---------|------|------|--------|----------|------|
| T1 | StudylistView 접근성 개선 | IN_PROGRESS | Design | P1 | Slides 5-7 |
| T2 | DoseDisplayView 레이아웃 개선 | NOT_STARTED | Design | P2 | 선량 표시 |

---

## 5. 완료 조건

- [ ] WCAG 2.1 AA 색상 대비 4.5:1+
- [ ] AutomationProperties 속성 완비
- [ ] HnVue.UI/Views, Styles, DesignTime 범위 내 수정만
- [ ] tests.integration/ 수정 금지 (Coordinator 소유)
- [ ] DISPATCH Status COMPLETED + 빌드 증거

---

## 6. Build Evidence

_(작업 완료 후 기록)_

---

## 7. 비고

- DesignTime Mock ViewModel은 Design 팀이 직접 관리
- _CURRENT.md 및 다른 팀 DISPATCH 파일 수정 금지 (CC 전용)
- DISPATCH 파일(active/, completed/) 생성/이동/삭제 금지 (CC 전용)
