# Coordinator — Integration & UI Contracts Rules

Shared rules: see `team-common.md` (Philosophy, Self-Verification, Git Protocol)

## Module Ownership
- HnVue.UI.Contracts (interface gate)
- HnVue.UI.ViewModels (domain composition)
- HnVue.App (DI composition root)
- tests.integration/ (all integration test projects)
- HnVue.sln (solution file structure)
- scripts/team/ (team worktree initialization)
- docs/architecture/ (co-owned with RA)

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

## Design Team 기능구현 분담 (Coordinator 담당분)

Design Team은 코더가 없으므로, 아래 항목은 Coordinator가 구현:

| 항목 | 예시 파일 | 이유 |
|------|-----------|------|
| 모든 ViewModel | *ViewModel.cs | 도메인 서비스 주입 + 비즈니스 로직 조합 |
| UI 서비스 클래스 | ThemeRollbackService.cs | 앱 생명주기 연동 |
| Modal/Toast C# 로직 | Modal.xaml.cs, Toast.xaml.cs | IDialogService 연동 |
| UI.Contracts 인터페이스 | ILoginViewModel.cs 등 | 인터페이스 게이트 관리 |
| DI Registration | App.xaml.cs | 서비스 등록 |

Design Team이 DISPATCH에서 `NEEDS_VIEWMODEL` 을 보고하면:
1. Coordinator가 필요한 ViewModel 속성/커맨드를 UI.Contracts에 추가
2. Coordinator가 ViewModel 구현
3. Design Team은 XAML 바인딩으로 연동

## Integration Test Standards
- Every cross-module interaction must have integration test coverage
- Integration tests use real services (not mocks) with in-memory SQLite
- Test naming: {Module}_{Scenario}_{ExpectedResult}
- Run integration tests before every PR merge to main

## Scope Limitation [HARD]
- Coordinator worktree only inspects its OWN modules
- Analyzing other teams' work is Commander Center's role

## PR Review Responsibilities
- Review ALL PRs that touch UI.Contracts, UI.ViewModels, App
- Verify DI registration completeness
- Check for interface contract violations
- Ensure integration test coverage for new interactions

## Issue Protocol
- UI.Contracts changes: create issue with `interface-contract` label + notify all teams
- DI registration changes: create issue with `coordinator` label
- Architecture changes: notify RA team for SAD/SDS update
