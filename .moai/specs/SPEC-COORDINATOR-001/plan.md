# Plan: SPEC-COORDINATOR-001

## Implementation Order (Dependency-Based)

All 6 repositories are already implemented. This plan documents the order and verifies completeness.

### Phase 1: Data Layer (Team A co-owns — Coordinator drafts)

1. **EfDoseRepository** — No upstream dependency. Depends on DoseRecordEntity.
2. **EfWorklistRepository** — Depends on StudyEntity + PatientEntity.
3. **EfIncidentRepository** — No upstream dependency. Depends on IncidentEntity.
4. **EfUpdateRepository** — No upstream dependency. Depends on UpdateHistoryEntity.
5. **EfSystemSettingsRepository** — No upstream dependency. Depends on SystemSettingsEntity.
6. **EfCdStudyRepository** — Depends on StudyEntity + ImageEntity.

### Phase 2: DI Registration (Coordinator)

Replace 6 NullRepository registrations in App.xaml.cs with EF Core implementations.
- Change lifetime from `AddSingleton` to `AddScoped` (match DbContext scope).
- Add comments referencing SPEC-COORDINATOR-001.

### Phase 3: Integration Tests (Coordinator)

Write integration tests in `tests.integration/HnVue.IntegrationTests/RepositoryIntegrationTests.cs`:
- Each repository gets minimum 2 tests (happy path + edge case).
- Use in-memory SQLite with `EnsureCreated()`.
- No mocks — test real EF Core behavior.

### DI Replacement Strategy

```
Before: services.AddSingleton<IDoseRepository, NullDoseRepository>()
After:  services.AddScoped<IDoseRepository, EfDoseRepository>()
```

Rationale for Scoped:
- HnVueDbContext is registered as Scoped
- Repository depends on DbContext via constructor injection
- Scoped ensures same DbContext instance per request scope
- Singleton would cause DbContext disposal issues

### Risk Mitigation

1. **EF Core in HnVue.Data is Team A domain** — Coordinator writes draft implementations, Team A reviews in S17
2. **In-memory SQLite != SQLCipher** — Integration tests validate logic, not encryption
3. **Thread safety** — Scoped lifetime per request avoids concurrent DbContext access
