# DISPATCH: S10-R4 — Team A

> **Sprint**: S10 | **Round**: 4 | **Date**: 2026-04-16
> **Team**: Team A (Infrastructure)
> **Priority**: P1 (Critical — coverage gap)

---

## Context

S10-R3 QA CONDITIONAL PASS. 전체 커버리지 79.3% (목표 85%). Team A 소유 모듈 2개가 85% 미달:
- **HnVue.Data: 51.7%** — 가장 큰 커버리지 갭
- **HnVue.Update: 80.8%** — EfUpdateRepository 23.6%

---

## Tasks

### Task 1: HnVue.Data 커버리지 개선 (P1) [HARD — 최우선]

**목표**: 51.7% → 85%+

필요한 테스트:
- `UserRepository`: 현재 70% → 85%+ (CRUD 누락 테스트)
- `StudyRepository`: 현재 77.6% → 85%+ (검색/필터 테스트)
- `EfIncidentRepository`: 현재 77.7% → 85%+
- `EfDoseRepository`: 현재 80.7% → 85%+
- `EfCdStudyRepository`: 현재 68.4% → 85%+ (가장 큰 갭)
- `HnVueDbContextFactory`: 0% → Mock/InMemory 테스트
- `Migrations`: 0% → 제외 (EF Core 자동생성, 테스트 불필요)

**접근법**:
- InMemory SQLite 사용 (기존 패턴)
- 누락된 CRUD 메서드 테스트
- 에러 케이스 (예외, null, 빈 결과)
- 비동기 메서드 CancellationToken 테스트

### Task 2: HnVue.Update 커버리지 개선 (P1)

**목표**: 80.8% → 85%+

필요한 테스트:
- `EfUpdateRepository`: 23.6% → 85%+ (가장 큰 갭)
- `SWUpdateService`: 77.5% → 85%+

**접근법**:
- EfUpdateRepository CRUD 전체 커버
- SWUpdateService 업데이트 확인/다운로드/설치 흐름 테스트

### Task 3: IDLE CONFIRM (P3)

할 일 없으면 IDLE 보고.

---

## Acceptance Criteria

- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` 전체 통과
- [ ] HnVue.Data 커버리지 85%+
- [ ] HnVue.Update 커버리지 85%+
- [ ] 소유권 범위 내 파일만 수정

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Data 커버리지 (P1) | NOT_STARTED | - | |
| Task 2: Update 커버리지 (P1) | NOT_STARTED | - | |
| Task 3: IDLE CONFIRM (P3) | NOT_STARTED | - | |

---

## Self-Verification Checklist

- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` PASS
- [ ] 커버리지 85%+ 달성
- [ ] DISPATCH Status 업데이트 완료
- [ ] `/clear` 실행 완료
