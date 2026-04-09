# Acceptance Criteria — SPEC-TEAMB-COV-001

## AC-001: Detector 모듈 커버리지 85%+

### Given: Detector 모듈의 OwnDetectorAdapter/Config/VendorAdapterTemplate이 0% 커버리지인 상태에서
### When: OwnDetectorAdapterTests, OwnDetectorConfigTests, VendorAdapterTemplateTests 테스트 파일이 작성되면
### Then: Detector 모듈 line coverage가 85% 이상이어야 한다

**검증 방법**:
```bash
dotnet test tests/HnVue.Detector.Tests --configuration Release --collect:"XPlat Code Coverage"
```

**하위 기준**:
- [ ] OwnDetectorAdapter: 70%+ line coverage
- [ ] OwnDetectorConfig: 70%+ line coverage
- [ ] VendorAdapterTemplate: 70%+ line coverage
- [ ] DetectorSimulator: 100% 유지
- [ ] 전체 모듈: 85%+

### Edge Cases:
- Disposed adapter에서 메서드 호출 시 ObjectDisposedException
- 잘못된 상태에서 ArmAsync 호출 시 실패
- CancellationToken 취소 시 올바른 동작

## AC-002: Dose 모듈 커버리지 90%+ (Safety-Critical)

### Given: Dose 모듈의 DoseRepository가 0% 커버리지이고 DoseService는 100%인 상태에서
### When: DoseRepositoryTests 테스트 파일이 작성되면
### Then: Dose 모듈 line AND branch coverage 모두 90% 이상이어야 한다

**HARD GATE**: DOC-012 안전성 기준 — branch coverage 90%+

**검증 방법**:
```bash
dotnet test tests/HnVue.Dose.Tests --configuration Release --collect:"XPlat Code Coverage"
```

**하위 기준**:
- [ ] DoseRepository: 80%+ line coverage
- [ ] DoseService: 100% 유지
- [ ] 전체 모듈 line: 90%+
- [ ] 전체 모듈 branch: 90%+
- [ ] 인터록 4-level 전 분기 경로 커버

### Edge Cases:
- Null 파라미터 → ArgumentNullException
- DB 오류 → Result.Failure(ErrorCode.DatabaseError)
- 날짜 범위 경계값 (같은 날 from/until)
- CancellationToken 취소

## AC-003: Dicom 모듈 커버리지 80%+

### Given: Dicom 모듈의 MppsScu가 0%, DicomOutbox 62.5%, DicomService 69.3%인 상태에서
### When: MppsScuTests 생성 + DicomOutboxTests/DicomServiceTests 확장되면
### Then: Dicom 모듈 line coverage가 80% 이상이어야 한다

**검증 방법**:
```bash
dotnet test tests/HnVue.Dicom.Tests --configuration Release --collect:"XPlat Code Coverage"
```

**하위 기준**:
- [ ] MppsScu: 60%+ line coverage
- [ ] DicomOutbox: 80%+ line coverage
- [ ] DicomService: 80%+ line coverage
- [ ] 전체 모듈: 80%+

### Edge Cases:
- 네트워크 예외 → ErrorCode.DicomConnectionFailed
- DICOM 비성공 응답 상태 처리
- 빈 워크리스트 응답
- Null 필드가 있는 Dataset 매핑

## AC-004: PatientManagement 모듈 커버리지 80%+

### Given: PatientManagement 모듈의 WorklistRepository가 0%인 상태에서
### When: WorklistRepositoryTests 테스트 파일이 작성되면
### Then: PatientManagement 모듈 line coverage가 80% 이상이어야 한다

**검증 방법**:
```bash
dotnet test tests/HnVue.PatientManagement.Tests --configuration Release --collect:"XPlat Code Coverage"
```

**하위 기준**:
- [ ] WorklistRepository: 70%+ line coverage
- [ ] PatientService: 100% 유지
- [ ] WorklistService: 100% 유지
- [ ] 전체 모듈: 80%+

### Edge Cases:
- DICOM 서비스 실패 → 빈 목록 (advisory)
- 빈 워크리스트 응답
- Null 설정 파라미터

## AC-005: 전체 빌드 + 테스트 통과

### Given: 모든 테스트 파일이 작성된 상태에서
### When: 전체 솔루션 빌드 및 테스트를 실행하면
### Then: 빌드 에러 0건, 테스트 실패 0건이어야 한다

**검증 방법**:
```bash
dotnet build HnVue.sln --configuration Release
dotnet test HnVue.sln --configuration Release --no-build
```

**하위 기준**:
- [ ] 빌드: 0 errors, 0 warnings (Release)
- [ ] 테스트: 전체 통과 (기존 + 신규)
- [ ] 기존 테스트 회귀 없음
