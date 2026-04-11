---
id: SPEC-INFRA-002
version: 1.0.0
status: approved
created: "2026-04-11"
updated: "2026-04-11"
author: moai
priority: P0-Blocker
issue_number: 0
team: team-a
sprint: S04
swr: SWR-CS-080
---

# SPEC-INFRA-002: PHI AES-256-GCM 암호화 완전 구현

## HISTORY

| 버전 | 날짜 | 변경 내용 | 작성자 |
|------|------|-----------|--------|
| 1.0.0 | 2026-04-11 | 최초 작성 — S04 Team A PHI 암호화 SPEC | MoAI |

## 개요

환자 개인 식별 정보(PHI: Protected Health Information)에 대한 AES-256-GCM 암호화를
완전 구현한다. 현재 `NullPhiEncryptionService`가 DI에 등록되어 있어 PHI 필드가
평문으로 저장되는 상태이며, 이는 IEC 62304, HIPAA 및 국내 의료기기 개인정보 보호 기준을 위반한다.

**배경**: SPEC-INFRA-001에서 `NullPhiEncryptionService`를 실제 구현으로 교체하는 항목이
"별도 SPEC 필요"로 Out of Scope 처리된 바 있음. 이 SPEC이 해당 항목을 구체적으로 정의함.
SWR-CS-080(PHI 암호화 요구사항)의 충족이 핵심 목표다.

## 범위

### 포함 (In Scope)

- `HnVue.Data/Services/AesGcmPhiEncryptionService.cs` 신규 작성
- `HnVue.Data/Entities/PatientEntity.cs` PHI 필드 암호화 적용
  - `Name` (환자명)
  - `BirthDate` (생년월일)
  - `PatientId` (환자 ID)
- `HnVue.App/App.xaml.cs` DI 등록 교체 (NullPhiEncryptionService → AesGcmPhiEncryptionService)
- `HnVue.Security`의 SQLCipher 키에서 PHI 암호화 키 파생 (HKDF)
- 단위 테스트 10개 이상
- `tests.integration/` 통합 테스트 2개 이상

### 제외 (Out of Scope)

- MFA(Multi-Factor Authentication) 구현
- 키 교체(Key Rotation) 메커니즘 (별도 SPEC 필요)
- 기존 평문 데이터 마이그레이션 스크립트 (별도 Migration SPEC 필요)
- DICOM 파일 내 PHI 암호화 (Team B 관할)

---

## 요구사항

### REQ-PHI-001: AesGcmPhiEncryptionService 구현 (Safety-Critical)

**EARS 패턴**: PHI 필드를 암호화하는 경우, 시스템은 AES-256-GCM 알고리즘을 사용하여 인증된 암호화를 수행해야 하며, 복호화 시 인증 태그 검증에 실패하면 예외를 발생시켜야 한다.

**현재 동작**: `NullPhiEncryptionService`가 암호화/복호화 모두 입력값을 그대로 반환.

**요구 동작**:
- `System.Security.Cryptography.AesGcm` (.NET 8+) 사용
- 키 길이: 256비트 (32바이트)
- Nonce(IV): 96비트 (12바이트), 호출마다 랜덤 생성
- 인증 태그: 128비트 (16바이트)
- 암호화 출력 형식: `[Nonce(12)] + [Ciphertext] + [Tag(16)]` base64 인코딩
- 복호화 시 태그 불일치 → `CryptographicException` 발생

**수용 기준**:
- 암호화 → 복호화 왕복 테스트 3개 (다양한 입력 길이)
- 변조된 태그로 복호화 시 예외 발생 테스트 1개
- null/empty 입력 처리 테스트 2개
- 동일 평문을 두 번 암호화 시 다른 결과(Nonce 랜덤성) 테스트 1개

---

### REQ-PHI-002: SQLCipher 키 기반 PHI 암호화 키 파생

