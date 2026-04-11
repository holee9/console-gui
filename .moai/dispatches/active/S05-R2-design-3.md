# DISPATCH: Design Team — S05 Round 2 (Task 3)

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-12 |
| **발행자** | Commander Center |
| **대상** | Design Team |
| **브랜치** | team/team-design |
| **유형** | S05 Round 2 추가 — WorkflowView PPT Slide 9-11 검증 |
| **우선순위** | P1 |
| **SPEC 참조** | SPEC-UI-001 / UISPEC-006 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S05-R2-design-3.md)만 Status 업데이트.

---

## 컨텍스트

S05 R2 진행 현황:
- Slides 1: LoginView ✅
- Slides 2-4: PatientListView ✅ (70%)
- Slides 5-7: StudylistView ✅
- Slides 8: AddPatientProcedureView ✅ (100%)
- **Slides 9-11: WorkflowView (Acquisition) ← 이번 DISPATCH**
- Slides 12-13: MergeView ✅ (90%)
- Slides 14-22: SettingsView — 향후

WorkflowView는 Acquisition 화면으로 **Thumbnail strip 포함이 허용**된 유일한 화면.

---

## 사전 확인

```bash
git checkout team/team-design
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
git log --oneline -5 -- src/HnVue.UI/Views/WorkflowView.xaml
```

---

## Task 1 (P1): WorkflowView.xaml PPT Slide 9-11 검증 및 개선

### 범위 (PPT Slide 9-11만)

- Slide 9: 워크플로우 상태 표시, 환자 정보 패널
- Slide 10: 이미지 획득 영역 (Acquisition Preview)
- Slide 11: Thumbnail strip (이 화면에서만 허용)

### 작업

1. 기존 WorkflowView.xaml이 PPT Slide 9-11과 일치하는지 검증
2. 누락 요소 있으면 추가, 모두 구현됐으면 COMPLETED 보고
3. ViewModel 변경 필요 시: `NEEDS_VIEWMODEL:` 기재 후 Coordinator 위임

### 금지 사항

- Slide 9-11 이외의 화면 요소 추가 금지

### 검증

```bash
dotnet build src/HnVue.UI/ 2>&1 | tail -5
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI/Views/WorkflowView.xaml
git add src/HnVue.UI/Views/WorkflowView.xaml.cs  # 필요 시
git commit -m "feat(design): UISPEC-006 WorkflowView PPT Slide 9-11 검증 및 개선"
git push origin team/team-design
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: WorkflowView 검증 | NOT_STARTED | -- | PPT Slide 9-11 기준 |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
