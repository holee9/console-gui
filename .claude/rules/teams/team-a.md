# Team A — Infrastructure & Foundation Rules

Shared rules: see `team-common.md` (Philosophy, Self-Verification, Git Protocol)

## Module Ownership
- HnVue.Common, HnVue.Data, HnVue.Security, HnVue.SystemAdmin, HnVue.Update
- tests/HnVue.Architecture.Tests/ (co-owned with QA: Team A implements, QA enforces)
- Root build config: global.json, nuget.config, Directory.Build.props, Directory.Packages.props

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

## Design Team 기능구현 분담 (Team A 담당분)

Design Team은 코더가 없으므로, 아래 인프라 항목은 Team A가 구현:

| 항목 | 현재 위치 | 올바른 위치 | 설명 |
|------|-----------|------------|------|
| MVVM 인프라 (ViewModelBase, RelayCommand) | HnVue.UI/Components/Common/ | HnVue.Common 또는 CommunityToolkit.Mvvm 통합 | 이미 CommunityToolkit.Mvvm 사용 중이므로 중복 제거 권장 |
| 공통 Domain Enum | HnVue.UI 내 인라인 정의 (DoseLevel 등) | HnVue.Common.Enums/ | 도메인 모델은 Common에 집중 |
| 공통 추상화 인터페이스 | HnVue.Common.Abstractions/ | 유지 | ISecurityService, IWorkflowEngine 등 |

## Issue Protocol
- Common interface changes: create issue with `breaking-change` label + notify Coordinator
- NuGet additions: create issue with `soup-update` label + notify RA team
- DB migration: create issue with `team-a` + `feat` labels
