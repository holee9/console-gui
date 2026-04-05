# HnVue.Security

> 인증, 인가, 감사 로그 (FDA §524B Cybersecurity)

## 목적

사용자 인증(JWT + BCrypt), 역할 기반 접근 제어(RBAC), 감사 로그(Audit Trail)를 제공합니다. FDA 사이버보안 요구사항을 충족합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `SecurityService` | ISecurityService 구현체 — 인증/인가 |
| `JwtTokenService` | JWT 토큰 발급/검증 |
| `JwtOptions` | JWT 설정 옵션 (기본 시크릿 없음 — 런타임 검증) |
| `PasswordHasher` | BCrypt 기반 패스워드 해싱 |
| `AuditOptions` | HMAC 키 IOptions 외부화 (하드코딩 시크릿 제거) |
| `AuditService` | IAuditService 구현체 — 감사 로그 HMAC-SHA256 해시 체인 |
| `RbacPolicy` | 역할 기반 접근 제어 정책 |
| `Permissions` | 권한 상수 정의 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`

### NuGet 패키지

- `BCrypt.Net-Next`
- `System.IdentityModel.Tokens.Jwt`
- `Microsoft.IdentityModel.Tokens`
- `Microsoft.Extensions.Options`
- `Serilog`
- `Serilog.Sinks.File`

## DI 등록

`AddHnVueSecurity()` — SecurityService, AuditService, JwtTokenService, IOptions<AuditOptions> 등록

## 보안 설정 (필수)

| 설정 키 | 환경변수 | 설명 |
|--------|---------|------|
| `Security:AuditHmacKey` | `HNVUE_AUDIT_HMAC_KEY` | HMAC-SHA256 감사 로그 서명 키 |
| `Security:JwtSecretKey` | — | JWT HS256 서명 키 |

`appsettings.Development.json`은 git 추적 제외 (`.gitignore`). 프로덕션 배포 시 환경변수 또는 외부 키 관리 시스템 사용 필수.

## 비고

- FDA 21 CFR Part 11 전자서명 지원
- 자동 세션 타임아웃 (15분 기본값)
- Serilog 기반 보안 이벤트 로깅
- `JwtOptions` 런타임 검증: SecretKey 미설정 시 예외 발생
- `AuditOptions.HmacKey` 미설정 시 `ArgumentException` — 키 없이는 서비스 시작 불가
