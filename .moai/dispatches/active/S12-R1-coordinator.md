# DISPATCH: S12-R1 — Coordinator

> **Sprint**: S12 | **Round**: 1 | **Date**: 2026-04-18
> **Team**: Coordinator (Integration)
> **Priority**: P1

---

## Context

S11-R2 완료. S12-R1 목표: PASS 전환.

HnVue.UI 커버리지 개선 필요. Design 팀과 협업.

---

## Tasks

### Task 1: UI 커버리지 개선 - ViewModels (P1)

**대상**: `src/HnVue.UI.ViewModels/`

**목표**: ViewModel 테스트 커버리지 85%+ 달성

**구현 항목**:
1. ViewModels TODO/FIXME 정리 (8개 항목)
2. 누�된 테스트 케이스 추가
3. 경계 조건 테스트 강화

**TODO 항목**:
- `StudylistViewModel.cs:3` - TODO 주석 처리
- `SettingsViewModel.cs:2` - FIXME 주석 처리
- `MergeViewModel.cs:1` - TODO 주석 처리
- `MainViewModel.cs:2` - TODO 주석 처리
- `AddPatientProcedureViewModel.cs:1` - TODO 주석 처리

### Task 2: UI 커버리지 개선 - Design 협업 (P1)

**목표**: Design 팀과 협업하여 UI 커버리지 개선

**구현 항목**:
1. Design 팀에 커버리지 개선 필요 영역 전달
2. UI.Tests 테스트 작업 지원
3. 통합 테스트 검증

---

## Acceptance Criteria

- [ ] ViewModels TODO/FIXME 8개 정리 완료
- [ ] UI 커버리지 개선 완료
- [ ] Design 팀 협업 완료
- [ ] 소유권 준수 (UI.Contracts, UI.ViewModels, App만)

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: UI 커버리지 개선 (P1) | NOT_STARTED | - | - |
| Task 2: Design 협업 (P1) | NOT_STARTED | - | - |

---

## Self-Verification Checklist

- [ ] 소유권 준수 (UI.Contracts, UI.ViewModels, App만)
- [ ] 커버리지 개선 완료
- [ ] Design 팀 협업 완료
- [ ] DISPATCH Status 업데이트
- [ ] `/clear` 실행 완료
