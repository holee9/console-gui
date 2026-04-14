# DISPATCH: Design — S07 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression v2) |
| **대상** | Design |
| **브랜치** | team/design |
| **유형** | S07 R2 — PPT 스펙 검증 + UX 일관성 |
| **우선순위** | P2-Medium |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R2-design.md)만 Status 업데이트.

---

## 컨텍스트

S06-R1 SettingsView 완료. 7개 핵심 뷰 모두 구현됨.
이번 라운드는 품질 검증 중심.

---

## 사전 확인

```bash
git checkout team/design
git pull origin main
```

---

## Task 1 (P1): PPT 스펙 1:1 검증

7개 핵심 뷰를 PPT 원본과 비교 검증:
- LoginView (PPT Slide 1)
- PatientListView/WorklistView (PPT Slides 2-4)
- StudylistView (PPT Slides 5-7)
- AddPatientProcedureView (PPT Slide 8)
- WorkflowView/Acquisition (PPT Slides 9-11)
- MergeView (PPT Slides 12-13)
- SettingsView (PPT Slides 14-22)

각 뷰별 차이점 리포트 작성.

---

## Task 2 (P2): 3테마 일관성 검증

Light, Dark, High Contrast 테마에서:
- 모든 뷰 정상 렌더링 확인
- MahApps.Metro 리소스 키 일관성
- 색상 대비비 4.5:1 이상 (WCAG 2.1 AA)

---

## Task 3 (P3): 접근성 검증

IEC 62366 / WCAG 2.1 AA 기준:
- 터치 타겟 44x44px 이상
- 키보드 Tab 네비게이션
- 긴급정지 버튼 가시성 (Acquisition 화면)
- AutomationProperties 설정

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI/Styles/ src/HnVue.UI/Views/ src/HnVue.UI/Themes/
git commit -m "style(design): S07-R2 PPT 검증 + 테마 일관성 + 접근성 (#issue)"
git push origin team/design
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: PPT 스펙 1:1 검증 (P1) | NOT_STARTED | | |
| Task 2: 3테마 일관성 검증 (P2) | NOT_STARTED | | |
| Task 3: 접근성 검증 (P3) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |
