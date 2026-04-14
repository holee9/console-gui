# DISPATCH: Design — S08 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center |
| **대상** | Design |
| **브랜치** | team/team-design |
| **유형** | S08 R1 — StudylistView XAML 구현 (PPT slides 5-7) |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S08-R1-team-design.md)만 Status 업데이트.

---

## 컨텍스트

S08-R1은 StudylistView (PPT slides 5-7) 구현 라운드.
Design Team은 PPT 지정 페이지(5-7)만 구현. PPT 범위 외 UI 요소 절대 금지.

**PPT Scope Compliance [HARD — Issue #59]**:
- Slides 5-7: StudylistView.xaml ONLY
- 썸네일 스트립, 이미지 뷰어, Acquisition 컴포넌트는 PPT slides 9-11 범위 — 절대 포함 금지

---

## 사전 확인

```bash
git checkout team/team-design
git pull origin main
```

---

## Task 1 (P1): StudylistView.xaml 구현

PPT slides 5-7 기반 StudylistView XAML 구현.

**수행 사항**:
- PPT slides 5-7 정밀 분석 (레이아웃, 색상, 컴포넌트, 인터랙션)
- CoreTokens.xaml 디자인 토큰 적용
- MahApps.Metro 스타일 상속
- DesignTime Mock DataContext 설정 (d:DataContext)
- IStudylistViewModel 바인딩 (Coordinator가 제공, 없으면 DesignTime Mock 사용)
- WCAG 2.1 AA 접근성 준수 (색 대비 4.5:1, 터치타겟 44x44px)

**목표**: StudylistView.xaml 완성 + PPT 1:1 비교 결과

---

## Task 2 (P2): PPT 1:1 비교 검증

구현 결과를 PPT 원본과 1:1 비교.

**수행 사항**:
- 각 UI 요소를 PPT slides 5-7과 비교
- 누락 요소 또는 범위 외 요소 확인
- 스크린샷 캡처 (WindowHandle only)

**목표**: PPT 1:1 비교 보고

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI/Views/ src/HnVue.UI/Styles/ src/HnVue.UI/DesignTime/
git commit -m "feat(design): StudylistView XAML 구현 — PPT slides 5-7 (#issue)"
git push origin team/team-design
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: StudylistView.xaml 구현 (P1) | COMPLETED | | |
| Task 2: PPT 1:1 비교 (P2) | COMPLETED | | |
| Git 완료 프로토콜 | COMPLETED | | |
