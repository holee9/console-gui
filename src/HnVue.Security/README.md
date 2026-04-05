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

### 비밀번호 정책 (SWR-NF-SC-042, Issue #19)

정규식: `^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*_\-]).{8,}$`

최소 8자, 대문자 1개 이상, 소문자 1개 이상, 숫자 1개 이상, 특수문자(`!@#$%^&*_-`) 1개 이상 필요합니다.

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

| 설정 키 | 환경변수 | 설명 |
|---------|---------|------|
| `Jwt:SecretKey` | `HNVUE_JWT_SECRET` | HMAC-SHA256 JWT 서명 키 (최소 32자) |
| `Security:AuditHmacKey` | `HNVUE_AUDIT_HMAC_KEY` | 감사 로그 HMAC-SHA256 서명 키 |

`appsettings.Development.json`은 `.gitignore`로 추적 제외합니다.
프로덕션 환경에서는 환경변수 또는 외부 키 관리 시스템 사용이 필수입니다.

---

## 테스트

| 항목 | 내용 |
|------|------|
| 테스트 프로젝트 | `tests/HnVue.Security.Tests/` |
| 테스트 파일 | `AuditServiceTests.cs`, `JwtTokenServiceTests.cs`, `PasswordHasherTests.cs`, `RbacPolicyTests.cs`, `SecurityServiceTests.cs` |
| 테스트 케이스 수 | **69개** (`[Fact]` / `[Theory]`) |
