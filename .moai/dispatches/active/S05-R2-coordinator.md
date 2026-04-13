# DISPATCH: Coordinator — S05 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-12 |
| **발행자** | Commander Center |
| **대상** | Coordinator |
| **브랜치** | team/coordinator |
| **유형** | S05 Round 2 — MergeView + WorkflowView ViewModel 보강 |
| **우선순위** | P1 |
| **SPEC 참조** | SPEC-UI-001 / UISPEC-005, UISPEC-006 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S05-R2-coordinator.md)만 Status 업데이트.

---

## 컨텍스트

Design Team이 두 화면에서 ViewModel 추가를 요청함:

**MergeView (PPT Slide 12-13)**: 스터디 체크박스 바인딩 필요
- `SelectedStudies` 컬렉션
- `StudyItem.IsSelected` 속성

**WorkflowView (PPT Slide 9-11)**: 3열 레이아웃 구현을 위한 ViewModel 속성 필요
- `PreviewImage` (ImageSource) — AcquisitionPreview 바인딩
- `ThumbnailList` (ObservableCollection<StudyThumbnail>)
- `SelectedPatient` (PatientInfo) — 환자 정보 패널 바인딩
- `WorkflowState` (enum: IDLE/READY/EXPOSING)

**의존성**: Design은 이 ViewModel 속성이 추가된 후 WorkflowView XAML 3열 레이아웃 구현 가능.

---

## 사전 확인

```bash
git checkout team/coordinator
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
# 현재 ViewModel 확인
grep -r "MergeViewModel\|WorkflowViewModel\|IWorkflowViewModel" src/HnVue.UI.Contracts/ src/HnVue.UI.ViewModels/ 2>/dev/null | head -20
```

---

## Task 1 (P1): WorkflowView ViewModel 보강 [우선]

Design이 WorkflowView 3열 레이아웃 구현 대기 중 — 이 Task가 먼저.

### 대상 파일

- `src/HnVue.UI.Contracts/` — IWorkflowViewModel 인터페이스 확장
- `src/HnVue.UI.ViewModels/` — WorkflowViewModel 구현 추가

### 구현 항목

1. `IWorkflowViewModel`에 속성 추가:
   - `PreviewImage` (ImageSource?)
   - `ThumbnailList` (ObservableCollection<object> 또는 적절한 타입)
   - `SelectedPatient` (PatientInfo 또는 적절한 타입)
   - `WorkflowState` (기존 enum 사용 또는 신규)
2. `WorkflowViewModel`에 구현 (mock 값으로 초기화 가능)
3. 단위 테스트 (최소 각 속성 1개)

---

## Task 2 (P2): MergeView ViewModel 보강

### 대상 파일

- `src/HnVue.UI.Contracts/` — IMergeViewModel 인터페이스 확장
- `src/HnVue.UI.ViewModels/` — MergeViewModel 구현 추가

### 구현 항목

1. `StudyItem` 모델에 `IsSelected` bool 속성 추가 (이미 있으면 확인만)
2. `IMergeViewModel`에 `SelectedStudies` 컬렉션 속성 추가
3. `MergeViewModel`에 구현 추가

### 검증

```bash
dotnet build HnVue.sln 2>&1 | tail -5
dotnet test tests/ 2>&1 | tail -10
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI.Contracts/ src/HnVue.UI.ViewModels/ tests/
git commit -m "feat(coordinator): WorkflowView + MergeView ViewModel 보강 — Slide 9-11 바인딩 지원"
git push origin team/coordinator
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: WorkflowView ViewModel (P1) | COMPLETED | 2026-04-12 | PreviewImagePath+ThumbnailList+SelectedPatient 추가, 53/53 통과 |
| Task 2: MergeView ViewModel (P2) | COMPLETED | 2026-04-12 | StudyItem+SelectedStudies 추가, 53/53 통과 |
| Git 완료 프로토콜 | COMPLETED | 2026-04-12 | PR #77 업데이트 (commit b3d265d) |
