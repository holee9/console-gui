# DISPATCH: S12-R2 — QA

> **Sprint**: S12 | **Round**: 2 | **Date**: 2026-04-18
> **Team**: QA (Quality Assurance)
> **Priority**: P1

---

## Context

S12-R1 QA: CONDITIONAL PASS (4013/4017, 99.93%)

Team A가 Data.Tests 3개 실패 수정 + Update 90%+ 달성.
Team B가 Dicom 커버리지 개선 후 재검증.

---

## Tasks

### Task 1: 전체 테스트 재실행 (P1)

**목표**: 0 실패

### Task 2: 커버리지 통합 리포트 (P1)

**목표**: 전체 평균 85%+ 또는 모듈별 최소 85% 확인

**구현 항목**:
1. Coverlet + Cobertura XML 생성
2. 모듈별 커버리지 요약
3. `TestReports/S12-R2-QA-Report.md` 작성

### Task 3: PASS 판정 (P1)

**기준**:
- 전체 테스트 0 실패
- Safety-Critical 90%+ (Dose, Incident, Security, Update)
- 전체 평균 85%+

---

## Acceptance Criteria

- [ ] 전체 테스트 PASS (0 실패)
- [ ] S12-R1 3개 실패 (Data.Tests) 해소 확인
- [ ] Update 커버리지 90%+ 확인
- [ ] Dicom 커버리지 개선 확인
- [ ] PASS 또는 CONDITIONAL PASS 판정
- [ ] 소유권 준수 (TestReports/, scripts/qa/)

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: 전체 테스트 재실행 (P1) | IN_PROGRESS | - | main 기준 실행 완료: 4014/4017 PASS, Data.Tests 3 FAIL (S12-R1과 동일). Team A 수정 미반영 |
| Task 2: 커버리지 리포트 (P1) | BLOCKED | - | Team A(Update 90%+) / Team B(Dicom 개선) 머지 후 실행 필요 |
| Task 3: PASS 판정 (P1) | BLOCKED | - | Task 1/2 완료 후 판정 가능 |

---

## Self-Verification Checklist

- [ ] 전체 테스트 0 실패
- [ ] 커버리지 리포트 작성
- [ ] PASS 판정
- [ ] DISPATCH Status COMPLETED
- [ ] `/clear` 실행 완료
