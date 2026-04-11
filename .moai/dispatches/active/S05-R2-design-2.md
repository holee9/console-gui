# DISPATCH: Design Team — S05 Round 2 (Task 2)

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-12 |
| **발행자** | Commander Center |
| **대상** | Design Team |
| **브랜치** | team/team-design |
| **유형** | S05 Round 2 추가 — MergeView PPT Slide 12-13 구현 |
| **우선순위** | P2 |
| **SPEC 참조** | SPEC-UI-001 / UISPEC-005 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S05-R2-design-2.md)만 Status 업데이트.

---

## 컨텍스트

S05 R2 Task 1에서 AddPatientProcedureView (Slide 8) 검증 완료.
PPT 진행 현황:
- Slides 1: LoginView ✅
- Slides 2-4: PatientListView ✅ (70%)
- Slides 5-7: StudylistView ✅
- Slides 8: AddPatientProcedureView ✅ (100%)
- Slides 9-11: WorkflowView — 향후
- **Slides 12-13: MergeView ← 이번 DISPATCH**
- Slides 14-22: SettingsView — 향후

---

## 사전 확인

```bash
git checkout team/team-design
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
# 현재 MergeView 상태 확인
git log --oneline -5 -- src/HnVue.UI/Views/MergeView.xaml
```

---

## Task 1 (P2): MergeView.xaml PPT Slide 12-13 구현

### 범위 (PPT Slide 12-13만)

Design Team 규칙:
- XAML 레이아웃, 스타일, 바인딩만 수정
- ViewModel 변경 필요 시: Status에 `NEEDS_VIEWMODEL:` 기재 후 Coordinator 위임

### 구현 대상 (PPT Slide 12-13 기준)

1. **환자 선택 패널** (Slide 12): 병합 대상 환자 목록
2. **스터디 선택 패널** (Slide 12): 병합할 스터디 체크박스
3. **병합 확인/실행** (Slide 13): 병합 결과 미리보기 + 실행 버튼

### 금지 사항

- Slide 12-13 이외의 화면 요소 추가 금지
- Thumbnail strip, ImageViewer 요소 추가 금지 (Issue #59 재발 방지)

### 검증

```bash
dotnet build src/HnVue.UI/ 2>&1 | tail -5
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI/Views/MergeView.xaml
git add src/HnVue.UI/Views/MergeView.xaml.cs  # 필요 시
git commit -m "feat(design): UISPEC-005 MergeView PPT Slide 12-13 구현"
git push origin team/team-design
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: MergeView 구현 | NOT_STARTED | -- | PPT Slide 12-13 기준 |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
