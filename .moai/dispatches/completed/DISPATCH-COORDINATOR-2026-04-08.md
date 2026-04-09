# DISPATCH: Coordinator

Issued: 2026-04-08
Issued By: Main (MoAI Orchestrator)
Priority: P1-Critical

## How to Execute

When user says "지시서대로 작업해":
1. Read this entire document
2. Set Status to IN_PROGRESS
3. Execute each task in order
4. After each task, update its checkbox and add result notes
5. Run final build verification
6. Set Status to COMPLETE with summary

## Context

QA team completed local analyzer infrastructure migration (SonarCloud -> local Roslyn analyzers). Build passes with 0 errors but 9,378 warnings. Three issues need Coordinator attention:

1. PerformanceTests.cs has a flaky HoverEffect test (53ms > 50ms threshold) blocking CI green
2. AccessibilityTests.cs has duplicate test case ID warnings
3. AddPatientProcedureViewModel.cs has SCS0005 (Weak RNG) security warning

These are blocking Phase 2 team dispatches.

## Tasks

### Task 1: Fix HoverEffect Performance Test Threshold
- **Target files**: `tests/HnVue.UI.Tests/UI/PerformanceTests.cs`
- **Action**: Line 51 has `[InlineData(50, "HoverEffect")]`. Change threshold from 50 to 100 (hover effects are non-critical UI, 100ms is still imperceptible). This is a flaky boundary — 50ms is too tight for CI environment variance.
- **Acceptance criteria**: Test passes consistently. No other InlineData values changed.
- **Constraints**: Do NOT change SearchResults (500ms) or ButtonResponse (100ms) thresholds — those are UX-critical.

### Task 2: Fix AccessibilityTests Duplicate Test Case IDs
- **Target files**: `tests/HnVue.UI.Tests/UI/AccessibilityTests.cs`
- **Action**: Find duplicate test case IDs (xUnit warnings about duplicate test cases). Each test method or InlineData combination must produce a unique test case. Fix by making test names or parameters unique.
- **Acceptance criteria**: No duplicate test case warnings in test output.
- **Constraints**: Do NOT change test logic or remove test cases. Only fix naming/ID uniqueness.

### Task 3: Fix SCS0005 Weak RNG in AddPatientProcedureViewModel
- **Target files**: `src/HnVue.UI.ViewModels/ViewModels/AddPatientProcedureViewModel.cs`
- **Action**: Line 307 uses `System.Random` which triggers SCS0005. Evaluate the usage context:
  - If used for security/crypto: Replace with `System.Security.Cryptography.RandomNumberGenerator`
  - If used for non-security (UI IDs, display order): Add `#pragma warning disable SCS0005` with comment explaining non-crypto usage
- **Acceptance criteria**: SCS0005 warning eliminated for this file.
- **Constraints**: Do NOT change the behavior of the random value generation, only the source or suppression.

### Final: Build Verification
- **Action**: `dotnet build HnVue.sln --configuration Release`
- **Acceptance criteria**: 0 errors
- **Action**: `dotnet test tests/HnVue.UI.Tests/HnVue.UI.Tests.csproj`
- **Acceptance criteria**: All tests pass including the fixed PerformanceTests and AccessibilityTests

## Constraints

- DO NOT modify files outside Coordinator ownership (UI.Contracts, UI.ViewModels, App, IntegrationTests, shared test files)
- DO NOT modify any src/ production code except AddPatientProcedureViewModel.cs
- DO NOT upgrade packages

## Status

- **State**: PENDING
- **Started**: -
- **Completed**: -
- **Results**:
  - Task 1 (HoverEffect): -
  - Task 2 (AccessibilityTests): -
  - Task 3 (SCS0005): -
  - Build: -
  - Tests: -
