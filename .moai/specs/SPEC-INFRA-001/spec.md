---
id: SPEC-INFRA-001
version: 1.1.0
status: draft
created: "2026-04-09"
updated: "2026-04-10"
author: moai
priority: P1-Critical
issue_number: 0
---

# SPEC-INFRA-001: Team A 인프라 모듈 기능개선 및 버그픽스

## HISTORY
| 버전 | 날짜 | 변경 내용 | 작성자 |
|------|------|-----------|--------|
| 1.0.0 | 2026-04-09 | 최초 작성 | MoAI |
| 1.1.0 | 2026-04-10 | 교차검증 보강: Security 82.5%->90% 커버리지 목표 추가, SystemSettingsRepository 빌드오류 반영, 취약 패키지 업그레이드 상태 갱신 | MoAI |

## 개요

Team A 관할 5개 모듈(HnVue.Common, HnVue.Data, HnVue.Security, HnVue.SystemAdmin, HnVue.Update)에 대한 기능개선 및 버그픽스. 딥 리서치 결과 식별된 4개 영역의 개선사항을 반영한다.

**배경**: QA 로컬 분석기 활성화 및 교차검증 완료 후, 보안 감사 및 코드 품질 분석에서 다수의 개선 필요사항이 식별됨. 특히 감사 로그 무결성 검증 논리 오류, PHI 암호화 미구현, 업데이트 안전성 미흡은 의료기기 소프트웨어 IEC 62304 준수를 위해 즉시 해결이 필요함.

## 범위

### 포함 (In Scope)
- HnVue.Security: 감사 로그 무결성 검증 수정, JWT 교착상태 해결, 토큰 거부목록 오류 처리
- HnVue.Data: NullPhiEncryptionService 실제 암호화 구현, 감사 추적 사용자 속성 수정, 성능 인덱스 추가
- HnVue.Update: 원자적 롤백 메커니즘, HTTPS 강제, 코드 서명 체인 검증
- HnVue.Common: ISecurityContext 스레드 안전성, ErrorCode 확장
- HnVue.SystemAdmin: 설정 변경 감사 로깅

### 제외 (Out of Scope)
- MFA(Multi-Factor Authentication) 구현 (별도 SPEC 필요)
- 새로운 설정 카테고리 추가 (Hardware, Calibration, Logging)
- DoseRepository/ImageRepository 신규 구현 (별도 SPEC 필요)
- ISecurityService 인터페이스 분리 (breaking change, Coordinator 승인 필요)
- BCrypt work factor 변경 (기존 비밀번호 호환성 검토 필요)

---

## 요구사항

### REQ-SEC-001: 감사 로그 무결성 검증 논리 수정 (Safety-Critical)

**EARS 패턴**: 감사 로그 항목의 해시 체인이 손상된 경우, 시스템은 해당 항목에 대해 검증 실패 결과를 반환해야 한다.

**현재 동작**: `AuditService.cs:89-95`에서 해시 체인이 깨진 경우 `Result.Success(false)`를 반환하여 변조가 탐지되지 않음.

**변경 내용**:
- [MODIFY] `src/HnVue.Security/AuditService.cs` — VerifyChainAsync 메서드: 해시 불일치 시 `Result.Failure<bool>(ErrorCode.IncidentLogFailed, "Audit chain integrity violation")` 반환
- [MODIFY] `src/HnVue.Security/AuditService.cs` — VerifyChainAsync 메서드: 재계산 해시 불일치 시 동일하게 실패 반환
- [NEW] 관련 테스트 케이스 추가: 변조된 체인 감지 시나리오

**수용 조건**: 해시 체인이 손상된 감사 항목 검증 시 `IsSuccess == false` 반환

### REQ-SEC-002: JWT 토큰 검증 교착상태 수정

**EARS 패턴**: 시스템은 JWT 토큰 검증 시 비동기 메서드를 동기적으로 차단하지 않아야 한다.

