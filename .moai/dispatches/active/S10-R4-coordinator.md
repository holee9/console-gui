# DISPATCH: S10-R4 — Coordinator

> **Sprint**: S10 | **Round**: 4 | **Date**: 2026-04-16
> **Team**: Coordinator (Integration)
> **Priority**: P2

---

## Context

S10-R3 QA CONDITIONAL PASS. HnVue.UI 전체 67.8%. Views code-behind 제외하면 ~88%.
하지만 일부 컴포넌트/서비스 낮음:
- AcquisitionPreview: 77% (Medical 컴포넌트 — Coordinator 영역 아님, Team B가 담당)
- ToastItem: 68.9% (UI 서비스 — Coordinator 영역)

---

## Tasks

### Task 1: ToastItem 커버리지 개선 (P2)

**목표**: 68.9% → 85%+

- Toast 표시/숨김 타이머 테스트
- Toast 종류(Success/Error/Warning/Info) 분기 테스트
- DataContext 바인딩 테스트

### Task 2: IDLE CONFIRM (P3)

할 일 없으면 IDLE 보고.

---

## Acceptance Criteria

- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` 전체 통과
- [ ] 소유권 범위 내 파일만 수정

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: ToastItem 커버리지 (P2) | NOT_STARTED | - | |
| Task 2: IDLE CONFIRM (P3) | NOT_STARTED | - | |

---

## Self-Verification Checklist

- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` PASS
- [ ] DISPATCH Status 업데이트 완료
- [ ] `/clear` 실행 완료
