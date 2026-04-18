# Team A Next Task Prep

Prepared: 2026-04-11
Next Dispatch: `.moai/dispatches/active/S04-R1-team-a.md`
Purpose: Session handoff and next-task startup context

## Current Session Closeout

- Team A dispatch report submitted.
- `DISPATCH.md` status is `COMPLETED`.
- No commit or push performed.
- Existing unrelated worktree changes were preserved.

## Relevant Dirty Files

Team A current-session files:
- `DISPATCH.md`
- `.moai/reports/team-a-dispatch-report-2026-04-11.md`
- `src/HnVue.Common/HnVue.Common.csproj`
- `src/HnVue.Data/HnVue.Data.csproj`
- `src/HnVue.Security/HnVue.Security.csproj`
- `src/HnVue.SystemAdmin/HnVue.SystemAdmin.csproj`
- `src/HnVue.Update/HnVue.Update.csproj`

Unrelated existing changes also remain in `.agency/`, `.claude/`, `.moai/config/`, `.gitignore`, and `CLAUDE.md`.

## Next Task Entry Points

- `src/HnVue.Common/Abstractions/IPhiEncryptionService.cs`
- `src/HnVue.Common/Configuration/HnVueOptions.cs`
- `src/HnVue.Security/Extensions/ServiceCollectionExtensions.cs`
- `src/HnVue.Data/Extensions/ServiceCollectionExtensions.cs`
- `src/HnVue.Data/HnVueDbContext.cs`
- `src/HnVue.Data/Entities/PatientEntity.cs`
- `src/HnVue.Data/Mappers/EntityMapper.cs`
- `src/HnVue.Data/Repositories/PatientRepository.cs`

## Findings Before Starting S04-R1

1. `IPhiEncryptionService` already exists in `HnVue.Common`.
2. AES-GCM `PhiEncryptionService` already exists in both `src/HnVue.Security/PhiEncryptionService.cs` and `src/HnVue.Data/Security/PhiEncryptionService.cs`.
3. `HnVueOptions` already has `PhiEncryptionKey`.
4. `HnVue.Security.Extensions.ServiceCollectionExtensions.AddPhiEncryption()` already registers `IPhiEncryptionService` from configuration.
5. `HnVue.Data.Extensions.ServiceCollectionExtensions.AddHnVueData()` currently registers `IPhiEncryptionService` with a random fallback key when no key is provided.
6. `PatientRepository` and `EntityMapper` already support optional PHI encryption/decryption via `IPhiEncryptionService`.
7. `PatientEntity` and `HnVueDbContext.OnModelCreating()` do not yet apply an EF Core value converter for PHI columns.
8. Search found no active `NullPhiEncryptionService` registration in Team A-owned files.

## Startup Risks

- The active dispatch assumes a greenfield PHI encryption implementation, but the repo already contains overlapping implementations.
- There is likely a design decision required between:
  - keep repository/mapper-level encryption only, or
  - move PHI encryption into EF Core value converters.
- `App.xaml.cs` is mentioned in the dispatch, but that file is not clearly Team A-owned. Prefer Team A-owned DI extensions first unless a Coordinator handoff is explicitly needed.
- The random fallback registration in `AddHnVueData()` may conflict with deterministic decryptability across sessions.

## Recommended First Actions

1. Compare `src/HnVue.Security/PhiEncryptionService.cs` and `src/HnVue.Data/Security/PhiEncryptionService.cs` and decide the single source of truth.
2. Confirm whether S04-R1 requires HKDF derivation from SQLCipher password, or whether `HnVue:PhiEncryptionKey` remains the canonical source.
3. Decide whether PHI encryption should happen in EF value converters or stay in `EntityMapper`/`PatientRepository`.
4. If DI changes are needed, prefer `HnVue.Security.Extensions.ServiceCollectionExtensions` or `HnVue.Data.Extensions.ServiceCollectionExtensions` before touching `App.xaml.cs`.
5. Add focused tests around deterministic decryption across process restarts and tamper detection before refactoring registrations.

## Ready Commands

```powershell
Get-Content .moai/dispatches/active/S04-R1-team-a.md
Select-String -Path src\**\*.cs -Pattern "IPhiEncryptionService|PhiEncryptionService|AddPhiEncryption|PatientRepository|EntityMapper|PhiEncryptionKey"
dotnet build src/HnVue.Security/HnVue.Security.csproj -c Release -t:Rebuild -v minimal
dotnet build src/HnVue.Data/HnVue.Data.csproj -c Release -t:Rebuild -v minimal
dotnet test tests/HnVue.Security.Tests/HnVue.Security.Tests.csproj -c Release --no-build -v minimal
dotnet test tests/HnVue.Data.Tests/HnVue.Data.Tests.csproj -c Release --no-build -v minimal
```
