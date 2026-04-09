# DISPATCH: Team A — Infrastructure & Foundation

Issued: 2026-04-08
Issued By: Main (MoAI Orchestrator)
Priority: P2-High

## How to Execute

When user says "지시서대로 작업해":
1. Read this entire document
2. Set Status to IN_PROGRESS
3. Execute each task in order
4. After each task, update its checkbox and add result notes
5. Run final build verification
6. Set Status to COMPLETE with summary

## Context

QA team completed local analyzer infrastructure migration. Three Roslyn analyzers now active: StyleCop.Analyzers, Roslynator.Analyzers, SecurityCodeScan.VS2019.

Security scan found HIGH vulnerabilities in transitive dependencies:
- `Microsoft.Extensions.Caching.Memory 8.0.0`
- `System.Text.Json 8.0.0`

These are existing vulnerabilities newly visible via local scanning. Team A owns NuGet package management.

Additionally, SA* (StyleCop) warnings need reduction in Team A owned modules. Total project-wide SA* count is 8,194 — Team A should address warnings in their owned modules only.

## Tasks

### Task 1: Upgrade Vulnerable Transitive Dependencies
- **Target files**: `Directory.Packages.props`
- **Action**: Bump versions of vulnerable packages to latest stable:
  - `Microsoft.Extensions.Caching.Memory` → latest 9.x
  - `System.Text.Json` → latest 9.x
  - Check all `Microsoft.Extensions.*` packages for consistent version alignment
  - Run `dotnet list package --vulnerable` after upgrade to verify resolution
- **Acceptance criteria**: `dotnet list package --vulnerable` returns no HIGH or CRITICAL vulnerabilities in direct or transitive dependencies.
- **Constraints**: Do NOT remove packages. Do NOT downgrade any package. Ensure all Microsoft.Extensions.* stay on same major version.

### Task 2: Fix StyleCop Warnings in HnVue.Common
- **Target files**: `src/HnVue.Common/**/*.cs`
- **Action**: Fix SA* warnings. Focus on high-frequency categories first (SA1600 missing docs, SA1101 prefix local calls with this). Use bulk approaches where safe:
  - SA1101: If project convention is no `this.` prefix, suppress via `.editorconfig` or `GlobalSuppressions.cs`
  - SA1600/SA1633: Add XML doc comments to public members, or suppress file headers globally if not required
  - Other SA*: Fix individually
- **Acceptance criteria**: Reduced SA* warning count for HnVue.Common module. Document before/after count.
- **Constraints**: Do NOT change method signatures or behavior. Do NOT add empty XML doc comments (`/// <summary></summary>`) — either write meaningful docs or suppress.

### Task 3: Fix StyleCop Warnings in HnVue.Data
- **Target files**: `src/HnVue.Data/**/*.cs`
- **Action**: Same approach as Task 2 for HnVue.Data module.
- **Acceptance criteria**: Reduced SA* warning count for HnVue.Data. Document before/after count.
- **Constraints**: Same as Task 2.

### Task 4: Fix StyleCop Warnings in HnVue.Security
- **Target files**: `src/HnVue.Security/**/*.cs`
- **Action**: Same approach as Task 2 for HnVue.Security module. Extra care with security code — review each change for correctness.
- **Acceptance criteria**: Reduced SA* warning count for HnVue.Security. Document before/after count.
- **Constraints**: Same as Task 2. EXTRA: Do NOT modify any cryptographic logic, password hashing, or JWT token handling code.

### Task 5: Fix StyleCop Warnings in HnVue.SystemAdmin and HnVue.Update
- **Target files**: `src/HnVue.SystemAdmin/**/*.cs`, `src/HnVue.Update/**/*.cs`
- **Action**: Same approach as Task 2 for remaining Team A modules.
- **Acceptance criteria**: Reduced SA* warning count. Document before/after count.
- **Constraints**: Same as Task 2.

### Final: Build Verification
- **Action**: `dotnet build HnVue.sln --configuration Release`
- **Acceptance criteria**: 0 errors. Document total warning count (before vs after).
- **Action**: `dotnet test tests/HnVue.Common.Tests/ tests/HnVue.Data.Tests/ tests/HnVue.Security.Tests/ tests/HnVue.SystemAdmin.Tests/ tests/HnVue.Update.Tests/`
- **Acceptance criteria**: All Team A tests pass.

## Constraints

- DO NOT modify files outside Team A ownership (Common, Data, Security, SystemAdmin, Update)
- DO NOT modify test files (warning fixes are for src/ only)
- DO NOT change public API signatures
- NuGet version changes ONLY in Directory.Packages.props

## Status

- **State**: COMPLETE
- **Started**: 2026-04-09
- **Completed**: 2026-04-09
- **Results**:
  - Task 1 (Vulnerable deps): DONE - All Microsoft.Extensions.* 8.0.0→9.0.0, System.Text.Json→9.0.0. 0 HIGH/CRITICAL vulnerabilities.
  - Task 2 (Common SA*): N/A - StyleCop analyzer not yet integrated in this branch (0 SA warnings baseline)
  - Task 3 (Data SA*): N/A - Same as Task 2
  - Task 4 (Security SA*): N/A - Same as Task 2
  - Task 5 (SystemAdmin+Update SA*): N/A - Same as Task 2
  - Build: errors=0, warnings=1124 (pre-existing, no SA* in Team A modules)
  - Tests: 440/440 passed (Common 82 + Data 102 + Security 148 + SystemAdmin 30 + Update 78)
