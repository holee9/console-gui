---
id: SPEC-INFRA-002-tasks
version: 1.0.0
status: completed
created: "2026-04-22"
author: team-a
---

# SPEC-INFRA-002 Tasks: RED-GREEN-REFACTOR 분할

## 구현 상태: ALL COMPLETED

모든 Task가 이전 Sprint에서 구현 완료. 사후 문서화.

---

## Phase 1: RED (실패하는 테스트 작성)

### Task 1.1: IPhiEncryptionService 인터페이스 확장 [COMPLETED]
- **파일**: `HnVue.Common/Abstractions/IPhiEncryptionService.cs`
- **작업**: VerifyTag, GenerateKey 메서드 추가
- **상태**: ✅ 완료

### Task 1.2: AesGcmPhiEncryptionService 스켈레톤 [COMPLETED]
- **파일**: `HnVue.Data/Services/AesGcmPhiEncryptionService.cs`
- **작업**: throw NotImplementedException 스켈레톤
- **상태**: ✅ 완료

### Task 1.3: 단위 테스트 작성 (RED) [COMPLETED]
- **파일**: `tests/HnVue.Data.Tests/Services/AesGcmPhiEncryptionServiceTests.cs`
- **작업**: 14개 실패 테스트 작성
- **상태**: ✅ 완료 (14개 테스트)

---

## Phase 2: GREEN (최소 구현으로 테스트 통과)

### Task 2.1: AES-256-GCM Encrypt/Decrypt 구현 [COMPLETED]
- **파일**: `AesGcmPhiEncryptionService.cs`
- **작업**: Encrypt, Decrypt, VerifyTag, GenerateKey 구현
- **상태**: ✅ 완료

### Task 2.2: HKDF 키 파생 구현 [COMPLETED]
- **파일**: `AesGcmPhiEncryptionService.cs`
- **작업**: FromSqlCipherKey, DeriveKey 팩토리 메서드
- **상태**: ✅ 완료

### Task 2.3: PhiEncryptedValueConverter 구현 [COMPLETED]
- **파일**: `HnVue.Data/Converters/PhiEncryptedValueConverter.cs`
- **파일**: `HnVue.Data/Converters/NullablePhiEncryptedValueConverter.cs`
- **작업**: EF Core Value Converter 구현
- **상태**: ✅ 완료

### Task 2.4: HnVueDbContext OnModelCreating 적용 [COMPLETED]
- **파일**: `HnVue.Data/HnVueDbContext.cs`
- **작업**: Name, DateOfBirth, PatientId 필드에 Value Converter 등록
- **상태**: ✅ 완료

### Task 2.5: DI 등록 [COMPLETED]
- **파일**: `HnVue.Data/Extensions/ServiceCollectionExtensions.cs`
- **작업**: AesGcmPhiEncryptionService Singleton 등록 + HKDF 파생
- **상태**: ✅ 완료

---

## Phase 3: REFACTOR (품질 개선)

### Task 3.1: 테스트 품질 개선 [COMPLETED]
- **작업**: Trait("SWR", "SWR-CS-080") 추가, 테스트명 규칙 준수
- **상태**: ✅ 완료

### Task 3.2: MX 태그 추가 [COMPLETED]
- **작업**: @MX:ANCHOR, @MX:WARN 태그 추가
- **상태**: ✅ 완료

---

## 파일 변경 요약

| 파일 | 변경 유형 | 모듈 |
|------|----------|------|
| AesGcmPhiEncryptionService.cs | 신규 | HnVue.Data |
| PhiEncryptedValueConverter.cs | 신규 | HnVue.Data |
| NullablePhiEncryptedValueConverter.cs | 신규 | HnVue.Data |
| IPhiEncryptionService.cs | 수정 | HnVue.Common |
| ServiceCollectionExtensions.cs | 수정 | HnVue.Data |
| HnVueDbContext.cs | 수정 | HnVue.Data |
| AesGcmPhiEncryptionServiceTests.cs | 신규 | HnVue.Data.Tests |
