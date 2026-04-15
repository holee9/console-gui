# DISPATCH: S10-R4 — Team B

> **Sprint**: S10 | **Round**: 4 | **Date**: 2026-04-16
> **Team**: Team B (Medical Imaging)
> **Priority**: P2

---

## Context

S10-R3 QA CONDITIONAL PASS. HnVue.Dicom 커버리지 83.7% — 85% 게이트에 1.3% 부족.
DicomService (80.4%)과 MppsScu (80%) 집중 개선 필요.

---

## Tasks

### Task 1: HnVue.Dicom 커버리지 85% 달성 (P2)

**목표**: 83.7% → 85%+

필요한 테스트 (2-3개면 충분):
- `DicomService`: 80.4% → 85%+ (연결 해제, 타임아웃, 에러 응답 시나리오)
- `MppsScu`: 80% → 85%+ (N-CREATE/N-SET 흐름, 예외 처리)

**접근법**:
- 기존 테스트 패턴 참고
- Mock Dicom 연결 (실제 SCP 불필요)
- 에지 케이스: 연결 실패, 잘못된 태그, 빈 데이터셋

### Task 2: IDLE CONFIRM (P3)

할 일 없으면 IDLE 보고.

---

## Acceptance Criteria

- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` 전체 통과
- [ ] HnVue.Dicom 커버리지 85%+
- [ ] 소유권 범위 내 파일만 수정

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Dicom 커버리지 (P2) | NOT_STARTED | - | |
| Task 2: IDLE CONFIRM (P3) | NOT_STARTED | - | |

---

## Self-Verification Checklist

- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` PASS
- [ ] 커버리지 85%+ 달성
- [ ] DISPATCH Status 업데이트 완료
- [ ] `/clear` 실행 완료
