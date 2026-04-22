# Acceptance Criteria: SPEC-COORDINATOR-001

## AC Matrix

| AC ID | Criterion | Verification Method | Status |
|-------|-----------|-------------------|--------|
| AC-001 | NullDoseRepository not in App.xaml.cs | Code search | PASS |
| AC-002 | NullWorklistRepository not in App.xaml.cs | Code search | PASS |
| AC-003 | NullIncidentRepository not in App.xaml.cs | Code search | PASS |
| AC-004 | NullUpdateRepository not in App.xaml.cs | Code search | PASS |
| AC-005 | NullSystemSettingsRepository not in App.xaml.cs | Code search | PASS |
| AC-006 | NullCdStudyRepository not in App.xaml.cs | Code search | PASS |
| AC-007 | 6 EfRepository classes exist in HnVue.Data/Repositories/ | File check | PASS |
| AC-008 | All DI registrations use AddScoped | Code review | PASS |
| AC-009 | RepositoryIntegrationTests — 13 tests | dotnet test | PENDING |
| AC-010 | DiRegistrationIntegrationTests — 6 tests | dotnet test | PENDING |
| AC-011 | Full solution build 0 errors | dotnet build | PENDING |

## Integration Test Coverage

### EfDoseRepository (2 tests)
- SaveAndGetByStudy_ReturnsRecord — SWR-DM-051
- GetByPatient_ReturnsRecordsForPatient

### EfWorklistRepository (2 tests)
- QueryToday_ReturnsTodaysStudies
- QueryToday_NoStudies_ReturnsEmptyList

### EfIncidentRepository (2 tests)
- SaveAndGetBySeverity_ReturnsRecords — SWR-IN-010
- Resolve_UpdatesRecord

### EfUpdateRepository (2 tests)
- CheckForUpdate_ReturnsLatestVersion
- CheckForUpdate_NoHistory_ReturnsNull

### EfSystemSettingsRepository (2 tests)
- Get_ReturnsDefaults
- SaveAndGet_RoundTrips

### EfCdStudyRepository (2 tests)
- GetFilesForStudy_ReturnsImagePaths
- GetFilesForStudy_NoImages_ReturnsEmptyList

### DI Registration (6 tests)
- DI_AllViewModels_ResolveSuccessfully — SWR-COORD-120
- DI_StrideSecurityServices_ResolveSuccessfully — SWR-COORD-130
- DI_CoreSecurityServices_ResolveSuccessfully — SWR-COORD-130
- DI_DomainServices_ResolveSuccessfully — SWR-COORD-120
- DI_NavigationService_ResolvesSuccessfully — SWR-COORD-120
- DI_AddPatientProcedureViewModel_ReceivesSecurityContext — SWR-COORD-120
