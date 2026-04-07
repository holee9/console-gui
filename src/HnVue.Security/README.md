# HnVue.Security

> 인증, 인가, 감사 로그 (IEC 62304 Class B / FDA §524B Cybersecurity)

## 목적

사용자 인증(JWT HS256 + BCrypt cost-12), 역할 기반 접근 제어(RBAC), HMAC-SHA256 해시 체인 감사 로그를 제공합니다.
FDA 사이버보안 요구사항 및 IEC 62304 Class B 안전 요건을 충족합니다.

---

## 주요 타입

| 타입 | 종류 | 설명 |
|------|------|------|
| `SecurityService` | `sealed class` | `ISecurityService` 구현체. 인증·인가·잠금·비밀번호 변경 |
| `JwtTokenService` | `internal sealed class` | JWT HS256 토큰 발급(`Issue`) 및 검증(`Validate`) |
| `JwtOptions` | `sealed class` | JWT 설정 바인딩 (`Jwt` 섹션). SecretKey 최소 32자 강제 |
| `PasswordHasher` | `sealed class` (static API) | BCrypt cost-12 해싱·검증·재해시 필요 여부 확인 |
| `AuditService` | `sealed class` | `IAuditService` 구현체. HMAC-SHA256 해시 체인 감사 로그 |
| `AuditOptions` | `class` | HMAC 키 IOptions 외부화 |
| `RbacPolicy` | `static class` | 역할 권한 조회(`Check`, `GetPermissions`, `HasRoleOrHigher`) |
| `Permissions` | `static class` | 권한 상수 정의 (8가지 named permission) |
| `ServiceCollectionExtensions` | `static class` | `AddHnVueSecurity()` 확장 메서드 |

---

## SecurityService 주요 메서드

| 메서드 | 반환 타입 | 설명 |
|--------|-----------|------|
| `AuthenticateAsync(username, password, CancellationToken)` | `Task<Result<AuthenticationToken>>` | BCrypt 검증 → 실패 5회 자동 잠금 → JWT 발급 → 감사 기록 |
| `CheckAuthorizationAsync(userId, UserRole, CancellationToken)` | `Task<Result>` | `RbacPolicy.HasRoleOrHigher`로 계층적 역할 검사 |
| `LockAccountAsync(userId, CancellationToken)` | `Task<Result>` | 계정 잠금 + 감사 기록 (`ACCOUNT_LOCKED`) |
| `UnlockAccountAsync(userId, adminId, CancellationToken)` | `Task<Result>` | 잠금 해제 + 실패 횟수 초기화 + 감사 기록 (`ACCOUNT_UNLOCKED`) |
| `ChangePasswordAsync(userId, currentPassword, newPassword, CancellationToken)` | `Task<Result>` | 비밀번호 정책 검증 → BCrypt 재해시 → 감사 기록 |
| `LogoutAsync(userId, CancellationToken)` | `Task<Result>` | 로그아웃 감사 로그 (`LOGOUT` 액션). SWR-NF-SC-041 준수 |
| `SetQuickPinAsync(userId, pin, CancellationToken)` | `Task<Result>` | Quick PIN 설정 (4~6자리 숫자, bcrypt 해시 저장). SWR-CS-076. ErrorCode.PinNotSet 사용 가능 |
| `VerifyQuickPinAsync(userId, pin, CancellationToken)` | `Task<Result>` | Quick PIN 검증. 3회 실패 시 5분 잠금 (무차별대입 방지). SWR-CS-076 |

### 비밀번호 정책 (SWR-NF-SC-042, Issue #19)

정규식: `^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*_\-]).{8,}$`

최소 8자, 대문자 1개 이상, 소문자 1개 이상, 숫자 1개 이상, 특수문자(`!@#$%^&*_-`) 1개 이상 필요합니다.

### Quick PIN (SWR-CS-076, Issue #12)

세션 중 화면 잠금 기능 지원:

| 항목 | 설명 |
|------|------|
| **PIN 길이** | 4~6자리 숫자 |
| **해싱** | bcrypt (cost=12, 유효기간 없음) |
| **실패 정책** | 3회 실패 시 5분 잠금 (무차별대입 방지) |
| **저장** | `UserEntity.QuickPinHash`, `UserEntity.QuickPinFailedCount`, `UserEntity.QuickPinLockedUntilTicks` |
| **무차별대입 방지** | 5분 이내 3회 이상 실패 시 `QuickPinLockedUntilTicks` 설정 |

QuickPinLockedUntilTicks >= 현재 시각 이면 검증 불가.

---

## RBAC 역할 계층

| 역할 | 레벨 | 주요 권한 |
|------|------|-----------|
| `Service` | 3 | 시스템 설정, 감사 로그 조회, 소프트웨어 업데이트 |
| `Admin` | 3 | 시스템 설정, 감사 로그 조회, 소프트웨어 업데이트, 환자 등록, CD 굽기 |
| `Radiologist` | 2 | 환자 조회/등록, 노출 수행, 영상 검토, CD 굽기 |
| `Radiographer` | 1 | 환자 조회/등록, 노출 수행 |

---

## AuditService

HMAC-SHA256 해시 체인으로 감사 로그 변조를 감지합니다.
각 항목은 이전 항목의 해시를 참조하여 체인을 구성합니다.

| 메서드 | 설명 |
|--------|------|
| `WriteAuditAsync(AuditEntry, CancellationToken)` | 이전 해시 조회 → HMAC 계산 → 저장 |
| `VerifyChainIntegrityAsync(CancellationToken)` | 전체 체인 재검증 (변조 탐지) |
| `GetAuditLogsAsync(AuditQueryFilter, CancellationToken)` | 필터 기반 감사 로그 조회 |

---

## DI 등록

