# DISPATCH: S11-R2 — Coordinator

> **Sprint**: S11 | **Round**: 2 | **Date**: 2026-04-17
> **Team**: Coordinator (Integration)
> **Priority**: P2

---

## Context

S11-R1 종료. UI.Contracts와 ViewModel 간 연동 개선 필요.

---

## Tasks

### Task 1: ISettingsViewModel 인터페이스 추가 (P2)

**목표**: SettingsView에 필요한 ViewModel 인터페이스 정의

**구현 항목**:
1. `ISettingsViewModel` 인터페이스 정의 (UI.Contracts)
2. 필요한 속성/커맨드 정의
3. Design Team과 협의

### Task 2: DI 등록 검증 (P3)

**파일**: `src/HnVue.App/App.xaml.cs`

**목표**: 모든 서비스 DI 등록 확인

**구현 항목**:
1. 누락된 서비스 등록 확인
2. 수명 주기 검증

---

## Acceptance Criteria

- [ ] ISettingsViewModel 인터페이스 정의 완료
- [ ] DI 등록 검증 완료
- [ ] 소유권 준수 (UI.Contracts, UI.ViewModels, App만)

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: ISettingsViewModel (P2) | NOT_STARTED | - | - |
| Task 2: DI 등록 검증 (P3) | NOT_STARTED | - | - |

---

## Self-Verification Checklist

- [ ] 소유권 준수 (UI.Contracts, UI.ViewModels, App만)
- [ ] 인터페이스 정의 완료
- [ ] DI 등록 확인
- [ ] DISPATCH Status 업데이트
- [ ] `/clear` 실행 완료
