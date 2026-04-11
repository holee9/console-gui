# DISPATCH: Team A — Coverage Floor 달성 (평균 갭 0.2pp, 개별 floor 미달 3개)

Issued: 2026-04-09
Issued By: Main (MoAI Orchestrator)
Priority: P2-High
Source: PHASE1_QA_COVERAGE_TARGETS_2026-04-09.md

## How to Execute

When user says "지시서대로 작업해":
1. Read this entire document
2. Execute tasks in priority order
3. Update Status section after each task
4. Run build verification as final step

## Context

QA Phase 1 확정 기준:
- Team A 평균 목표: **85%+** (현재 84.8%, 갭 0.2pp — 평균은 거의 달성)
- 그러나 개별 모듈 floor 미달 3개:
  - SystemAdmin: 66.6% → 80% (갭 **13.4pp**)
  - Update: 77.3% → 85% (갭 **7.7pp**)
  - Security: 86.2% → 90% (갭 **3.8pp**)
- Common 94.1%, Data 100% → 이미 목표 초과

## File Ownership

- HnVue.Common/**
- HnVue.Data/**
- HnVue.Security/**
- HnVue.SystemAdmin/**
- HnVue.Update/**

## Tasks

### Task 1: SystemAdmin 66.6% → 80.0% (갭 13.4pp, P1-Critical)

**0% 클래스**:
- SystemSettingsRepository (0%) ← 전체 갭의 주요 원인

**50%+ 클래스**:
- SystemAdminService (94.7%) ← 이미 양호

**테스트 작성 대상**:
- SystemSettingsRepository CRUD 테스트 (In-Memory SQLite)
- 설정 키/값 유효성 검증
- 존재하지 않는 설정 조회 시 기본값 반환
- 동시 접근 안전성

**기준**:
- xUnit + FluentAssertions
- [Trait("SWR", "SWR-xxx")] 어노테이션
- async + CancellationToken

**검증 기준**:
- [ ] SystemAdmin line coverage 80%+
- [ ] SystemSettingsRepository 0% → 70%+
- [ ] 빌드 + 테스트 통과

### Task 2: Update 77.3% → 85.0% (갭 7.7pp, P2-High)

**0% 근접 클래스**:
- UpdateRepository (11.7%) ← 핵심 갭

**양호 클래스**: BackupManager 100%, BackupService 100%, SignatureVerifier 100%, SWUpdateService 100%

**테스트 작성 대상**:
- UpdateRepository 버전 조회/저장 테스트
- 업데이트 이력 관리 테스트
- 파일시스템 접근 Mock 테스트

**검증 기준**:
- [ ] Update line coverage 85%+
- [ ] UpdateRepository 11.7% → 70%+
- [ ] 빌드 + 테스트 통과

### Task 3: Security 86.2% → 90.0% (갭 3.8pp, Safety-Adjacent)

**저커버리지 클래스**:
- AuditService (71.4%)
- ServiceCollectionExtensions (0%)

**양호 클래스**: PasswordHasher 100%, SecurityService 100%, JwtTokenService 89.1%, RbacPolicy 98%

**테스트 작성 대상**:
- AuditService 감사 로그 기록/조회/HMAC 체인 검증
- ServiceCollectionExtensions DI 등록 검증
- JwtTokenService 만료 토큰 경계값 테스트

**검증 기준**:
- [ ] Security line coverage 90%+
- [ ] AuditService 71.4% → 85%+
- [ ] ServiceCollectionExtensions 0% → 80%+
- [ ] 빌드 + 테스트 통과

### Task 4: SA* Warning 정리 (P3-Medium)

**접근법**: Team B 패턴 참조 — GlobalSuppressions.cs 생성
- SA1101, SA1309, SA1200, SA1600: GlobalSuppressions.cs 일괄 억제
- 안전 중요 모듈 (Security): 자동 수정 가능한 것만 처리

**검증 기준**:
- [ ] SA* 경고 50% 이상 감소
- [ ] 빌드 + 테스트 통과

## Build Verification

```bash
dotnet build HnVue.sln --configuration Release
dotnet test HnVue.sln --configuration Release --no-build
```

## Status

- **State**: SUPERSEDED
- **Started**: -
- **Completed**: 2026-04-11
- **Results**: S04-R1-team-a.md로 대체됨
