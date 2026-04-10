# Acceptance Criteria — SPEC-TEAMB-FIX-001

## AC-001: Dicom 모듈 커버리지 80% 달성

### Given/When/Then

**Scenario 1: MppsScu SendInProgressAsync**
- Given MppsScu 인스턴스가 유효한 설정으로 생성되고
- When SendInProgressAsync를 유효한 파라미터로 호출하면
- Then MppsUid 문자열을 포함한 Result.Success를 반환

**Scenario 2: MppsScu SendInProgressAsync - 연결 실패**
- Given DICOM 호스트 설정이 누락되고
- When SendInProgressAsync를 호출하면
- Then ErrorCode.DicomConnectionFailed를 포함한 Result.Failure를 반환

**Scenario 3: DicomStoreScu SendAsync**
- Given DicomStoreScu 인스턴스가 유효한 설정으로 생성되고
- When SendAsync를 유효한 파일 경로와 파라미터로 호출하면
- Then Result.Success를 반환

**Scenario 4: DicomOutbox 재시도 정책**
- Given DicomOutbox가 지수 백오프 재시도 정책으로 구성되고
- When 첫 번째 전송이 실패하면
- Then 재시도 후 성공 시 Result.Success를 반환

### Coverage Gate

- Dicom line coverage >= 80%
- MppsScu coverage >= 70%
- DicomStoreScu coverage >= 70%
- All tests passing (0 failures)

## AC-002: IncidentRepository null guard

### Given/When/Then

**Scenario 5: UpdateAsync null record**
- Given IncidentRepository 인스턴스가 존재하고
- When UpdateAsync를 null record로 호출하면
- Then ArgumentNullException을 발생

**Scenario 6: AddAsync null record**
- Given IncidentRepository 인스턴스가 존재하고
- When AddAsync를 null record로 호출하면
- Then ArgumentNullException을 발생

## AC-003: WorkflowEngine null guard

### Given/When/Then

**Scenario 7: PrepareExposureAsync null parameters**
- Given WorkflowEngine 인스턴스가 존재하고
- When PrepareExposureAsync를 null parameters로 호출하면
- Then ArgumentNullException을 발생

## AC-004: Build & Test 통과

### Gate Criteria

- `dotnet build HnVue.sln --configuration Release` 성공 (0 errors)
- `dotnet test HnVue.sln --configuration Release` 성공 (0 failures)
- 기존 테스트 회귀 없음
