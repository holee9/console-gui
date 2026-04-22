---
id: SPEC-INFRA-002-plan
version: 1.0.0
status: completed
created: "2026-04-22"
author: team-a
---

# SPEC-INFRA-002 Plan: AES-256-GCM 구현 전략

## 구현 상태: COMPLETED (사후 문서화)

본 Plan은 구현 완료 후 작성됨. 실제 구현은 research.md에 기록된 대로 이미 main에 반영됨.

---

## 구현 전략 (설계 의도)

### 1. AES-256-GCM 서비스 아키텍처

```
IPhiEncryptionService (HnVue.Common.Abstractions)
    ├── AesGcmPhiEncryptionService (HnVue.Data.Services) ← 주 구현체
    │   ├── FromSqlCipherKey() — HKDF 키 파생 팩토리
    │   ├── DeriveKey() — 범용 키 파생
    │   ├── Encrypt() — AES-256-GCM + base64 인코딩
    │   └── Decrypt() — base64 디코딩 + AES-256-GCM
    └── PhiEncryptionService (HnVue.Security) — 대체 구현체
```

### 2. 출력 형식

```
Base64( Nonce[12] || Ciphertext[N] || Tag[16] )
```

- Nonce: 12 bytes (96-bit), 호출마다 RandomNumberGenerator.Fill()로 생성
- Tag: 16 bytes (128-bit), AesGcm 자동 생성/검증
- Ciphertext: 평문과 동일 길이

### 3. HKDF 키 파생 흐름

```
SQLCipher Password → UTF-8 bytes (IKM)
    → HKDF-SHA256(salt="HnVue-PHI-Encryption-v1", info="HnVue-PHI-AES256GCM-v1")
    → 32 bytes AES-256 키
```

**설계 결정**: Salt와 Info를 상수로 관리. 버전 업그레이드 시 상수 변경으로 키 순환 가능.

### 4. PatientEntity 필드별 암호화 적용 순서

| 순서 | 필드 | Converter | 비고 |
|------|------|-----------|------|
| 1 | Name | PhiEncryptedValueConverter | 필수 PHI |
| 2 | DateOfBirth | NullablePhiEncryptedValueConverter | Nullable 처리 |
| 3 | PatientId | PhiEncryptedValueConverter | 필수 PHI |

### 5. EF Core Value Converter 패턴

```csharp
// HnVueDbContext.OnModelCreating
var phiConverter = new PhiEncryptedValueConverter(_phiEncryptionService);
var nullablePhiConverter = new NullablePhiEncryptedValueConverter(_phiEncryptionService);

entity.Property(p => p.Name).HasConversion(phiConverter);
entity.Property(p => p.DateOfBirth).HasConversion(nullablePhiConverter);
entity.Property(p => p.PatientId).HasConversion(phiConverter);
```

**설계 결정**: Value Converter 패턴 선택 이유 — EF Core가 자동으로 변환 처리, 비즈니스 로직에 암호화 관심사 침투 방지.

### 6. DI 등록 전략

```csharp
// ServiceCollectionExtensions.AddHnVueData()
if (phiEncryptionKey != null)
    phiService = new AesGcmPhiEncryptionService(phiEncryptionKey);
else if (sqlCipherPassword != null)
    phiService = AesGcmPhiEncryptionService.FromSqlCipherKey(sqlCipherPassword);
else
    phiService = new AesGcmPhiEncryptionService(RandomNumberGenerator.GetBytes(32)); // dev only
```

**우선순위**: 명시적 키 > SQLCipher 파생 > 랜덤 키(개발용)

---

## Out of Scope (명시적 제외)

- MFA (Multi-Factor Authentication) — 별도 SPEC 필요
- Key Rotation — 별도 SPEC 필요
- 기존 평문 데이터 마이그레이션 — 별도 Migration SPEC 필요
- DICOM 파일 내 PHI 암호화 — Team B 관할

---

## 파일 구조

```
HnVue.Data/
  Services/AesGcmPhiEncryptionService.cs          ← 주 구현체
  Converters/PhiEncryptedValueConverter.cs         ← EF Core Value Converter
  Converters/NullablePhiEncryptedValueConverter.cs ← Nullable 지원
  Extensions/ServiceCollectionExtensions.cs        ← DI 등록
  HnVueDbContext.cs                                ← OnModelCreating Converter 적용

HnVue.Common/
  Abstractions/IPhiEncryptionService.cs            ← 인터페이스

tests/HnVue.Data.Tests/
  Services/AesGcmPhiEncryptionServiceTests.cs      ← 14개 단위 테스트
```
