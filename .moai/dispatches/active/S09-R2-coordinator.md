# DISPATCH: S09-R2 — Coordinator

Sprint: S09 | Round: 2 | Team: Coordinator
Updated: 2026-04-14

---

## Context

S09-R1 Detector DI 조건부등록 통합테스트 완료. 통합테스트 11개에서 주요 화면 통합테스트 보완 필요.

---

## Tasks

### Task 1: WorkflowViewModel 통합테스트 추가 (P1)

WorkflowView (Acquisition) 화면 통합테스트:
- [ ] 워크플로우 상태 전환 통합테스트
- [ ] 환자 선택 → 프로토콜 → 촬영 시나리오
- [ ] IWorkflowEngine 인터페이스 계약 검증

**위치**: `tests.integration/`

### Task 2: MergeViewModel 통합테스트 추가 (P2)

MergeView 화면 통합테스트:
- [ ] 영상 병합 시나리오 통합테스트
- [ ] IStudyItem 인터페이스 계약 검증

**위치**: `tests.integration/`

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Workflow 통합테스트 (P1) | NOT_STARTED | | |
| Task 2: Merge 통합테스트 (P2) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |

---

## Self-Verification Checklist

- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` 전원 통과
- [ ] 통합테스트 범위 내만 수정
