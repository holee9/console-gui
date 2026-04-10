# Coordinator — Integration & UI Contracts Rules

## Module Ownership
- HnVue.UI.Contracts (interface gate)
- HnVue.UI.ViewModels (domain composition)
- HnVue.App (DI composition root)
- tests.integration/HnVue.IntegrationTests

## UI.Contracts Interface Management
- Coordinator is the SOLE modifier of UI.Contracts interfaces
- Any interface addition/modification requires impact analysis across all consumers
- Breaking changes require `interface-contract` issue + all affected team notification
- Use interface segregation — prefer small, focused interfaces

## DI Registration Standards (Microsoft.Extensions.DependencyInjection)
- All service registrations in App.xaml.cs or dedicated ServiceCollectionExtensions
- Use appropriate lifetimes: Singleton for stateless, Scoped for per-request, Transient for lightweight
- Missing DI registration = App startup failure — always verify with integration test
- New module integration: add registration + integration test in same PR

## ViewModel Composition
- ViewModels inject domain services through constructor injection
- ViewModels must NOT directly reference infrastructure (Data, Security internals)
- Use interfaces from UI.Contracts for all ViewModel dependencies
- Complex composition: use factories or builders, not service locator

## Integration Test Standards
- Every cross-module interaction must have integration test coverage
- Integration tests use real services (not mocks) with in-memory SQLite
- Test naming: {Module}_{Scenario}_{ExpectedResult}
- Run integration tests before every PR merge to main

## PR Review Responsibilities
- Review ALL PRs that touch UI.Contracts, UI.ViewModels, App
- Verify DI registration completeness
- Check for interface contract violations
- Ensure integration test coverage for new interactions

## Issue Protocol
- UI.Contracts changes: create issue with `interface-contract` label + notify all teams
- DI registration changes: create issue with `coordinator` label
- Architecture changes: notify RA team for SAD/SDS update

## Git Completion Protocol [HARD]

After completing DISPATCH tasks:
1. `git add` changed files (exclude secrets, temp files)
2. `git commit` with conventional commit format matching team prefix
3. `git push origin team/coordinator`
4. Create PR to main via Gitea API (check for existing open PR first to avoid duplicates)
5. Record PR URL in DISPATCH.md Status section

Push failure: report "PUSH_FAILED" status, do not block on git errors.