**현재 동작**: `JwtTokenService.cs:104-106`에서 `.GetAwaiter().GetResult()` 사용으로 잠재적 교착상태 발생.

**변경 내용**:
- [MODIFY] `src/HnVue.Security/JwtTokenService.cs` — ValidateTokenAsync: `IsRevokedAsync` 호출을 비동기로 변경
- [MODIFY] 인터페이스 시그니처가 동기인 경우, `ITokenDenylist.IsRevokedAsync`를 비동기로 유지하고 호출부를 async/await로 수정
- [NEW] 관련 테스트: 비동기 검증 경로 정상 동작 확인

**수용 조건**: JWT 검증 경로에서 `.GetAwaiter().GetResult()` 호출 제거, 모든 비동기 작업에 async/await 사용

### REQ-SEC-003: 토큰 거부목록 파일 손상 처리 개선

**EARS 패턴**: 시스템은 토큰 거부목록 파일이 손상된 경우, 모든 기존 토큰을 무효화하고 관리자에게 알림을 생성해야 한다.

**현재 동작**: `PersistentTokenDenylist.cs:102-109`에서 IOException/JsonException을 무시하여 취소된 토큰이 재활성화됨.

**변경 내용**:
- [MODIFY] `src/HnVue.Security/PersistentTokenDenylist.cs` — 로드 실패 시 빈 거부목록으로 시작 + 로깅
- [MODIFY] 손상 감지 시 모든 기존 JWT 세션 무효화 (보수적 접근)
- [NEW] 관련 테스트: 파일 손상 시나리오

**수용 조건**: 파일 손상 시 기존 토큰이 거부목록을 우회하지 못함, ILogger를 통해 경고 기록

### REQ-DATA-001: NullPhiEncryptionService 실제 암호화 구현

**EARS 패턴**: 시스템은 환자 식별 정보(PHI)를 데이터베이스에 저장할 때 AES-256-GCM 암호화를 적용해야 한다.

**현재 동작**: `HnVue.Data/Security/NullPhiEncryptionService.cs`가 암호화 없이 평문을 반환.

**참조 구현**: `HnVue.Security/PhiEncryptionService.cs`에 AES-256-GCM 구현이 이미 존재함.

**변경 내용**:
- [MODIFY] `src/HnVue.Data/Security/NullPhiEncryptionService.cs` → `PhiEncryptionService.cs`로 교체
- [MODIFY] `src/HnVue.Data/Extensions/ServiceCollectionExtensions.cs` — DI 등록을 실제 구현으로 변경
- [MODIFY] `src/HnVue.Data/Mappers/EntityMapper.cs` — 암호화 서비스 활성화 시 PHI 필드 자동 암호화
- [NEW] 암호화 키 설정 검증 로직

**수용 조건**: PatientRecord의 이름, 생년월일, 주민등록번호 필드가 DB에 암호화되어 저장됨

### REQ-DATA-002: 감사 추적 사용자 속성 수정

**EARS 패턴**: 시스템은 모든 감사 로그 항목에 실제 인증된 사용자를 기록해야 한다.

**현재 동작**: `HnVue.Data/PatientRepository.cs:32-37`에서 "system" 하드코딩.

**변경 내용**:
- [MODIFY] `src/HnVue.Data/Repositories/PatientRepository.cs` — ISecurityContext 주입 후 실제 사용자명 사용
- [MODIFY] `src/HnVue.Data/Repositories/StudyRepository.cs` — 동일 패턴 적용
- [MODIFY] `src/HnVue.Data/Repositories/AuditRepository.cs` — 동일 패턴 적용
- [NEW] ISecurityContext 미설정 시 fallback 로직

**수용 조건**: 감사 로그의 사용자 필드가 ISecurityContext.CurrentUser.UserName과 일치

### REQ-DATA-003: 데이터베이스 성능 인덱스 추가

**EARS 패턴**: 시스템은 자주 조회되는 엔티티 컬럼에 데이터베이스 인덱스를 유지해야 한다.

