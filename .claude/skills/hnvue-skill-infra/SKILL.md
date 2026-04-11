---
name: hnvue-skill-infra
description: >
  HnVue Team A infrastructure engineering skill. Encodes Result<T> monad patterns, EF Core+SQLCipher
  data access, JWT/bcrypt security with HMAC-SHA256 audit chain, repository pattern with async/CancellationToken,
  and staged update mechanism. Loaded by hnvue-infra agent for Common, Data, Security, SystemAdmin, Update modules.
  Triggers on: EF Core migration, SQLCipher, repository, JWT, bcrypt, audit, NuGet, password policy, Result monad.
user-invocable: false
metadata:
  version: "1.0.0"
  category: "domain"
  status: "active"
  updated: "2026-04-11"
  tags: "hnvue, infrastructure, ef-core, security, sqlcipher, audit"

# MoAI Extension: Progressive Disclosure
progressive_disclosure:
  enabled: true
  level1_tokens: 100
  level2_tokens: 4500

# MoAI Extension: Triggers
triggers:
  keywords: ["ef core", "migration", "sqlcipher", "repository", "jwt", "bcrypt", "audit", "nuget", "password", "result monad", "security service", "update service"]
  agents: ["hnvue-infra"]
---

# HnVue Infrastructure Engineering Skill

Senior-level domain knowledge for HnVue infrastructure modules (Common, Data, Security, SystemAdmin, Update).

## 1. Result<T> Monad Pattern

All service methods return `Result<T>` or `Result` (not exceptions) for domain errors.

**ErrorCode ranges (150+ codes):**
- General: 0xxx | Security: 2xxx | Data: 3xxx | Workflow: 4xxx | DICOM: 5xxx | Update: 7xxx

**Composition pattern:**
```
Result<T> → Map(transform) → Bind(nextOperation) → Match(onSuccess, onFailure)
```

**Rules:**
- Never throw exceptions for expected domain failures — return Result.Failure(ErrorCode.Xxx)
- Exceptions reserved for truly exceptional situations (infrastructure failures, null guards)
- All public service methods return Result<T> or Task<Result<T>>
- ErrorCode selection: use the most specific code for the failure domain

## 2. EF Core + SQLCipher Data Layer

**DbContext: HnVueDbContext**
- 7 DbSets: Patients, Studies, Images, DoseRecords, Users, AuditLogs, UpdateHistories
- SQLite with optional SQLCipher AES-256 encryption
- `PRAGMA key` immediately after connection open
- Design-time factory: HnVueDbContextFactory for EF CLI tooling

**Migration naming:** `YYYYMMDD_DescriptiveName` (e.g., 20260408_AddAuditLogIndex)
- Always implement both Up() and Down()
- Test with `dotnet ef database update` before PR

**Cascade rules (IEC 62304 integrity):**
- Restrict: Patient->Studies, Study->DoseRecords (prevent accidental deletion)
- Cascade: Study->Images (orphan cleanup)

**Repository pattern:**
- All repos implement interfaces from HnVue.Common.Abstractions
- Async methods with CancellationToken parameter on every method
- Return Result<T> not raw entities
- IUnitOfWork for transactional operations

## 3. Security Module (IEC 62304 Class B)

**SecurityService (9 methods):**
- bcrypt: cost factor 12 (~300ms per hash for brute-force resistance)
- JWT: HS256 signing with JTI for revocation, configurable expiry
- Key rotation: PreviousSecretKey fallback in JwtTokenService
- Account lockout: 5 failed attempts; Quick PIN lockout: 3 failures (5-min duration)

**Password policy (compiled Regex, 100ms timeout for ReDoS prevention):**
- 8+ chars, 1 uppercase, 1 lowercase, 1 digit, 1 special char
- Quick PIN: exactly 4-6 digits

**Audit chain (tamper-evident):**
- HMAC-SHA256 hash chain: each entry references prior hash
- VerifyChainIntegrity detects modifications
- Critical incidents get `CRITICAL_INCIDENT` audit tag
- Fire-and-forget audit writes (non-blocking)

**RBAC: 4 roles**
- Radiographer -> Radiologist -> Admin = Service
- 9 permission constants (ViewPatients, ConfigureSystem, ApplySoftwareUpdate, etc.)

**Token denylist:**
- ITokenDenylist: RevokeAsync/IsRevokedAsync for JTI-based revocation
- InMemoryTokenDenylist (dev), PersistentTokenDenylist (prod)

## 4. SystemAdmin Module

**SystemAdminService:**
- GetSettings: 5-min cache for performance
- UpdateSettings: validates + audits all field-level changes (old -> new diffs)
- ExportAuditLog: CSV with RFC 4180 escaping for regulatory compliance

**Validation rules:**
- DICOM port: 1-65535
- AE Title: required non-empty
- Session timeout: >= 1 minute
- Max failed logins: >= 1

## 5. Update Module (IEC 62304 section 6.2.5)

**SWUpdateService staged update flow:**
1. CheckUpdateAsync (queries update server)
2. ApplyUpdateAsync: SHA-256 verify -> Authenticode verify (optional) -> backup -> stage
3. State tracking: InProgress -> Staged -> Completed/Failed/RolledBack
4. RollbackAsync: restore from backup + audit write

**CodeSignVerifier:** SHA-256 hash from sidecar file, 81KB async FileStream buffer
**BackupManager:** CreateBackupAsync, RestoreFromBackupAsync, GetLatestBackupPath
**Atomic rollback:** UpdateFailedException triggers automatic rollback on verification failures

## 6. Testing Patterns

**Frameworks:** xUnit, NSubstitute, FluentAssertions, Coverlet
**Key patterns:**
- Result<T> assertions: `result.IsSuccess.Should().BeTrue()`
- Mock repositories via NSubstitute
- Audit chain integrity verification tests
- JWT expiry/rotation scenarios
- In-memory SQLite for data integration tests
- Table-driven test patterns preferred
- [Trait("SWR", "SWR-xxx")] for requirement tracing

## 7. Cross-Module Protocol

- Common interface changes -> notify Coordinator (breaking-change label)
- NuGet additions -> notify RA (soup-update label) + update DOC-033 SOUP
- DB schema changes -> notify Coordinator before migration
- Security module changes -> notify RA for DOC-049 IEC81001 update

## 8. Quality Enforcement Protocol [HARD]

Before writing any code, read `${CLAUDE_SKILL_DIR}/references/infra-patterns.md` for:
- Pre-implementation checklist (MUST complete before coding)
- Code templates with correct/anti-pattern examples
- Post-implementation verification script (MUST run before COMPLETED)

**Implementation flow:**
1. Read references/infra-patterns.md Pre-Implementation Checklist
2. Write code following the correct patterns (anti-patterns are explicitly listed)
3. Run Post-Implementation Verification Script (all 4 steps)
4. Only report COMPLETED with build evidence
