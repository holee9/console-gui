# DISPATCH: {Team Name}

Issued: {YYYY-MM-DD}
Issued By: Main (MoAI Orchestrator)
Priority: {P1-Critical | P2-High | P3-Medium | P4-Low}

## How to Execute

When user says "지시서대로 작업해":
1. Read this entire document
2. Set Status to IN_PROGRESS
3. Execute each task in order
4. After each task, update its checkbox and add result notes
5. Run final build verification
6. Set Status to COMPLETE with summary

## Context

{Background information: what happened before, why this work is needed, dependencies on other teams}

## Tasks

### Task 1: {Title}
- **Target files**: {exact relative paths}
- **Action**: {specific instruction}
- **Acceptance criteria**: {how to verify success}
- **Constraints**: {what NOT to do}

### Task 2: {Title}
...

### Final: Build Verification
- **Action**: `dotnet build HnVue.sln --configuration Release`
- **Acceptance criteria**: 0 errors, warning count documented
- **Action**: `dotnet test` (for owned test projects only)
- **Acceptance criteria**: All tests pass

## Constraints

- DO NOT modify files outside team ownership
- DO NOT upgrade packages unless explicitly listed
- DO NOT change public interfaces without Coordinator approval

## Status

- **State**: PENDING
- **Started**: -
- **Completed**: -
- **Results**:
  - Task 1: -
  - Task 2: -
  - Build: -
  - Tests: -