**EARS 패턴**: 애플리케이션이 초기화되는 경우, PHI 암호화 키는 SQLCipher 데이터베이스 키에서 HKDF(HMAC-based Key Derivation Function)를 사용하여 파생되어야 하며, PHI 키와 DB 키는 동일하면 안 된다.

**요구 동작**:
- `HKDF.DeriveKey` (System.Security.Cryptography, .NET 8+) 사용
- Input Key Material: SQLCipher 연결 문자열의 Password 파라미터
- Salt: 앱 고정 상수 (버전 관리됨)
- Info: `"HnVue-PHI-AES256GCM-v1"` UTF-8 바이트
- Output Length: 32바이트

**수용 기준**:
- 동일 입력 → 동일 파생 키 (결정론적) 테스트 1개
- 다른 입력 → 다른 파생 키 테스트 1개
- Salt/Info 변경 시 다른 키 파생 테스트 1개

---

### REQ-PHI-003: PatientEntity PHI 필드 암호화 적용

**EARS 패턴**: PatientEntity가 DbContext를 통해 저장되는 경우, Name, BirthDate, PatientId 필드는 AesGcmPhiEncryptionService를 통해 암호화된 값으로 저장되어야 한다.

**현재 동작**: `PatientEntity.cs`의 PHI 필드가 평문으로 `DbSet`에 저장됨.

**요구 동작**:
- EF Core Value Converter 패턴 사용하여 PHI 필드 자동 암/복호화
- `PhiEncryptedValueConverter` 구현 (IPhiEncryptionService 주입)
- `OnModelCreating`에서 PHI 필드에 Value Converter 등록
- 저장: 평문 → 암호화 → DB 저장
- 조회: DB → 암호화 텍스트 → 복호화 → 평문 반환

**수용 기준**:
- 저장 후 DB 원시값 암호화 확인 테스트 1개
- 조회 후 복호화된 평문 일치 테스트 1개
- 필드별 독립 암호화 (Name 변조 시 BirthDate 복호화 영향 없음) 테스트 1개

---

### REQ-PHI-004: DI 등록 교체

**EARS 패턴**: 애플리케이션이 시작되는 경우, DI 컨테이너에서 IPhiEncryptionService는 NullPhiEncryptionService가 아닌 AesGcmPhiEncryptionService를 반환해야 한다.

**요구 동작**:
- `HnVue.App/App.xaml.cs`에서 `NullPhiEncryptionService` 등록 제거
- `AesGcmPhiEncryptionService` Singleton으로 등록
- 키 파생 초기화는 앱 시작 시 1회 수행

**수용 기준**:
- `NullPhiEncryptionService`가 App.xaml.cs에서 미등장
- 앱 시작 후 DI 해석 오류 없음

---

### REQ-PHI-005: 단위 테스트 커버리지

**EARS 패턴**: AesGcmPhiEncryptionService 및 PhiEncryptedValueConverter에 대한 단위 테스트는 긍정 케이스, 경계 케이스, 오류 케이스를 모두 포함해야 한다.

**요구 동작**:
- `HnVue.Security.Tests` 또는 `HnVue.Data.Tests` 내 10개 이상의 단위 테스트
- 테스트명 규칙: `{Class}_{시나리오}_{예상결과}`
- xUnit 프레임워크 사용

**수용 기준**:
- 단위 테스트 10개 이상 전원 통과
- `AesGcmPhiEncryptionService` 라인 커버리지 90% 이상

---

## 수용 기준 종합

