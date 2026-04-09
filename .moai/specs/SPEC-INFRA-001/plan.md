# Implementation Plan: SPEC-INFRA-001

## SPEC ID: SPEC-INFRA-001
## Priority: P1-Critical
## Development Mode: TDD

## Task Decomposition

### Phase 1: Security Critical Fixes (REQ-SEC-001 ~ REQ-SEC-003)

**Task 1.1: AuditService Chain Verification Fix** [REQ-SEC-001]
- File: `src/HnVue.Security/AuditService.cs`
- Change: Fix VerifyChainAsync to return failure on hash mismatch
- Test: Write reproduction test first (tampered chain → must fail)
- Dependencies: None

**Task 1.2: JWT Async Deadlock Fix** [REQ-SEC-002]
- File: `src/HnVue.Security/JwtTokenService.cs`
- Change: Replace `.GetAwaiter().GetResult()` with async/await
- Test: Verify async token validation works correctly
- Dependencies: None

**Task 1.3: PersistentTokenDenylist Error Handling** [REQ-SEC-003]
- File: `src/HnVue.Security/PersistentTokenDenylist.cs`
- Change: Log corruption, start with empty list
- Test: Corrupted file scenario
- Dependencies: None

### Phase 2: Data Layer Security (REQ-DATA-001 ~ REQ-DATA-003)

**Task 2.1: Replace NullPhiEncryptionService** [REQ-DATA-001]
- Files: `src/HnVue.Data/Security/`, `src/HnVue.Data/Extensions/`
- Change: Use real PhiEncryptionService from HnVue.Security
- Test: Verify encryption/decryption roundtrip
- Dependencies: None (can parallel with Phase 1)

**Task 2.2: Fix Audit Trail User Attribution** [REQ-DATA-002]
- Files: `src/HnVue.Data/Repositories/PatientRepository.cs`, `StudyRepository.cs`, `AuditRepository.cs`
- Change: Inject ISecurityContext, use actual user name
- Test: Verify audit entries contain authenticated user
- Dependencies: ISecurityContext thread safety (Task 4.1) recommended

**Task 2.3: Add Performance Indexes** [REQ-DATA-003]
- Files: `src/HnVue.Data/HnVueDbContext.cs`, new migration
- Change: Add HasIndex() calls, generate migration
- Test: Migration applies cleanly
- Dependencies: None

### Phase 3: Update Safety (REQ-UPDATE-001 ~ REQ-UPDATE-003)

**Task 3.1: Atomic Rollback Mechanism** [REQ-UPDATE-001]
- File: `src/HnVue.Update/SWUpdateService.cs`
- Change: Add state tracking, try-catch with cleanup, staged update
- Test: Failure-at-each-stage scenarios
- Dependencies: None

**Task 3.2: HTTPS Enforcement** [REQ-UPDATE-002]
- Files: `src/HnVue.Update/UpdateOptions.cs`, `UpdateChecker.cs`
- Change: Validate URL scheme in options and constructor
- Test: HTTP URL rejection
- Dependencies: None

**Task 3.3: Code Signing Chain Validation** [REQ-UPDATE-003]
- Files: `src/HnVue.Update/SignatureVerifier.cs`, `CodeSignVerifier.cs`, `UpdateOptions.cs`
- Change: Enable revocation checking, add timestamp validation, prevent disable in prod
- Test: Expired/revoked certificate rejection
- Dependencies: None

### Phase 4: Common & SystemAdmin (REQ-COMMON-001, REQ-COMMON-002, REQ-SYSADMIN-001)

**Task 4.1: ISecurityContext Thread Safety** [REQ-COMMON-001]
- Files: `src/HnVue.Common/Abstractions/ISecurityContext.cs`, security implementation
- Change: Add synchronization documentation, implement lock in concrete class
- Test: Concurrent access stress test
- Dependencies: None

**Task 4.2: ErrorCode Network Codes** [REQ-COMMON-002]
- File: `src/HnVue.Common/Results/ErrorCode.cs`
- Change: Add NetworkTimeout, CommunicationFailure, HardwareNoResponse, etc.
- Test: Verify new codes are accessible
- Dependencies: None

**Task 4.3: Settings Change Audit Logging** [REQ-SYSADMIN-001]
- File: `src/HnVue.SystemAdmin/SystemAdminService.cs`
- Change: Add IAuditRepository call in SaveSettingsAsync
- Test: Settings change produces audit entry
- Dependencies: None

### Phase 5: Final Verification
- Run full Team A test suite
- Build verification (Release mode)
- Coverage check

## Execution Order
- Phase 1 (Security Critical): First — safety-critical bug fixes
- Phase 2 (Data Security): Can run parallel with Phase 3 and Phase 4
- Phase 3 (Update Safety): Can run parallel with Phase 2 and Phase 4
- Phase 4 (Common/SysAdmin): Can run parallel with Phase 2 and Phase 3
- Phase 5 (Verification): After all phases complete

## MX Tag Plan
- `@MX:WARN` on AuditService.VerifyChainAsync (security-critical hash verification)
- `@MX:WARN` on JwtTokenService.ValidateTokenAsync (security-critical token validation)
- `@MX:WARN` on PersistentTokenDenylist.LoadAsync (security-critical denylist loading)
- `@MX:ANCHOR` on PhiEncryptionService (high fan_in, cross-module dependency)
- `@MX:NOTE` on SWUpdateService.ApplyUpdateAsync (update safety critical path)
- `@MX:WARN` on UpdateOptions.Validate (configuration security gate)

## Technical Constraints
- .NET 8, WPF, EF Core 9, SQLite/SQLCipher
- TDD: RED-GREEN-REFACTOR cycle for all changes
- No public API signature changes (internal implementation fixes only)
- All security-critical changes require 90%+ branch coverage
- Medical device IEC 62304 Class B compliance
