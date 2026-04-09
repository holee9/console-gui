# DISPATCH: Design — UI Coverage 71.4% → 75.0% (갭 3.6pp)

Issued: 2026-04-09
Issued By: Main (MoAI Orchestrator)
Priority: P2-High
Source: PHASE1_QA_COVERAGE_TARGETS_2026-04-09.md

## How to Execute

When user says "지시서대로 작업해":
1. Read this entire document
2. Execute tasks in priority order
3. Update Status section after each task
4. Run build verification as final step

## Context

QA Phase 1 확정 기준:
- HnVue.UI 목표: **75%+** (현재 71.4%, 갭 3.6pp)
- 기존 hard target 60%+ 이미 달성, QA working target 75%로 상향
- 핵심 갭: Converter 0% 다수, View 0% 다수, ThemeRollbackService 0%

## File Ownership

- HnVue.UI/Views/**
- HnVue.UI/Styles/**
- HnVue.UI/Themes/**
- HnVue.UI/Components/**
- HnVue.UI/Converters/**
- HnVue.UI/Assets/**
- HnVue.UI/DesignTime/**

## Tasks

### Task 1: Converter 0% 클래스 테스트 (P1-Critical, 빠른 커버리지 향상)

**0% 클래스 (12개)**:
- ActiveTabToVisibilityConverter
- CountToVisibilityConverter
- InverseBoolConverter
- InvertedBoolToVisibilityConverter
- MultiBoolAndConverter
- MultiBoolOrConverter
- NullToCollapsedConverter
- SafeStateToColorConverter
- StatusToBrushConverter
- StringEqualityToBoolConverter
- StringToVisibilityConverter
- (NullToVisibilityConverter 50% → 80%+)

**테스트 패턴**: Convert/ConvertBack 메서드, 경계값 (null, empty, 타입 불일치)

**검증 기준**:
- [ ] 12개 Converter 모두 70%+ 달성
- [ ] 빌드 + 테스트 통과

### Task 2: ThemeRollbackService 테스트 (P2-High)

**현재**: 0%

**테스트 대상**:
- 테마 변경 → 롤백 시나리오
- 롤백 스택 관리
- 잘못된 테마 설정 시 예외 처리

**검증 기준**:
- [ ] ThemeRollbackService 70%+
- [ ] 빌드 + 테스트 통과

### Task 3: 저커버리지 Component 보강 (P3-Medium)

**대상**:
- RelayCommand (50%) → 80%+
- RelayCommand<T> (0%) → 70%+
- StatusBarItem (55.2%) → 75%+

**검증 기준**:
- [ ] 3개 클래스 모두 목표 달성
- [ ] HnVue.UI 전체 75%+ 달성
- [ ] 빌드 + 테스트 통과

## Build Verification

```bash
dotnet build HnVue.sln --configuration Release
dotnet test HnVue.sln --configuration Release --no-build
```

## Status

- **State**: IN_PROGRESS
- **Started**: 2026-04-09
- **Completed**: -
- **Results**: Task 1 (Converter 테스트) 작성 중
