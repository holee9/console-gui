# HnVue 보안 설정 및 주의사항

> 원본: README.md "보안 설정 및 주의사항" 섹션에서 분리 (2026-04-09)

## 기본값 (개발용)

현재 소스코드에 포함된 기본값들:

| 항목 | 파일 | 기본값 | 용도 |
|------|------|--------|------|
| **JWT Secret** | `JwtOptions.cs` | `"your-secret-key-at-least-32-characters"` | 토큰 서명 |
| **HMAC Key** | `AuditService.cs` | `"default-hmac-key-for-development"` | 감사 로그 무결성 |
| **DB Password** | `appsettings.json` | `"dev-password-12345"` | SQLCipher 암호화 |
| **bcrypt Cost** | `PasswordHasher.cs` | `12` | 해싱 강도 (~300ms) |

## 프로덕션 배포 체크리스트

다음 항목들을 **반드시** 환경변수로 교체하세요:

```json
// appsettings.Production.json
{
  "JwtOptions": {
    "SecretKey": "${JWT_SECRET_KEY}"
  },
  "AuditService": {
    "DefaultHmacKey": "${AUDIT_HMAC_KEY}"
  },
  "ConnectionStrings": {
    "HnVueDb": "Data Source={appDataPath}/hnvue.db; Password=${DB_PASSWORD};"
  }
}
```

## 암호화 표준

| 항목 | 표준 | 강도 | 설명 |
|------|------|------|------|
| **비밀번호** | bcrypt | cost=12 | ~300ms 해싱 시간, salt 포함 |
| **데이터베이스** | SQLCipher AES-256 | 256-bit | 모든 데이터 암호화 저장 |
| **JWT** | HS256 | 256-bit | 토큰 서명, 15분 만료 |
| **감사 로그** | HMAC-SHA256 | 256-bit | 체인 무결성, 변조 감지 |
| **코드 서명** | SHA-256 | 256-bit | 소프트웨어 업데이트 검증 |

## RBAC 4-Tier 계층 구조

```
Service (최고 권한)
  +-- Admin
  |   +-- 시스템 설정 변경
  |   +-- 사용자 관리
  |   +-- 감사 로그 조회
  |   +-- 모든 Radiologist 권한 포함
  |
  +-- Radiologist (의사)
  |   +-- 촬영 프로토콜 승인
  |   +-- 긴급 선량 Block 해제
  |   +-- 인시던트 리뷰
  |   +-- 모든 Radiographer 권한 포함
  |
  +-- Radiographer (기사)
      +-- 환자 등록
      +-- 촬영 실행
      +-- 영상 검토
      +-- 자신의 감사 로그만 조회
```

---

문서 최종 업데이트: 2026-04-09
