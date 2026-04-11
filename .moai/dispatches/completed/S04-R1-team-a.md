# DISPATCH: Team A — S04 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-11 |
| **발행자** | Main (MoAI Orchestrator) |
| **대상** | Team A (인프라 팀) |
| **브랜치** | team/team-a |
| **유형** | S04 Round 1 — PHI AES-256-GCM 구현 |
| **우선순위** | P0 (PHI AES-256-GCM), P1 (SPEC-INFRA-001 검증) |
| **SPEC 참조** | SPEC-INFRA-002, SPEC-INFRA-001 |
| **SWR** | SWR-CS-080 |
| **Gitea API** | http://10.11.1.40:7001/api/v1 (repo: drake.lee/Console-GUI) |

---

## 실행 방법

이 문서 전체를 읽고 Task 순서대로 실행하라.
- Task 1 완료 후 Task 2 착수
- 각 Task 완료 후 Status 섹션 업데이트 필수

---

## 컨텍스트

SPEC-INFRA-001에서 PHI AES-256-GCM 구현이 "별도 SPEC 필요"로 제외된 바 있음.
S04에서 SPEC-INFRA-002로 독립 SPEC이 생성되었으며, 이 DISPATCH가 구현 지시서임.

현재 `NullPhiEncryptionService`가 DI에 등록되어 모든 PHI 필드가 평문 저장됨.
이는 IEC 62304, SWR-CS-080 위반 사항으로 S04 P0 최우선 항목.

---

## 파일 소유권

```
HnVue.Common/
HnVue.Data/
HnVue.Security/
HnVue.SystemAdmin/
HnVue.Update/
tests/HnVue.Data.Tests/
tests/HnVue.Security.Tests/
```

---

## Task 1 (P0): PHI AES-256-GCM 완전 구현 (SPEC-INFRA-002)

### 사전 확인

```bash
# IPhiEncryptionService 인터페이스 위치 확인
grep -r "IPhiEncryptionService" src/ --include="*.cs" -l

# NullPhiEncryptionService 현재 등록 위치 확인
grep -r "NullPhiEncryptionService" src/ --include="*.cs" -l

# PatientEntity PHI 필드 확인
# src/HnVue.Data/Entities/PatientEntity.cs 읽기

# 현재 DbContext 파일 확인
# src/HnVue.Data/HnVueDbContext.cs (또는 ApplicationDbContext.cs) 읽기
```

### 1.1 AesGcmPhiEncryptionService 구현

**신규 파일**: `HnVue.Data/Services/AesGcmPhiEncryptionService.cs`

구현 요구사항:
- `System.Security.Cryptography.AesGcm` (.NET 8+) 사용
- 키 길이: 256비트 (32바이트)
- Nonce(IV): 96비트 (12바이트), 호출마다 `RandomNumberGenerator.GetBytes(12)` 생성
- 인증 태그: 128비트 (16바이트)
- 암호화 출력 형식: `Base64(Nonce[12] + Ciphertext[n] + Tag[16])`
- `IPhiEncryptionService` 인터페이스 구현

```csharp
// 구현 핵심 패턴:
public string Encrypt(string plaintext)
{
    if (string.IsNullOrEmpty(plaintext)) return plaintext;
    var nonce = new byte[12];
    RandomNumberGenerator.Fill(nonce);
    var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
    var ciphertext = new byte[plaintextBytes.Length];
    var tag = new byte[16];
    using var aes = new AesGcm(_key, 16);
    aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);
    var result = new byte[12 + ciphertext.Length + 16];
    nonce.CopyTo(result, 0);
    ciphertext.CopyTo(result, 12);
    tag.CopyTo(result, 12 + ciphertext.Length);
    return Convert.ToBase64String(result);
}
```

**수용 기준**: 암호화-복호화 왕복, 변조 태그 예외, null/empty 처리, Nonce 랜덤성