**변경 내용**:
- [NEW] EF Core 마이그레이션: PatientEntity(Name, IsDeleted 복합 인덱스)
- [NEW] EF Core 마이그레이션: StudyEntity(StudyDateTicks 인덱스)
- [NEW] EF Core 마이그레이션: DoseRecordEntity(StudyInstanceUid, RecordedAtTicks 복합 인덱스)
- [NEW] EF Core 마이그레이션: AuditLogEntity(TimestampTicks 인덱스)

**수용 조건**: 새 마이그레이션이 정상 적용되고, 쿼리 성능이 개선됨

### REQ-UPDATE-001: 원자적 업데이트 롤백 메커니즘

**EARS 패턴**: 시스템은 소프트웨어 업데이트 실패 시, 이전 상태로 완전히 복원되거나 사용자에게 명확한 오류를 보고해야 한다.

**현재 동작**: 부분 업데이트 실패 시 시스템이 미정의 상태로 남음.

**변경 내용**:
- [MODIFY] `src/HnVue.Update/SWUpdateService.cs` — ApplyUpdateAsync: try-catch에서 백업 복원 + 임시 파일 정리
- [NEW] 업데이트 상태 추적 enum (InProgress, Staged, Completed, Failed, RolledBack)
- [NEW] 부분 업데이트 정리 메서드 CleanupPartialUpdateAsync
- [NEW] 관련 테스트: 각 실패 지점별 롤백 시나리오

**수용 조건**: 업데이트 프로세스 중 임의 지점에서 실패해도 시스템이 이전 버전으로 복원됨

### REQ-UPDATE-002: 업데이트 서버 HTTPS 강제

**EARS 패턴**: 시스템은 업데이트 다운로드 시 HTTPS 프로토콜만 허용해야 한다.

**현재 동작**: `UpdateChecker.cs`에서 URL 스킴 검증 없음.

**변경 내용**:
- [MODIFY] `src/HnVue.Update/UpdateOptions.cs` — Validate 메서드에 HTTPS 강제 로직 추가
- [MODIFY] `src/HnVue.Update/UpdateChecker.cs` — 생성자에서 URL 스킴 검증
- [NEW] 관련 테스트: HTTP URL 거부 시나리오

**수용 조건**: HTTP URL로 설정된 경우 설정 검증 실패, 업데이트 다운로드 시도 차단

### REQ-UPDATE-003: 코드 서명 체인 검증 강화

**EARS 패턴**: 시스템은 업데이트 패키지의 코드 서명을 검증할 때 인증서 체인 및 타임스탬프를 확인해야 한다.

**현재 동작**: `SignatureVerifier.cs`에서 `fdwRevocationChecks = None`.

**변경 내용**:
- [MODIFY] `src/HnVue.Update/SignatureVerifier.cs` — 인증서 해지 확인 활성화
- [MODIFY] `src/HnVue.Update/CodeSignVerifier.cs` — 타임스탬프 검증 추가
- [MODIFY] `src/HnVue.Update/UpdateOptions.cs` — 프로덕션에서 서명 검증 비활성화 불가
- [NEW] 관련 테스트

**수용 조건**: 만료된 인증서로 서명된 패키지 거부, 해지된 인증서 거부

### REQ-COMMON-001: ISecurityContext 스레드 안전성

**EARS 패턴**: 시스템은 보안 컨텍스트에 대한 동시 접근 시 데이터 무결성을 보장해야 한다.

**변경 내용**:
- [MODIFY] `src/HnVue.Common/Abstractions/ISecurityContext.cs` — 스레드 안전 명세 추가
- [NEW] `src/HnVue.Security` 구현체에 lock 또는 ReaderWriterLockSlim 적용
- [NEW] 관련 테스트: 동시 설정/해제 시나리오

**수용 조건**: 다중 스레드에서 동시 접근 시 예외 또는 데이터 손상 없음

