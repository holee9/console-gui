## Task Decomposition
SPEC: SPEC-INFRA-001

| Task ID | Description | Requirement | Dependencies | Planned Files | Status |
|---------|-------------|-------------|--------------|---------------|--------|
| T-001 | AuditService 해시 체인 검증 논리 수정 | REQ-SEC-001 | T-004 | src/HnVue.Security/AuditService.cs, tests/HnVue.Security.Tests/AuditServiceTests.cs | pending |
| T-002 | JWT 토큰 검증 비동기 교착상태 수정 | REQ-SEC-002 | T-004 | src/HnVue.Security/JwtTokenService.cs, tests/HnVue.Security.Tests/JwtTokenServiceTests.cs | pending |
| T-003 | PersistentTokenDenylist 파일 손상 처리 개선 | REQ-SEC-003 | - | src/HnVue.Security/PersistentTokenDenylist.cs, tests/HnVue.Security.Tests/PersistentTokenDenylistTests.cs | pending |
| T-004 | ISecurityContext 스레드 안전성 보장 | REQ-COMMON-001 | - | src/HnVue.Common/Abstractions/ISecurityContext.cs, tests/ | pending |
| T-005 | ErrorCode 네트워크/통신 오류 코드 추가 | REQ-COMMON-002 | - | src/HnVue.Common/Results/ErrorCode.cs, tests/ | pending |
| T-006 | NullPhiEncryptionService 실제 암호화 교체 | REQ-DATA-001 | T-004 | src/HnVue.Data/Security/, src/HnVue.Data/Extensions/, src/HnVue.Data/Mappers/, tests/HnVue.Data.Tests/ | pending |
| T-007 | 감사 로그 사용자 속성 ISecurityContext 연동 | REQ-DATA-002 | T-004, T-006 | src/HnVue.Data/Repositories/*.cs, tests/ | pending |
| T-008 | 데이터베이스 성능 인덱스 추가 | REQ-DATA-003 | - | src/HnVue.Data/HnVueDbContext.cs, src/HnVue.Data/Migrations/ | pending |
| T-009 | 원자적 업데이트 롤백 메커니즘 | REQ-UPDATE-001 | - | src/HnVue.Update/SWUpdateService.cs, tests/HnVue.Update.Tests/ | pending |
| T-010 | 업데이트 서버 HTTPS 강제 | REQ-UPDATE-002 | - | src/HnVue.Update/UpdateOptions.cs, src/HnVue.Update/UpdateChecker.cs, tests/ | pending |
| T-011 | 코드 서명 체인/해지 검증 강화 | REQ-UPDATE-003 | T-010 | src/HnVue.Update/SignatureVerifier.cs, src/HnVue.Update/CodeSignVerifier.cs, src/HnVue.Update/UpdateOptions.cs, tests/ | pending |
| T-012 | 설정 변경 감사 로깅 | REQ-SYSADMIN-001 | T-004 | src/HnVue.SystemAdmin/SystemAdminService.cs, tests/HnVue.SystemAdmin.Tests/ | pending |
