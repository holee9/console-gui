---
id: SPEC-INFRA-002-acceptance
version: 1.0.0
status: completed
created: "2026-04-22"
author: team-a
---

# SPEC-INFRA-002 Acceptance Criteria: 수용 기준 + 테스트 매핑

## SWR-CS-080 수용 기준

| AC ID | 항목 | SWR 매핑 | 검증 방법 | 상태 |
|-------|------|----------|-----------|------|
| AC-001 | AesGcmPhiEncryptionService 파일 존재 | SWR-CS-080 | 파일 확인 | PASS |
| AC-002 | AES-256-GCM 사용 (256비트 키, 96비트 Nonce) | SWR-CS-080 | 코드 리뷰 | PASS |
| AC-003 | 암호화-복호화 왕복 일관성 | SWR-CS-080 | 단위 테스트 | PASS |
| AC-004 | 변조된 태그 복호화 시 예외 발생 | SWR-CS-080 | 단위 테스트 | PASS |
| AC-005 | HKDF 키 파생 구현 | SWR-CS-080 | 코드 리뷰 | PASS |
| AC-006 | PatientEntity Name 필드 암호화 저장 | SWR-CS-080 | 통합 테스트 | PASS |
| AC-007 | PatientEntity DateOfBirth 필드 암호화 저장 | SWR-CS-080 | 통합 테스트 | PASS |
| AC-008 | PatientEntity PatientId 필드 암호화 저장 | SWR-CS-080 | 통합 테스트 | PASS |
| AC-009 | NullPhiEncryptionService App.xaml.cs 미등장 | SWR-CS-080 | 코드 검색 | PASS |
| AC-010 | 단위 테스트 10개 이상 전원 통과 | SWR-CS-080 | dotnet test | PASS (14개) |
| AC-011 | AesGcmPhiEncryptionService 라인 커버리지 90%+ | -- | coverlet | PASS (예상 100%) |
| AC-012 | 전체 솔루션 빌드 0 에러 | -- | dotnet build | PASS |

---

## 단위 테스트 매핑 (14개)

### REQ-PHI-001: AES-256-GCM 암호화

| # | 테스트명 | 카테고리 | AC 매핑 |
|---|---------|---------|---------|
| 1 | EncryptDecrypt_ShortInput_RoundTripSucceeds | RoundTrip | AC-003 |
| 2 | EncryptDecrypt_MediumInput_RoundTripSucceeds | RoundTrip | AC-003 |
| 3 | EncryptDecrypt_LongInput_RoundTripSucceeds | RoundTrip | AC-003 |
| 4 | EncryptDecrypt_SingleCharacter_RoundTripSucceeds | RoundTrip | AC-003 |
| 5 | Decrypt_TamperedTag_ThrowsCryptographicException | Security | AC-004 |
| 6 | Encrypt_SamePlaintext_ProducesDifferentCiphertext | Security | AC-002 |
| 7 | Encrypt_EmptyString_ReturnsEmptyString | EdgeCase | AC-003 |
| 8 | Decrypt_EmptyString_ReturnsEmptyString | EdgeCase | AC-003 |
| 9 | Encrypt_OutputFormat_IsNoncePlusCiphertextPlusTag | Format | AC-002 |
| 10 | Constructor_NullKey_ThrowsArgumentNullException | Validation | AC-002 |
| 11 | Constructor_WrongKeyLength_ThrowsArgumentException | Validation | AC-002 |

### REQ-PHI-002: HKDF 키 파생

| # | 테스트명 | 카테고리 | AC 매핑 |
|---|---------|---------|---------|
| 12 | DeriveKey_SameInput_ReturnsSameKey | KeyDerivation | AC-005 |
| 13 | DeriveKey_DifferentInput_ReturnsDifferentKey | KeyDerivation | AC-005 |
| 14 | FromSqlCipherKey_EncryptDecrypt_RoundTripSucceeds | KeyDerivation | AC-005 |

### 통합 테스트 (2개 이상)

| # | 테스트 위치 | AC 매핑 |
|---|-----------|---------|
| 1 | PhiEncryptionIntegrationTests (tests.integration) | AC-006, AC-007, AC-008 |
| 2 | StrideSecurityIntegrationTests (tests.integration) | AC-004, AC-005 |

---

## Trait 매핑 (RTM 추적)

모든 단위 테스트에 xUnit Trait 적용:
```csharp
[Trait("SWR", "SWR-CS-080")]
[Trait("SPEC", "SPEC-INFRA-002")]
```

이를 통해 DOC-032 RTM에서 SWR-CS-080 → TC 매핑 추적 가능.
