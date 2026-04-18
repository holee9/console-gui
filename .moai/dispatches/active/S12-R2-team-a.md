# DISPATCH: S12-R2 — Team A

> **Sprint**: S12 | **Round**: 2 | **Date**: 2026-04-18
> **Team**: Team A (Infrastructure & Foundation)
> **Priority**: P1

---

## Context

S12-R1 QA 결과: **CONDITIONAL PASS** (4013/4017)

3개 실패 — HnVue.Data.Tests:
1. `EfUpdateRepositoryTests.RecordInstallationAsync_EmptyFromVersion_ThrowsArgumentNullException` (line 303)
2. `EfUpdateRepositoryTests.RecordInstallationAsync_EmptyToVersion_ThrowsArgumentNullException` (line 316)
3. `DataCoverageBoostV2Tests.UserRepository_AddAsync_DuplicateUsername_ReturnsAlreadyExists` (line 434)

Update 커버리지: **89.9%** (90% 기준 0.1% 미달)

---

## Tasks

### Task 1: HnVue.Data.Tests 실패 3개 수정 (P1)

**목표**: 0 실패

**구현 항목**:
1. `EfUpdateRepositoryTests` line 303/316 — RecordInstallationAsync 빈 문자열 테스트 수정
2. `DataCoverageBoostV2Tests` line 434 — UserRepository 중복 유저명 테스트 수정
3. `dotnet test tests/HnVue.Data.Tests/` → 0 실패 확인

### Task 2: Update 커버리지 90%+ 달성 (P1)

**목표**: HnVue.Update 라인 커버리지 90%+

**구현 항목**:
1. 현재 89.9% — 미테스트 경로 식별
2. 추가 테스트 작성 (tests/HnVue.Update.Tests/)
3. `dotnet test tests/HnVue.Update.Tests/` → 커버리지 확인

---

## Acceptance Criteria

- [ ] HnVue.Data.Tests 0 실패
- [ ] HnVue.Update 커버리지 90%+ 달성
- [ ] 전체 Team A 모듈 빌드 0 에러
- [ ] 소유권 준수 (HnVue.Common, Data, Security, SystemAdmin, Update, tests/)

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Data.Tests 실패 수정 (P1) | IN_PROGRESS | - | 분석 시작 |
| Task 2: Update 커버리지 90%+ (P1) | IN_PROGRESS | - | 분석 시작 |

---

## Self-Verification Checklist

- [ ] `dotnet test tests/HnVue.Data.Tests/` 0 실패
- [ ] `dotnet test tests/HnVue.Update.Tests/` 커버리지 90%+
- [ ] `dotnet build HnVue.sln` 0 에러
- [ ] DISPATCH Status COMPLETED + 빌드 증거
- [ ] `/clear` 실행 완료
