# Research: Team A Module Improvements & Bug Fixes

## Date: 2026-04-09

## Methodology
Parallel deep analysis of 5 Team A modules via dedicated Explore agents.

## Module Summaries

### HnVue.Common
- **Critical**: ISecurityContext lacks thread synchronization (race condition risk)
- **Critical**: Audit trail user attribution hardcoded as "system" in PatientRepository
- **High**: ISecurityService violates Interface Segregation (auth+RBAC+PIN+lockout)
- **Medium**: ErrorCode missing network/communication error codes
- **Medium**: Missing Supervisor role in UserRole enum
- **Medium**: PatientRecord/ExposureParameters lack input validation

### HnVue.Data
- **Critical**: NullPhiEncryptionService is a no-op (PHI not encrypted)
- **High**: Missing DoseRepository, ImageRepository implementations
- **High**: No performance indexes on frequently queried columns
- **Medium**: No transaction management for multi-repository operations
- **Medium**: Missing pagination, date-range queries in repositories
- **Medium**: Missing mappers for DoseRecord, Image, UpdateHistory

### HnVue.Security
- **CRITICAL**: AuditService.cs:89-95 returns Result.Success(false) when hash chain is broken — tampering goes undetected
- **HIGH**: JwtTokenService.cs:104-106 uses .GetAwaiter().GetResult() causing potential deadlocks
- **HIGH**: PersistentTokenDenylist.cs:102-109 silently ignores file corruption — revoked tokens reinstated
- **Medium**: BCrypt work factor 12 (should be 14+ for medical devices)
- **Medium**: No account lockout auto-unlock mechanism
- **Low**: No timing attack protection on password verification

### HnVue.Update
- **High**: No atomic rollback — partial update failures leave undefined state
- **High**: No HTTPS enforcement for update server URL
- **High**: Code signing verification missing chain validation and timestamp checking
- **Medium**: No backup integrity verification (checksums)
- **Medium**: Allows disabling signature verification in production

### HnVue.SystemAdmin
- **High**: No audit logging for settings changes
- **Medium**: No settings change notification mechanism
- **Medium**: Missing import functionality for settings
- **Low**: Missing hardware/calibration/logging settings categories
- **Low**: Settings duplication between SystemSettings and HnVueOptions

## Cross-Cutting Concerns
1. Audit trail integrity is compromised at multiple levels (hash chain logic, user attribution, settings changes)
2. PHI data protection is not implemented (NullPhiEncryptionService)
3. Update safety mechanisms are incomplete for medical device deployment
4. Thread safety issues in security-critical code paths

## Reference Implementations Found
- HnVue.Security/PhiEncryptionService.cs — real AES-256-GCM implementation exists, but Data layer uses NullPhiEncryptionService
- HnVue.Data/Repositories/AuditRepository.cs — proper audit pattern can be extended to SystemAdmin
- HnVue.Data/Repositories/PatientRepository.cs — soft-delete and audit trail pattern reusable for new repositories
