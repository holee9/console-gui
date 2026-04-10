# Implementation Plan — SPEC-TEAMB-FIX-001

## Task Decomposition

### Phase 1: Dicom 테스트 커버리지 (최우선, 43% → 80%)

#### Task 1.1: MppsScu 테스트 (P2-High)
- **File**: `tests/HnVue.Dicom.Tests/MppsScuTests.cs`
- **Pattern**: NSubstitute mock for DicomClient
- **Scope**: SendInProgressAsync, SendCompletedAsync
- **Test Count**: ~10-12개

#### Task 1.2: DicomStoreScu 테스트
- **File**: `tests/HnVue.Dicom.Tests/DicomStoreScuTests.cs`
- **Pattern**: NSubstitute mock for DicomClient
- **Scope**: SendAsync 성공/실패, 체크섬 검증
- **Test Count**: ~8-10개

#### Task 1.3: DicomOutbox/DicomService 테스트 보강
- **Files**: 기존 테스트 파일 확장
- **Scope**: 엣지 케이스, 재시도 정책, 동시성
- **Test Count**: ~10-15개 추가

### Phase 2: 방어적 개선

#### Task 2.1: IncidentRepository null guard
- **File**: `src/HnVue.Incident/IncidentRepository.cs`
- **Scope**: UpdateAsync, AddAsync null guard
- **Test**: 해당 케이스 테스트 추가

#### Task 2.2: WorkflowEngine null guard
- **File**: `src/HnVue.Workflow/WorkflowEngine.cs`
- **Scope**: 공개 메서드 null guard
- **Test**: 해당 케이스 테스트 추가

### Phase 3: Build Verification

- `dotnet build HnVue.sln --configuration Release`
- `dotnet test HnVue.sln --configuration Release --no-build`

## Technology Stack

- **Framework**: xUnit 2.x
- **Mocking**: NSubstitute
- **Assertions**: FluentAssertions
- **Target**: .NET 8

## Risk Analysis

| Risk | Impact | Mitigation |
|------|--------|------------|
| DicomClient 정적 팩토리 mock 불가 | MppsScu/DicomService 테스트 제한 | 인터페이스 래핑 또는 내부 메서드 간접 테스트 |
| fo-dicom 응답 시뮬레이션 | DicomService 테스트 어려움 | Dataset 직접 생성으로 간접 테스트 |
| 기존 테스트 호환성 | 확장 시 기존 테스트 손상 가능 | 기존 테스트 먼저 실행 후 확장 |