### 1.2 HKDF 키 파생 구현

**AesGcmPhiEncryptionService 생성자** 또는 별도 `PhiKeyDerivationService`에서:

```csharp
// IConfiguration 또는 IOptions<SqlCipherOptions>에서 SQLCipher 비밀번호 수신
// HKDF로 PHI 암호화 키 파생
private static byte[] DeriveKey(string sqlCipherPassword)
{
    var ikm = Encoding.UTF8.GetBytes(sqlCipherPassword);
    var salt = Encoding.UTF8.GetBytes("HnVue-PHI-Salt-v1");
    var info = Encoding.UTF8.GetBytes("HnVue-PHI-AES256GCM-v1");
    return HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, 32, salt, info);
}
```

**수용 기준**: 동일 입력 → 동일 키, 다른 입력 → 다른 키

### 1.3 PhiEncryptedValueConverter 구현 (EF Core Value Converter)

**신규 파일**: `HnVue.Data/Converters/PhiEncryptedValueConverter.cs`

```csharp
// ValueConverter<string, string> 상속
// ModelClrType: string (평문), ProviderClrType: string (암호화 텍스트)
// 저장 시: 평문 → Encrypt() 호출
// 읽기 시: 암호화 텍스트 → Decrypt() 호출
```

### 1.4 PatientEntity.cs PHI 필드 암호화 적용

**수정 파일**: `HnVue.Data/Entities/PatientEntity.cs`

EF Core OnModelCreating에서 Value Converter 등록:

```csharp
// HnVueDbContext.OnModelCreating에 추가:
modelBuilder.Entity<PatientEntity>()
    .Property(p => p.Name)
    .HasConversion(phiConverter);

modelBuilder.Entity<PatientEntity>()
    .Property(p => p.BirthDate)
    .HasConversion(phiConverter);

modelBuilder.Entity<PatientEntity>()
    .Property(p => p.PatientId)
    .HasConversion(phiConverter);
```

**수용 기준**: 저장 후 DB 원시값 암호화 확인, 조회 후 복호화된 평문 일치

### 1.5 DI 등록 교체

**수정 파일**: `HnVue.App/App.xaml.cs` (또는 Team A 소유 DI 등록 위치)

```csharp
// 제거:
// services.AddXxx<IPhiEncryptionService, NullPhiEncryptionService>()

// 추가:
services.AddSingleton<IPhiEncryptionService, AesGcmPhiEncryptionService>();
```

**주의**: App.xaml.cs는 Coordinator 소유이므로, 변경이 필요한 경우 Coordinator와 조율.
Team A 소유 파일에 별도 ServiceCollectionExtensions가 있다면 그 파일 수정.

### 1.6 단위 테스트 작성

**파일**: `tests/HnVue.Security.Tests/` 또는 `tests/HnVue.Data.Tests/`

필수 10개 이상 테스트:
1. `Encrypt_ValidPlaintext_ReturnsBase64String`
2. `Decrypt_ValidCiphertext_ReturnsOriginalPlaintext`
3. `Encrypt_SamePlaintext_ReturnsDifferentCiphertext` (Nonce 랜덤성)
4. `Decrypt_TamperedTag_ThrowsCryptographicException`
5. `Encrypt_EmptyString_ReturnsEmpty`
6. `Encrypt_NullString_ReturnsNull`
7. `Decrypt_EmptyString_ReturnsEmpty`
8. `DeriveKey_SameInput_ReturnsSameKey`
9. `DeriveKey_DifferentInput_ReturnsDifferentKey`
10. `Encrypt_LongString_RoundTrip_Success` (경계 케이스)

**수용 기준**: 10개 이상 전원 PASS, AesGcmPhiEncryptionService 라인 커버리지 90%+

---

## Task 2 (P1): SPEC-INFRA-001 완료 검증

### 확인 항목

SPEC-INFRA-001에서 정의된 구현 항목이 모두 완료되었는지 검증:

```bash
# 1. 솔루션 빌드
dotnet build HnVue.sln

# 2. Team A 관련 테스트 전체 실행
dotnet test tests/HnVue.Security.Tests/ --no-build
dotnet test tests/HnVue.Data.Tests/ --no-build
dotnet test tests/HnVue.Common.Tests/ --no-build
dotnet test tests/HnVue.SystemAdmin.Tests/ --no-build
dotnet test tests/HnVue.Update.Tests/ --no-build

# 3. 커버리지 확인
dotnet test --collect:"XPlat Code Coverage"
```

### 검증 기준

| 모듈 | 목표 커버리지 | SPEC-INFRA-001 항목 |
|------|------------|-------------------|
| HnVue.Security | 90%+ | 감사로그 무결성, JWT, 토큰 거부목록 |
| HnVue.Data | 85%+ | 암호화, 감사추적, 인덱스 |
| HnVue.Update | 80%+ | 롤백, HTTPS 강제, 코드 서명 |
| HnVue.Common | 80%+ | ISecurityContext 스레드 안전성 |
| HnVue.SystemAdmin | 80%+ | 설정 감사 로깅 |

**수용 기준**: SPEC-INFRA-001 수용 기준 전체 충족 확인 후 보고

---

## 빌드 검증

```bash
dotnet build HnVue.sln
dotnet test tests/HnVue.Security.Tests/ --no-build
dotnet test tests/HnVue.Data.Tests/ --no-build
```

---

## Git 완료 프로토콜 [HARD]

모든 Task 완료 후 순서대로 실행:

```bash
# 1. 스테이징
git add HnVue.Data/Services/AesGcmPhiEncryptionService.cs
git add HnVue.Data/Converters/PhiEncryptedValueConverter.cs
git add HnVue.Data/Entities/PatientEntity.cs
git add HnVue.Data/HnVueDbContext.cs  # OnModelCreating 수정 시
git add tests/HnVue.Security.Tests/
git add tests/HnVue.Data.Tests/
# (변경된 파일 모두 명시적으로 추가)

# 2. 커밋
git commit -m "feat(security): implement AES-256-GCM PHI encryption with HKDF key derivation (SPEC-INFRA-002, SWR-CS-080)"

# 3. 푸시
git push origin team/team-a

# 4. PR 생성
curl -X POST "http://10.11.1.40:7001/api/v1/repos/drake.lee/Console-GUI/pulls" \
  -H "Authorization: token a4cb79626194b34a2d52835de05fb770162af014" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "[S04-R1-TeamA] PHI AES-256-GCM 암호화 구현 (SPEC-INFRA-002, SWR-CS-080)",
    "body": "S04 Round 1 Team A DISPATCH 완료\n\n## 변경 사항\n- AesGcmPhiEncryptionService 신규 구현 (AES-256-GCM + HKDF 키 파생)\n- PhiEncryptedValueConverter 신규 구현 (EF Core Value Converter)\n- PatientEntity: Name, BirthDate, PatientId 필드 암호화 적용\n- NullPhiEncryptionService DI 등록 교체\n\n## 테스트\n- 단위 테스트 10개+ PASS\n- AesGcmPhiEncryptionService 커버리지 90%+\n\n## 규제 매핑\n- SWR-CS-080 충족\n- IEC 62304 §5.1 준수",
    "head": "team/team-a",
    "base": "main"
  }'
```

---

## Status (작업 후 업데이트)

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: AES-256-GCM 구현 | NOT_STARTED | -- | -- |
| Task 2: SPEC-INFRA-001 검증 | NOT_STARTED | -- | -- |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |

### 빌드 검증 결과

```
# 여기에 실제 빌드/테스트 결과 기록
dotnet build: ?
HnVue.Security.Tests: ?
HnVue.Data.Tests: ?
AesGcmPhiEncryptionService 커버리지: ?
```