```csharp
services.AddHnVueSecurity(jwtOptions, auditOptions);
```

`AddHnVueSecurity()` 내부 동작:
- `JwtOptions.SecretKey`가 null이거나 32자 미만이면 **startup 시 `InvalidOperationException`** 발생 (Issue #18)
- `ISecurityService` → `SecurityService` (Scoped)
- `IAuditService` → `AuditService` (Scoped)
- `JwtTokenService` (Singleton, internal)
- `IOptions<AuditOptions>` (Singleton)

---

## 의존성

### 프로젝트 참조

| 프로젝트 | 제공 항목 |
|----------|-----------|
| `HnVue.Common` | `ISecurityService`, `IAuditService`, `IUserRepository`, `IAuditRepository`, `AuthenticationToken`, `AuditEntry`, `UserRole`, `Result<T>`, `ErrorCode` |

### NuGet 패키지

| 패키지 | 용도 |
|--------|------|
| `BCrypt.Net-Next` | BCrypt cost-12 비밀번호 해싱·검증 |
| `System.IdentityModel.Tokens.Jwt` | JWT 발급·파싱 |
| `Microsoft.IdentityModel.Tokens` | 토큰 검증 파라미터 |
| `Microsoft.Extensions.Options` | `IOptions<T>` 바인딩 |
| `Serilog` | 보안 이벤트 로깅 |
| `Serilog.Sinks.File` | 파일 기반 로그 싱크 |

---

## 보안 설정 (필수)

### 설정 위치

| 파일 | 목적 | git 추적 |
|------|------|----------|
| `appsettings.json` | 기본 설정 (빈 값) | ✅ 추적 |
| `appsettings.Development.json` | 개발 환경 설정값 | ❌ 제외 (.gitignore) |
| 환경변수 또는 KeyVault | 프로덕션 배포 | N/A |

### 필수 설정 키

| 설정 키 | 환경변수 | 설명 |
|---------|---------|------|
| `Jwt:SecretKey` | `HNVUE_JWT_SECRET` | HMAC-SHA256 JWT 서명 키 (최소 32자) |
| `Security:AuditHmacKey` | `HNVUE_AUDIT_HMAC_KEY` | 감사 로그 HMAC-SHA256 서명 키 |

### appsettings.json 생성

```json
{
  "Jwt": {
    "SecretKey": ""
  },
  "Security": {
    "AuditHmacKey": ""
  }
}
```

프로덕션 환경에서는 환경변수 또는 Azure KeyVault 사용 필수입니다.

---

## 테스트

| 항목 | 내용 |
|------|------|
| 테스트 프로젝트 | `tests/HnVue.Security.Tests/` |
| 테스트 파일 | `AuditServiceTests.cs`, `JwtTokenServiceTests.cs`, `PasswordHasherTests.cs`, `RbacPolicyTests.cs`, `SecurityServiceTests.cs` |
| 테스트 케이스 수 | **120개** (`[Fact]` / `[Theory]`) — LogoutAsync/SetQuickPinAsync/VerifyQuickPinAsync 신규 21개 시나리오 포함 |
| 테스트 커버리지 | **91.5%** (SWR-NF-MT-051 충족) |

---

## JWT Token Denylist (로그아웃 토큰 폐기 — Issue #29)

### 개요

`LogoutAsync()` 호출 시 JWT JTI(JWT ID)를 폐기 목록(Denylist)에 추가하여 로그아웃된 토큰이 재사용되지 않도록 합니다.
이는 세션 탈취 시나리오에서 강제 로그아웃 기능을 제공합니다.

### 주요 타입

| 타입 | 설명 | 위치 |
|------|------|------|
| `ITokenDenylist` | 토큰 폐기 목록 인터페이스 | `src/HnVue.Security/ITokenDenylist.cs` |
| `InMemoryTokenDenylist` | 메모리 기반 구현체 (프로세스 생존 기간 동안 유지) | `src/HnVue.Security/InMemoryTokenDenylist.cs` |

### 동작 원리

#### Step 1: LogoutAsync 호출
```
SecurityService.LogoutAsync(token)
  ↓
JWT에서 JTI 추출 (예: "jti-abc123")
  ↓
ITokenDenylist.AddAsync(jti, expiry) 호출
  ↓
메모리에 저장
```

#### Step 2: 이후 요청에서 토큰 검증
```
SecurityService.ValidateTokenAsync(token)
  ↓
JWT 디코딩 및 기본 검증 (서명, 만료 등)
  ↓
ITokenDenylist.IsRevokedAsync(jti) 호출
  ↓
폐기되었으면 Unauthorized 반환
  ↓
정상이면 계속 진행
```

### 메서드

| 메서드 | 설명 |
|--------|------|
| `AddAsync(jti, expiry)` | JTI를 폐기 목록에 추가 (만료 시간 지정) |
| `IsRevokedAsync(jti)` | JTI가 폐기되었는지 확인 |
| `RemoveAsync(jti)` | JTI를 폐기 목록에서 제거 (선택사항) |

### 보안 고려사항

- **메모리 유지**: `InMemoryTokenDenylist`는 프로세스 재시작 시 초기화됩니다.
  영속적인 폐기가 필요한 경우, Redis 또는 데이터베이스 기반 구현으로 교체할 수 있습니다.
  
- **만료 시간**: JWT의 `exp` 클레임과 동일한 시간을 폐기 목록 만료로 설정하여 메모리 누수 방지.

- **성능**: 메모리 기반 이므로 조회(`IsRevokedAsync`) 성능 우수.

### SWR 준수

- **SWR-SEC-029** (JWT 로그아웃 토큰 무효화): 이 섹션에서 구현
- **SWR-NF-SC-041** (LogoutAsync 감사 기록): 이미 구현 완료
