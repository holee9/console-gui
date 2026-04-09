# SPEC-INFRA-001 Compact — Team A 인프라 모듈 기능개선 및 버그픽스

## Requirements Summary

| ID | Module | Priority | Description |
|----|--------|----------|-------------|
| REQ-SEC-001 | Security | Critical | AuditService 해시 체인 검증 논리 수정 (변조 미탐지 버그) |
| REQ-SEC-002 | Security | High | JWT 토큰 검증 .GetAwaiter().GetResult() 교착상태 수정 |
| REQ-SEC-003 | Security | High | PersistentTokenDenylist 파일 손상 시 무조용 처리 수정 |
| REQ-DATA-001 | Data | Critical | NullPhiEncryptionService → 실제 AES-256-GCM 암호화 교체 |
| REQ-DATA-002 | Data | High | 감사 로그 사용자 "system" 하드코딩 → ISecurityContext 연동 |
| REQ-DATA-003 | Data | Medium | PatientEntity, StudyEntity, DoseRecordEntity, AuditLogEntity 인덱스 추가 |
| REQ-UPDATE-001 | Update | High | 원자적 업데이트 롤백 메커니즘 구현 |
| REQ-UPDATE-002 | Update | High | 업데이트 서버 HTTPS 강제 |
| REQ-UPDATE-003 | Update | Medium | 코드 서명 체인/해지/타임스탬프 검증 강화 |
| REQ-COMMON-001 | Common | High | ISecurityContext 스레드 안전성 보장 |
| REQ-COMMON-002 | Common | Medium | ErrorCode 네트워크/통신 오류 코드 추가 |
| REQ-SYSADMIN-001 | SystemAdmin | High | 설정 변경 시 감사 로그 생성 |

## Acceptance Criteria (Key Scenarios)

- 해시 체인 변조 시 Result.IsSuccess == false 반환 (SEC-001)
- JWT 검증에 .GetAwaiter().GetResult() 없음 (SEC-002)
- 거부목록 파일 손상 시 빈 목록 + 로깅 (SEC-003)
- PHI 암호화 라운드트립 정상 (DATA-001)
- 감사 로그에 실제 사용자명 기록 (DATA-002)
- 인덱스 마이그레이션 오류 없음 (DATA-003)
- 업데이트 실패 시 이전 버전 복원 (UPDATE-001)
- HTTP URL 거부 (UPDATE-002)
- 만료/해지 인증서 거부 (UPDATE-003)
- 다중 스레드 접근 시 예외 없음 (COMMON-001)

## Files to Modify

- src/HnVue.Security/AuditService.cs, JwtTokenService.cs, PersistentTokenDenylist.cs
- src/HnVue.Data/Security/, Extensions/, Mappers/, Repositories/, HnVueDbContext.cs
- src/HnVue.Update/SWUpdateService.cs, UpdateOptions.cs, UpdateChecker.cs, SignatureVerifier.cs, CodeSignVerifier.cs
- src/HnVue.Common/Abstractions/ISecurityContext.cs, Results/ErrorCode.cs
- src/HnVue.SystemAdmin/SystemAdminService.cs

## Exclusions

- MFA 구현, 새 설정 카테고리, DoseRepository/ImageRepository 신규 구현, ISecurityService 분리, BCrypt work factor 변경
