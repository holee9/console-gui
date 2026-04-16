# DISPATCH: S11-R1 — Team A

> **Sprint**: S11 | **Round**: 1 | **Date**: 2026-04-16
> **Team**: Team A (Infrastructure)
> **Priority**: P1 (Critical — Coverage Gap)

---

## Context

S10-R4 QA CONDITIONAL PASS. 가장 큰 커버리지 갭:
- **HnVue.Data: 50.0%** — 35% gap (가장 시급)
- **HnVue.Update: 88.9%** — 1.1% gap (Safety-Critical 90% 미달)
- 2개 테스트 실패 (EfUpdateRepository empty string validation)

---

## Tasks

### Task 1: HnVue.Data 집중 커버리지 개선 (P1) [HARD — 최우선]

**목표**: 50.0% → 85%+ (35% gap)

**핵심 타겟**:
1. `UserRepository`: 70% → 85%+
   - 누락 CRUD: AddAsync 중복, UpdateAsync 존재하지 않는 사용자, DeleteAsync 연쇄 삭제
   - 에지 케이스: null/empty username, 긴 username (>256 chars), 이미 존재하는 username

2. `StudyRepository`: 77.6% → 85%+
   - 검색/필터: GetStudiesByPatientId (빈 결과), GetStudiesByDateRange (날짜 경계), GetStudiesByModality (잘못된 modality)
   - 정렬/페이징: GetStudiesPaged (0개 결과, page index overflow)

3. `EfCdStudyRepository`: 68.4% → 85%+ (가장 큰 갭)
   - 다중 스터디: GetCdStudiesByStudyIds (빈 배열, null array, 매우 큰 배열)
   - CD 관련: CreateCdStudy (중복 CD study), UpdateCdStudy (존재하지 않는 ID)

4. `HnVueDbContextFactory`: 0% → 85%+
   - Mock/InMemory 테스트: CreateDbContext (연결 문자열 변형), EnsureCreated (이미 존재하는 DB)

**접근법**:
- InMemory SQLite 사용 (기존 패턴 준수)
- 에지 케이스 중심 (null, empty, overflow, 중복)
- 비동기 메서드 CancellationToken 테스트

### Task 2: HnVue.Update Safety-Critical 확보 (P1)

**목표**: 88.9% → 90%+ (1.1% gap)

**핵심 타겟**:
1. `EfUpdateRepository`: 엣지 케이스
   - 빈 문자열 검증: CheckForUpdate (empty version string), GetPackageInfo (null/empty package path)
   - 에러 핸들링: ApplyPackage (corrupted package file, insufficient disk space)

2. 테스트 실패 수정 (2건)
   - EfUpdateRepository empty string validation 테스트 수정

**접근법**:
- 빈 문자열/null 입력 시 예외 발생 검증
- 파일 시스템 에러 시나리오 Mock

### Task 3: IDLE CONFIRM (P3)

할 일 없으면 IDLE 보고.

---

## Acceptance Criteria

- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` 전체 통과
- [ ] HnVue.Data 커버리지 85%+ (50% → 85%)
- [ ] HnVue.Update 커버리지 90%+ (88.9% → 90%)
- [ ] EfUpdateRepository 테스트 실패 2건 해결
- [ ] 소유권 범위 내 파일만 수정

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Data 커버리지 (P1) | NOT_STARTED | - | 35% gap 해소 목표 |
| Task 2: Update 커버리지 (P1) | NOT_STARTED | - | Safety-Critical 90%+ |
| Task 3: IDLE CONFIRM (P3) | NOT_STARTED | - | - |

---

## Self-Verification Checklist

- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` PASS (all Team A modules)
- [ ] 커버리지 85%+ (Data), 90%+ (Update)
- [ ] 테스트 실패 0건
- [ ] DISPATCH Status 업데이트 완료
- [ ] `/clear` 실행 완료
