# DISPATCH: Coordinator — S05 Round 2 (Task 2)

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-12 |
| **발행자** | Commander Center |
| **대상** | Coordinator |
| **브랜치** | team/coordinator |
| **유형** | S05 Round 2 추가 — WorkflowView ViewModel 보강 |
| **우선순위** | P1 (Design BLOCKED) |
| **SPEC 참조** | SPEC-UI-001 / UISPEC-006 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S05-R2-coordinator-2.md)만 Status 업데이트.

---

## 컨텍스트

S05-R2 Coordinator Task 2 (MergeView ViewModel) main 머지 완료.
**Design Team이 WorkflowView 3열 레이아웃 구현을 위해 BLOCKED 상태**:
ViewModel 속성 4개가 없어 착수 불가.

이 DISPATCH는 Design의 블로킹을 해제하기 위한 긴급 Task.

---

## 사전 확인

```bash
git checkout team/coordinator
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
# 현재 WorkflowViewModel 확인
grep -r "WorkflowViewModel\|IWorkflowViewModel" src/HnVue.UI.Contracts/ src/HnVue.UI.ViewModels/ 2>/dev/null | head -20
```

---

## Task 1 (P1 — URGENT): WorkflowView ViewModel 보강

Design이 WorkflowView.xaml 3열 레이아웃을 구현하려면 다음 속성이 필요.

### 대상 파일

- `src/HnVue.UI.Contracts/ViewModels/IWorkflowViewModel.cs` — 인터페이스 확장
- `src/HnVue.UI.ViewModels/ViewModels/WorkflowViewModel.cs` — 구현 추가

### 구현 항목

1. `IWorkflowViewModel`에 속성 추가:
   - `BitmapSource? PreviewImage` — AcquisitionPreview 바인딩
   - `ObservableCollection<StudyItem> ThumbnailList` — Thumbnail strip 바인딩
   - `PatientDto? SelectedPatient` — 환자 정보 패널 바인딩 (또는 적절한 타입)
   - `WorkflowState WorkflowState` — IDLE/READY/EXPOSING (기존 enum 확인 후 활용)
2. `WorkflowViewModel`에 구현 (초기값: null/빈 컬렉션)
3. 단위 테스트 최소 1개 per 속성

### 확인 사항

- `WorkflowState` enum: `HnVue.Common` 또는 `HnVue.Workflow`에 이미 있을 가능성 높음 — 중복 생성 금지
- `PatientDto` 또는 `PatientInfo`: 기존 타입 재사용

### 검증

```bash
dotnet build HnVue.sln 2>&1 | tail -5
dotnet test tests/ 2>&1 | tail -5
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI.Contracts/ src/HnVue.UI.ViewModels/ tests/
git commit -m "feat(coordinator): WorkflowView ViewModel 보강 — PreviewImage/ThumbnailList/SelectedPatient/WorkflowState"
git push origin team/coordinator
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: WorkflowView ViewModel (P1) | COMPLETED | 2026-04-13 | commit 1913fac, main 머지 완료 (b1418e3) |
| Git 완료 프로토콜 | COMPLETED | 2026-04-13 | main 머지 완료, Design 블로킹 해제 |
