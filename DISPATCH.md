# DISPATCH: S04 R2 — Team A (Infrastructure)

Issued: 2026-04-11
Issued By: Main (MoAI Commander Center)
Sprint: S04 Round 2
SPEC: SPEC-INFRA-002 (P0-Blocker)
Priority: P0-Blocker (Safety-Critical)

## Objective

PHI AES-256-GCM 암호화 완전 구현. 현재 `NullPhiEncryptionService`가 DI에 등록되어 환자 개인정보가 평문 저장됨.
IEC 62304, HIPAA, 국내 의료기기 개인정보 보호 기준 충족 필요.

## SPEC Reference

`.moai/specs/SPEC-INFRA-002/spec.md` — 반드시 전문 읽고 구현할 것.

## Tasks

### T1: AesGcmPhiEncryptionService 구현 (REQ-PHI-001)

**파일**: `src/HnVue.Data/Services/AesGcmPhiEncryptionService.cs` (신규)

구현 요구사항:
- `System.Security.Cryptography.AesGcm` (.NET 8+) 사용
- 키 길이: 256비트 (32바이트)
- Nonce(IV): 96비트 (12바이트), 호출마다 랜덤 생성
- 인증 태그: 128비트 (16바이트)
- 암호화 출력: `[Nonce(12)] + [Ciphertext] + [Tag(16)]` base64 인코딩
- 복호화 시 태그 불일치 → `CryptographicException` 발생
- `IPhiEncryptionService` 인터페이스 구현

### T2: SQLCipher 키 기반 PHI 키 파생 (REQ-PHI-002)

**파일**: `src/HnVue.Data/Services/PhiKeyDerivationService.cs` (신규) 또는 AesGcmPhiEncryptionService 내 포함

구현 요구사항:
- HKDF (HMAC-based Key Derivation) 사용
- SQLCipher 마스터 키 → PHI 전용 암호화 키 파생
- Salt: "HnVue-PHI-Encryption-v1" (고정)
- 파생 키 길이: 32바이트 (AES-256)

### T3: PatientEntity PHI 필드 암호화 적용 (REQ-PHI-003)

**파일**: `src/HnVue.Data/Entities/PatientEntity.cs` 수정

PHI 필드 암호화 적용:
- `Name` (환자명)
- `BirthDate` (생년월일)
- `PatientId` (환자 ID)

### T4: DI 등록 교체 (REQ-PHI-004)

**파일**: `src/HnVue.App/App.xaml.cs` 수정

- `NullPhiEncryptionService` → `AesGcmPhiEncryptionService` 교체
- 생성자 파라미터로 SQLCipher 키 전달

### T5: 단위 테스트 작성 (REQ-PHI-005)

**파일**: `tests/HnVue.Data.Tests/Services/AesGcmPhiEncryptionServiceTests.cs` (신규)

최소 10개 테스트:
- 암호화→복호화 왕복 테스트 3개 (다양한 입력 길이)
- 변조된 태그로 복호화 시 예외 발생 1개
- null/empty 입력 처리 2개
- 동일 평문 두 번 암호화 시 다른 결과(Nonce 랜덤성) 1개
- 키 파생 일관성 테스트 2개
- 경계값 테스트 1개

### T6: 통합 테스트 작성 (REQ-PHI-006)

**파일**: `tests.integration/HnVue.IntegrationTests/PhiEncryptionIntegrationTests.cs` (신규)

최소 2개 테스트:
- 실제 DbContext 연결 암호화→저장→복호화 검증
- DI 컨테이너 통합 검증

## Build Verification [HARD]

완료 전 반드시 실행:
```bash
dotnet build HnVue.sln --no-incremental
dotnet test HnVue.sln --filter "FullyQualifiedName~HnVue.Data" --no-build
```

**게이트**: 0 에러, 모든 신규 테스트 통과

## Git Protocol [HARD]

1. `git add` 관련 파일만
2. `git commit -m "feat(team-a): SPEC-INFRA-002 PHI AES-256-GCM 암호화 완전 구현"`
3. `git push origin team/team-a`
4. PR 생성 (기존 open PR 있으면 업데이트)
5. PR URL을 DISPATCH.md Status에 기록

## Status

- **State**: PENDING
- **Assigned**: Team A
- **PR**: (작성 후 기록)
- **Started**: (시작 시 기록)
- **Completed**: (완료 시 기록)
