# DISPATCH: Coordinator — S05 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-12 |
| **발행자** | Commander Center |
| **대상** | Coordinator |
| **브랜치** | team/coordinator |
| **유형** | S05 Round 2 — MergeView ViewModel 보강 |
| **우선순위** | P2 |
| **SPEC 참조** | SPEC-UI-001 / UISPEC-005 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S05-R2-coordinator.md)만 Status 업데이트.

---

## 컨텍스트

Design Team이 MergeView (PPT Slide 12-13) 검증 완료.
MergeView XAML에서 스터디 체크박스 바인딩을 위한 ViewModel 속성이 필요함:
- `SelectedStudies` 컬렉션
- `StudyItem.IsSelected` 속성
- 체크박스 바인딩 지원

---

## 사전 확인

```bash
git checkout team/coordinator
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
# 현재 MergeView ViewModel 확인
grep -r "MergeViewModel\|IMergeViewModel" src/HnVue.UI.Contracts/ src/HnVue.UI.ViewModels/ 2>/dev/null | head -20
```

---

## Task 1 (P2): MergeView ViewModel 보강

### 대상 파일

- `src/HnVue.UI.Contracts/` — IMergeViewModel 인터페이스 확장
- `src/HnVue.UI.ViewModels/` — MergeViewModel 구현 추가

### 구현 항목

1. `StudyItem` 모델에 `IsSelected` bool 속성 추가 (이미 있으면 확인만)
2. `IMergeViewModel`에 `SelectedStudies` 컬렉션 속성 추가
3. `MergeViewModel`에 구현 추가
4. 단위 테스트 추가 (`tests.integration/` 또는 ViewModel 테스트)

### 제약

- UI.Contracts 변경 시 영향받는 팀(Design) 없음 — 이번은 추가이므로 파급 없음
- 기존 MergeView XAML 바인딩은 이미 있는 것 유지

### 검증

```bash
dotnet build HnVue.sln 2>&1 | tail -5
dotnet test tests/ -k "Merge" 2>&1 | tail -10
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI.Contracts/ src/HnVue.UI.ViewModels/ tests/
git commit -m "feat(coordinator): MergeView ViewModel 보강 — SelectedStudies + StudyItem.IsSelected"
git push origin team/coordinator
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: MergeView ViewModel 보강 | NOT_STARTED | -- | SelectedStudies + IsSelected |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
