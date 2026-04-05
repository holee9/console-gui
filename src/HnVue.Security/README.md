# HnVue.Security

> 인증, 인가, 감사 로그 (FDA §524B Cybersecurity)

## 목적

사용자 인증(JWT + BCrypt), 역할 기반 접근 제어(RBAC), 감사 로그(Audit Trail)를 제공합니다. FDA 사이버보안 요구사항을 충족합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `SecurityService` | ISecurityService 구현체 — 인증/인가 |
| `JwtTokenService` | JWT 토큰 발급/검증 |
| `JwtOptions` | JWT 설정 옵션 |
| `PasswordHasher` | BCrypt 기반 패스워드 해싱 |
| `AuditService` | IAuditService 구현체 — 감사 로그 |
| `RbacPolicy` | 역할 기반 접근 제어 정책 |
| `Permissions` | 권한 상수 정의 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`

### NuGet 패키지

- `BCrypt.Net-Next`
- `System.IdentityModel.Tokens.Jwt`
- `Microsoft.IdentityModel.Tokens`
- `Serilog`
- `Serilog.Sinks.File`

## DI 등록

`AddHnVueSecurity()` — SecurityService, AuditService, JwtTokenService 등록

## 비고

- FDA 21 CFR Part 11 전자서명 지원
- 자동 세션 타임아웃 (15분 기본값)
- Serilog 기반 보안 이벤트 로깅