### REQ-COMMON-002: ErrorCode 네트워크/통신 오류 코드 추가

**EARS 패턴**: 시스템은 네트워크 및 하드웨어 통신 오류를 식별 가능한 에러 코드로 분류해야 한다.

**변경 내용**:
- [MODIFY] `src/HnVue.Common/Results/ErrorCode.cs` — 네트워크 타임아웃, 통신 실패, 하드웨어 응답 없음 등 에러 코드 추가
- [NEW] 관련 테스트

**수용 조건**: 새 에러 코드로 네트워크/통신 문제를 구체적으로 식별 가능

### REQ-SYSADMIN-001: 설정 변경 감사 로깅

**EARS 패턴**: 시스템은 시스템 설정이 변경될 때마다 감사 로그 항목을 생성해야 한다.

**현재 동작**: `SystemAdminService.cs:67`에서 설정 저장 시 감사 로그 생성 없음.

**변경 내용**:
- [MODIFY] `src/HnVue.SystemAdmin/SystemAdminService.cs` — SaveSettingsAsync에 IAuditRepository.AppendAsync 호출 추가
- [NEW] 감사 항목에 이전 값과 새 값 포함
- [NEW] 관련 테스트

**수용 조건**: 설정 변경 시 감사 로그에 변경자, 변경 시간, 변경 필드가 기록됨

---

## 영향 받는 파일

| 파일 | 변경 유형 | 모듈 |
|------|-----------|------|
| src/HnVue.Security/AuditService.cs | MODIFY | Security |
| src/HnVue.Security/JwtTokenService.cs | MODIFY | Security |
| src/HnVue.Security/PersistentTokenDenylist.cs | MODIFY | Security |
| src/HnVue.Data/Security/NullPhiEncryptionService.cs | MODIFY→RENAME | Data |
| src/HnVue.Data/Extensions/ServiceCollectionExtensions.cs | MODIFY | Data |
| src/HnVue.Data/Mappers/EntityMapper.cs | MODIFY | Data |
| src/HnVue.Data/Repositories/PatientRepository.cs | MODIFY | Data |
| src/HnVue.Data/Repositories/StudyRepository.cs | MODIFY | Data |
| src/HnVue.Data/Repositories/AuditRepository.cs | MODIFY | Data |
| src/HnVue.Data/HnVueDbContext.cs | MODIFY | Data |
| src/HnVue.Update/SWUpdateService.cs | MODIFY | Update |
| src/HnVue.Update/UpdateOptions.cs | MODIFY | Update |
| src/HnVue.Update/UpdateChecker.cs | MODIFY | Update |
| src/HnVue.Update/SignatureVerifier.cs | MODIFY | Update |
| src/HnVue.Update/CodeSignVerifier.cs | MODIFY | Update |
| src/HnVue.Common/Abstractions/ISecurityContext.cs | MODIFY | Common |
| src/HnVue.Common/Results/ErrorCode.cs | MODIFY | Common |
| src/HnVue.SystemAdmin/SystemAdminService.cs | MODIFY | SystemAdmin |

## 위험 분석

| 위험 | 확률 | 영향 | 완화 대책 |
|------|------|------|-----------|
| PHI 암호화 전환 시 기존 데이터 복구 불가 | 중간 | 높음 | 마이그레이션 스크립트로 평문→암호문 변환 |
| JWT 비동기 변경으로 인한 API 호환성 | 낮음 | 중간 | 인터페이스 변경 없이 내부 구현만 수정 |
| 감사 로그 검증 실패 시 기존 데이터 처리 | 중간 | 중간 | 기존 체인은 검증 경고만, 신규부터 엄격 검증 |
| 인덱스 추가로 인한 DB 크기 증가 | 낮음 | 낮음 | 의료 영상 시스템에서 DB 크기는 주요 고려사항 아님 |
| 업데이트 롤백 메커니즘의 디스크 공간 | 낮음 | 중간 | 백업 전 디스크 공간 확인 로직 포함 |
