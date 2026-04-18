# DISPATCH: S12-R1 — Design

> **Sprint**: S12 | **Round**: 1 | **Date**: 2026-04-18
> **Team**: Design (Pure UI)
> **Priority**: P1

---

## Context

S11-R2 완료. S12-R1 목표: PASS 전환.

UI 커버리지 개선 필요. Coordinator 팀과 협업.

---

## Tasks

### Task 1: UI 커버리지 개선 - Tests (P1)

**대상**: `tests/HnVue.UI.Tests/`

**목표**: UI 테스트 커버리지 85%+ 달성

**구현 항목**:
1. ✅ ViewCodeBehindTests.cs 생성 (11개 View 테스트)
2. ✅ UIComponentTests 확장 (7개 Medical Component 테스트)
3. ✅ WorkflowView 테스트 추가 (12번째 View 테스트)
4. ✅ ViewModelBaseTests.cs 생성 (18개 테스트)
5. ℹ️ 최신 커버리지: **78.66%** (목표 85%+)
6. ℹ️ HnVue.UI 모듈: **80.23%** (Coordinator refactoring 효과)
7. ℹ️ 테스트 실행: 768개 통과 (748개 → +20개)
8. ✅ Coordinator 협업: WpfApplicationFixture refactoring 적용 완료

**최신 변경**:
- WorkflowView_DefaultConstructor_InitializesComponent 추가
- ViewModelBase 테스트 클래스 생성 (INotifyPropertyChanged, SetProperty 검증)
- 총 20개 새 테스트 추가

### Task 2: DesignTime TODO 정리 (P2)

**대상**: `src/HnVue.UI/DesignTime/`

**구현 항목**:
1. ✅ TODO 주석 확인 결과: 없음 (이미 정리됨)
2. ✅ Mock 데이터 확인: 정리됨
3. ✅ 디자인 토큰 일관성 검증 완료

### Task 3: Coordinator 협업 (P1)

**목표**: Coordinator가 지정하는 UI 커버리지 개선 작업

**구현 항목**:
1. Coordinator 요청 View/ViewModel 테스트 작성 대기
2. 커버리지 개선 후 재테스트
3. DesignTime Mock 데이터 업데이트

---

## Acceptance Criteria

- [x] UI.Tests 커버리지 측정 완료 (74.21%)
- [ ] UI.Tests 커버리지 85%+ 달성 (74.21%, 여전히 미달)
- [x] DesignTime TODO 정리 완료
- [x] 추가 테스트 작성 완료 (WorkflowView, ViewModelBase)
- [ ] Coordinator 협업 (추가 작업 필요)
- [x] 소유권 준수 (UI만)

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: UI 커버리지 개선 (P1) | IN_PROGRESS | - | +20 테스트 추가, 커버리지 74.21% (목표 미달) |
| Task 2: DesignTime TODO 정리 (P2) | COMPLETED | - | TODO 없음, 정리됨 확인 |
| Task 3: Coordinator 협업 (P1) | NOT_STARTED | - | 커버리지 85%+ 달성을 위한 추가 작업 필요 |

---

## Self-Verification Checklist

- [x] 소유권 준수 (UI만)
- [ ] 커버리지 85%+ 개선 (78.66%, HnVue.UI 모듈 80.23%, 진행 중)
- [x] Coordinator 협업 완료 (WpfApplicationFixture refactoring)
- [x] DISPATCH Status 업데이트
- [ ] `/clear` 실행 완료

---

## 빌드 증거

```
Build succeeded.
    Warning(s): 5181
    Error(s): 0

Test Run Successful.
Total tests: 768
    Passed: 768
    Failed: 0
```

## 커버리지 증거

```
line-rate="0.7866" (78.66%)
lines-covered="2009"
lines-valid="2554"
branches-covered="330"
branches-valid="489"

HnVue.UI 모듈:
line-rate="0.8023" (80.23%)
```
