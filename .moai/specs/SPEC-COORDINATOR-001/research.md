# Research: SPEC-COORDINATOR-001

## Current State (2026-04-22)

### DI Registration (App.xaml.cs)

All 6 NullRepository stubs have been replaced with EF Core implementations:

| Line | Interface | Implementation | Lifetime |
|------|-----------|---------------|----------|
| 178 | IDoseRepository | EfDoseRepository | Scoped |
| 183 | IWorklistRepository | EfWorklistRepository | Scoped |
| 189 | IIncidentRepository | EfIncidentRepository | Scoped |
| 194 | IUpdateRepository | EfUpdateRepository | Scoped |
| 202 | ISystemSettingsRepository | EfSystemSettingsRepository | Scoped |
| 208 | IStudyRepository (CDBurning) | EfCdStudyRepository | Scoped |

All registrations use `AddScoped` (correct for DbContext-scoped lifetime).

### EfRepository Implementations (HnVue.Data/Repositories/)

All 6 repositories exist and accept HnVueDbContext via constructor injection:

- `EfDoseRepository.cs` — implements IDoseRepository
- `EfWorklistRepository.cs` — implements IWorklistRepository
- `EfIncidentRepository.cs` — implements IIncidentRepository
- `EfUpdateRepository.cs` — implements IUpdateRepository
- `EfSystemSettingsRepository.cs` — implements ISystemSettingsRepository
- `EfCdStudyRepository.cs` — implements IStudyRepository (CDBurning)

### Integration Tests (tests.integration/HnVue.IntegrationTests/)

**RepositoryIntegrationTests.cs** (469 lines, 13 tests):
- EfDoseRepository: 2 tests (SaveAndGetByStudy, GetByPatient)
- EfWorklistRepository: 2 tests (QueryToday, QueryToday_NoStudies)
- EfIncidentRepository: 2 tests (SaveAndGetBySeverity, Resolve_UpdatesRecord)
- EfUpdateRepository: 2 tests (CheckForUpdate, CheckForUpdate_NoHistory)
- EfSystemSettingsRepository: 2 tests (Get_ReturnsDefaults, SaveAndGet_RoundTrips)
- EfCdStudyRepository (StudyRepository): 2 tests (GetFilesForStudy, GetFilesForStudy_NoImages)

**DiRegistrationIntegrationTests.cs** (275 lines, 6 tests):
- DI_AllViewModels_ResolveSuccessfully
- DI_StrideSecurityServices_ResolveSuccessfully
- DI_CoreSecurityServices_ResolveSuccessfully
- DI_DomainServices_ResolveSuccessfully
- DI_NavigationService_ResolvesSuccessfully
- DI_AddPatientProcedureViewModel_ReceivesSecurityContext

### DbContext (HnVue.Data/HnVueDbContext.cs)

Uses SQLite with SQLCipher encryption. Integration tests use in-memory SQLite (no encryption required for test scenarios).

### Key Entities

- DoseRecordEntity → DoseRecord (via EntityMapper)
- StudyEntity → Worklist/Study models
- IncidentEntity → IncidentRecord
- UpdateHistoryEntity → Update version info
- SystemSettingsEntity → SystemSettings
- ImageEntity → Study file paths (CD burning)

### Gaps Identified

1. No `dotnet build` / `dotnet test` verification on current codebase state
2. SPEC planning artifacts (research/plan/acceptance/tasks) missing
3. Repository implementations are in HnVue.Data (Team A domain) — Coordinator wrote drafts, Team A reviews in S17
