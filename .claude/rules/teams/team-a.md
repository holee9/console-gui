# Team A — Infrastructure & Foundation Rules

## Module Ownership
- HnVue.Common, HnVue.Data, HnVue.Security, HnVue.SystemAdmin, HnVue.Update

## EF Core Migration Standards
- Migration naming: `YYYYMMDD_DescriptiveName` (e.g., `20260408_AddAuditLogIndex`)
- Always create both Up() and Down() methods
- Test migrations with `dotnet ef database update` before PR
- Notify Coordinator before any schema change

## SQLCipher Security Requirements
- AES-256 encryption key must come from secure configuration, never hardcoded
- Use `PRAGMA key` immediately after connection open
- Connection string must use `Password=` parameter for SQLCipher

## Repository Pattern Standards
- All repositories implement interfaces from HnVue.Common
- Use async methods (Task<T>) for all data access
- Include CancellationToken parameter in all async methods
- Use IUnitOfWork for transactional operations

## Security Code Standards
- bcrypt work factor: minimum 12 rounds
- JWT tokens: HS256 signing, configurable expiry
- Audit log entries: HMAC-SHA256 hash chain integrity
- Password validation: minimum 8 chars, complexity rules from HnVue.Security.PasswordPolicy

## NuGet Package Management
- All package versions in Directory.Packages.props (Central Package Management)
- New package addition requires: security review + SOUP list (DOC-033) RA team notification
- Remove unused packages promptly

## Issue Protocol
- Common interface changes: create issue with `breaking-change` label + notify Coordinator
- NuGet additions: create issue with `soup-update` label + notify RA team
- DB migration: create issue with `team-a` + `feat` labels
