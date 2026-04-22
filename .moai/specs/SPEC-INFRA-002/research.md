---
id: SPEC-INFRA-002-research
version: 1.0.0
status: completed
created: "2026-04-22"
author: team-a
---

# SPEC-INFRA-002 Research: PHI AES-256-GCM 암호화 구현 조사

## 1. 기존 NullPhiEncryptionService 참조 경로

### 위치
- `HnVue.Common.Abstractions.IPhiEncryptionService` — 인터페이스 정의
- `HnVue.Security.PhiEncryptionService` — 실제 AES-256-GCM 구현 (Security 모듈)
- `HnVue.Data.Services.AesGcmPhiEncryptionService` — Data 모듈 구현 (HKDF 포함)

### DI 등록 위치
- `HnVue.Data.Extensions.ServiceCollectionExtensions.AddHnVueData()` — AesGcmPhiEncryptionService를 Singleton으로 등록
- SQLCipher Password에서 HKDF로 키 파생, 없으면 Random 키 (개발용)

### NullPhiEncryptionService 상태
- NullPhiEncryptionService는 더 이상 DI에 등록되지 않음
- App.xaml.cs에 IPhiEncryptionService 참조 없음 — Data 모듈 ServiceCollectionExtensions에서 처리

## 2. HKDF 적용 지점

### 현재 구현
- `AesGcmPhiEncryptionService.FromSqlCipherKey(string)` — 정적 팩토리 메서드
- `AesGcmPhiEncryptionService.DeriveKey(string)` / `DeriveKey(byte[])` — 키 파생 유틸리티
- Salt: `"HnVue-PHI-Encryption-v1"` (UTF-8 bytes)
- Info: `"HnVue-PHI-AES256GCM-v1"` (UTF-8 bytes)
- HashAlgorithm: SHA256
- Output: 32 bytes (AES-256)

### HKDF 사용 방식
```csharp
HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, 32, HkdfSalt, HkdfInfo);
```

## 3. SQLCipher 키 접근 경로

### 연결 문자열에서 Password 추출
- `ServiceCollectionExtensions.ExtractPasswordFromConnectionString(connectionString)` 사용
- SQLCipher 연결 문자열의 `Password=` 파라미터에서 추출

### 키 파생 흐름
```
SQLCipher Password (string)
    → ExtractPasswordFromConnectionString()
    → FromSqlCipherKey(password)
    → HKDF.DeriveKey(SHA256, UTF8(password), 32, salt, info)
    → AesGcmPhiEncryptionService(derivedKey)
```

## 4. PatientEntity PHI 필드

### 필드 목록 (HnVueDbContext OnModelCreating에서 확인)
| 필드 | 타입 | Converter | PHI 여부 |
|------|------|-----------|---------|
| Name | string | PhiEncryptedValueConverter | YES |
| DateOfBirth | DateTime? | NullablePhiEncryptedValueConverter | YES |
| PatientId | string | PhiEncryptedValueConverter | YES |

### Value Converter 적용
- `PhiEncryptedValueConverter` — non-nullable string 필드용
- `NullablePhiEncryptedValueConverter` — nullable string/DateTime 필드용
- `OnModelCreating`에서 `HasConversion()`으로 등록

## 5. 기존 테스트 현황

### 단위 테스트 (14개)
- `AesGcmPhiEncryptionServiceTests` — 14개 테스트 전원 통과
  - Round-trip: 4개 (short, medium, long, single char)
  - Security: 2개 (tamper detection, nonce randomness)
  - Edge case: 2개 (empty string encrypt/decrypt)
  - Key derivation: 3개 (deterministic, different input, factory)
  - Validation: 2개 (null key, wrong length)
  - Format: 1개 (output format verification)

### 통합 테스트
- `PhiEncryptionIntegrationTests` — tests.integration에 존재
- `StrideSecurityIntegrationTests` — stride 보안 통합 테스트

## 6. 결론

SPEC-INFRA-002의 핵심 구현은 이미 완료됨:
- REQ-PHI-001: AesGcmPhiEncryptionService 구현 ✅
- REQ-PHI-002: HKDF 키 파생 ✅
- REQ-PHI-003: PatientEntity PHI 필드 Value Converter ✅
- REQ-PHI-004: DI 등록 (NullPhiEncryptionService 제거) ✅
- REQ-PHI-005: 단위 테스트 14개 (기준 10개 초과) ✅

남은 작업: Planning 산출물 4개 파일 작성 (사후 문서화).