| ID | 항목 | SWR 매핑 | 검증 방법 | 우선순위 |
|----|------|----------|-----------|---------|
| AC-001 | AesGcmPhiEncryptionService 파일 존재 | SWR-CS-080 | 파일 확인 | P0 |
| AC-002 | AES-256-GCM 사용 (256비트 키, 96비트 Nonce) | SWR-CS-080 | 코드 리뷰 | P0 |
| AC-003 | 암호화-복호화 왕복 일관성 | SWR-CS-080 | 단위 테스트 | P0 |
| AC-004 | 변조된 태그 복호화 시 예외 발생 | SWR-CS-080 | 단위 테스트 | P0 |
| AC-005 | HKDF 키 파생 구현 | SWR-CS-080 | 코드 리뷰 | P0 |
| AC-006 | PatientEntity Name 필드 암호화 저장 | SWR-CS-080 | 통합 테스트 | P0 |
| AC-007 | PatientEntity BirthDate 필드 암호화 저장 | SWR-CS-080 | 통합 테스트 | P0 |
| AC-008 | PatientEntity PatientId 필드 암호화 저장 | SWR-CS-080 | 통합 테스트 | P0 |
| AC-009 | NullPhiEncryptionService App.xaml.cs 미등장 | SWR-CS-080 | 코드 검색 | P0 |
| AC-010 | 단위 테스트 10개 이상 전원 통과 | SWR-CS-080 | dotnet test | P0 |
| AC-011 | AesGcmPhiEncryptionService 라인 커버리지 90%+ | -- | coverlet | P1 |
| AC-012 | 전체 솔루션 빌드 0 에러 | -- | dotnet build | P0 |

---

## 기술 접근 방안

### 파일 구조

```
HnVue.Data/
  Services/
    AesGcmPhiEncryptionService.cs    (신규 — IPhiEncryptionService 구현)
  Converters/
    PhiEncryptedValueConverter.cs    (신규 — EF Core Value Converter)
  Entities/
    PatientEntity.cs                 (수정 — PHI 필드 암호화 적용)

HnVue.App/
  App.xaml.cs                        (수정 — NullPhiEncryptionService 교체)
```

### 암호화 출력 형식

```
Base64( Nonce[12bytes] || Ciphertext[n bytes] || Tag[16bytes] )
```

### EF Core Value Converter 패턴

```
modelBuilder.Entity<PatientEntity>()
    .Property(p => p.Name)
    .HasConversion(phiConverter);
```

### 키 파생 흐름

```
SQLCipher Password (string)
    -> UTF-8 bytes (IKM)
    -> HKDF-SHA256 (salt=fixed, info="HnVue-PHI-AES256GCM-v1")
    -> 32 bytes (AES-256 키)
```

---

## 규제 매핑

| 요구사항 | 규제/표준 | 문서 |
|----------|----------|------|
| PHI 필드 암호화 | IEC 62304 §5.1 소프트웨어 개발 계획 | SRS DOC-005 SWR-CS-080 |
| 인증된 암호화(AEAD) | NIST SP 800-38D (AES-GCM) | 사이버보안 계획 DOC-016 |
| 키 파생 | NIST SP 800-56C (HKDF) | 위협 모델 DOC-017 |
| 개인정보 보호 | 의료기기 소프트웨어 개인정보 보호 기준 | -- |

---

## 관련 문서

- `SPEC-INFRA-001`: PHI AES-256-GCM을 Out of Scope로 처리한 이전 SPEC
- `HnVue.Data/Entities/PatientEntity.cs`: PHI 필드 보유 엔티티
- `HnVue.Security/Services/`: IPhiEncryptionService 인터페이스 위치
- `docs/regulatory/SRS_DOC-005`: SWR-CS-080 원문
- `.claude/rules/teams/team-a.md`: Team A 보안 코드 기준

---

## Git 완료 프로토콜

1. `git add` 변경 파일 (비밀키, 임시 파일 제외)
2. `git commit` — 커밋 메시지: `feat(security): implement AES-256-GCM PHI encryption (SWR-CS-080)`
3. `git push origin team/team-a`
4. Gitea API로 PR 생성 (`http://10.11.1.40:7001/api/v1`, repo: `drake.lee/Console-GUI`)
5. PR URL을 DISPATCH Status 섹션에 기록
