# Tasks: SPEC-COORDINATOR-001

## Task Breakdown

### Task 1: EfDoseRepository Implementation [DONE]
- File: `src/HnVue.Data/Repositories/EfDoseRepository.cs`
- Interface: IDoseRepository (HnVue.Dose)
- Entity: DoseRecordEntity
- DI: `services.AddScoped<IDoseRepository, EfDoseRepository>()`
- Tests: 2 integration tests
- Status: Implemented, DI registered

### Task 2: EfWorklistRepository Implementation [DONE]
- File: `src/HnVue.Data/Repositories/EfWorklistRepository.cs`
- Interface: IWorklistRepository (HnVue.PatientManagement)
- Entity: StudyEntity + PatientEntity
- DI: `services.AddScoped<IWorklistRepository, EfWorklistRepository>()`
- Tests: 2 integration tests
- Status: Implemented, DI registered

### Task 3: EfIncidentRepository Implementation [DONE]
- File: `src/HnVue.Data/Repositories/EfIncidentRepository.cs`
- Interface: IIncidentRepository (HnVue.Incident)
- Entity: IncidentEntity
- DI: `services.AddScoped<IIncidentRepository, EfIncidentRepository>()`
- Tests: 2 integration tests
- Status: Implemented, DI registered

### Task 4: EfUpdateRepository Implementation [DONE]
- File: `src/HnVue.Data/Repositories/EfUpdateRepository.cs`
- Interface: IUpdateRepository (HnVue.Update)
- Entity: UpdateHistoryEntity
- DI: `services.AddScoped<IUpdateRepository, EfUpdateRepository>()`
- Tests: 2 integration tests
- Status: Implemented, DI registered

### Task 5: EfSystemSettingsRepository Implementation [DONE]
- File: `src/HnVue.Data/Repositories/EfSystemSettingsRepository.cs`
- Interface: ISystemSettingsRepository (HnVue.SystemAdmin)
- Entity: SystemSettingsEntity
- DI: `services.AddScoped<ISystemSettingsRepository, EfSystemSettingsRepository>()`
- Tests: 2 integration tests
- Status: Implemented, DI registered

### Task 6: EfCdStudyRepository Implementation [DONE]
- File: `src/HnVue.Data/Repositories/EfCdStudyRepository.cs`
- Interface: IStudyRepository (HnVue.CDBurning)
- Entity: StudyEntity + ImageEntity
- DI: Registered in App.xaml.cs CD burning section
- Tests: 2 integration tests
- Status: Implemented, DI registered

### Task 7: App.xaml.cs DI Registration Replacement [DONE]
- Replace 6 NullRepository registrations with EF Core implementations
- Change AddSingleton → AddScoped
- Add SPEC-COORDINATOR-001 reference comments
- Status: All 6 replacements complete

### Task 8: Integration Test Suite [DONE]
- File: `tests.integration/HnVue.IntegrationTests/RepositoryIntegrationTests.cs`
- 13 repository tests using in-memory SQLite
- File: `tests.integration/HnVue.IntegrationTests/DiRegistrationIntegrationTests.cs`
- 6 DI resolution tests
- Status: All tests written, pending build verification

## Remaining Work

- [ ] Run `dotnet build` to verify 0 errors
- [ ] Run `dotnet test tests.integration/` to verify all tests pass
- [ ] Team A review of EfRepository implementations (S17)
