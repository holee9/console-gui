# DISPATCH: S04 R2 — Coordinator (Integration & DI)

Issued: 2026-04-11
Issued By: Main (MoAI Commander Center)
Sprint: S04 Round 2
SPEC: SPEC-COORDINATOR-001 (P0-Blocker)
Priority: P0-Blocker

## Objective

App.xaml.cs에 등록된 6개 NullRepository stub을 EF Core 기반 실제 구현체로 교체.
현재 DI 컨테이너에 Null 구현체가 등록되어 모든 Repository 호출이 no-op 처리됨.
전체 파이프라인 E2E 정상화의 핵심 의존성.

## SPEC Reference

`.moai/specs/SPEC-COORDINATOR-001/spec.md` — 반드시 전문 읽고 구현할 것.

## Tasks

### T1: EfDoseRepository 구현 (REQ-COORD-001)

**파일**: `src/HnVue.Data/Repositories/EfDoseRepository.cs` (신규)

- `HnVueDbContext` 생성자 주입
- `IDoseRepository` 전 메서드 구현 (CRUD + 집계)
- `App.xaml.cs:179` `NullDoseRepository` → `EfDoseRepository` 교체

### T2: EfWorklistRepository 구현 (REQ-COORD-002)

**파일**: `src/HnVue.Data/Repositories/EfWorklistRepository.cs` (신규)

- `IWorklistRepository` 전 메서드 구현
- `App.xaml.cs:184` `NullWorklistRepository` → `EfWorklistRepository` 교체

### T3: EfIncidentRepository 구현 (REQ-COORD-003)

**파일**: `src/HnVue.Data/Repositories/EfIncidentRepository.cs` (신규)

- `IIncidentRepository` 전 메서드 구현
- `App.xaml.cs` 해당 라인 교체

### T4: EfUpdateRepository 구현 (REQ-COORD-004)

**파일**: `src/HnVue.Data/Repositories/EfUpdateRepository.cs` (신규)

- `IUpdateRepository` 전 메서드 구현
- DI 등록 교체

### T5: EfSystemSettingsRepository 구현 (REQ-COORD-005)

**파일**: `src/HnVue.Data/Repositories/EfSystemSettingsRepository.cs` (신규)

- `ISystemSettingsRepository` 전 메서드 구현
- DI 등록 교체

### T6: EfCdStudyRepository 구현 (REQ-COORD-006)

**파일**: `src/HnVue.Data/Repositories/EfCdStudyRepository.cs` (신규)

- `HnVue.CDBurning.IStudyRepository` 전 메서드 구현
- DI 등록 교체

### T7: 통합 테스트 작성 (REQ-COORD-007)

**파일**: `tests.integration/HnVue.IntegrationTests/RepositoryIntegrationTests.cs` (신규)

최소 6개 테스트 (각 Repository별 1개 이상):
- in-memory SQLite DbContext 활용
- CRUD 동작 검증
- DI 컨테이너 통합 검증

## Implementation Notes

- 기존 스키마 활용 (새 Migration 불필요)
- Repository 인터페이스는 수정하지 않음
- `HnVueDbContext`의 `DbSet<T>` 확인 후 매핑
- async/await 패턴 필수, CancellationToken 파라미터 포함

## Build Verification [HARD]

```bash
dotnet build HnVue.sln --no-incremental
dotnet test HnVue.sln --filter "FullyQualifiedName~HnVue.IntegrationTests" --no-build
```

**게이트**: 0 에러, 통합 테스트 6개+ 모두 통과

## Git Protocol [HARD]

1. `git add` 관련 파일만
2. `git commit -m "feat(coordinator): SPEC-COORDINATOR-001 Null Repository 6개 EF Core 교체"`
3. `git push origin team/coordinator`
4. PR 생성
5. PR URL을 DISPATCH.md Status에 기록

## Status

- **State**: PENDING
- **Assigned**: Coordinator
- **PR**: (작성 후 기록)
- **Started**: (시작 시 기록)
- **Completed**: (완료 시 기록)
