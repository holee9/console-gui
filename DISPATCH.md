# DISPATCH: Design — 오염 정리 + 빌드 오류 + UI 커버리지

Issued: 2026-04-10
Issued By: Main (MoAI Commander Center)
Priority: **P0-Blocker** (오염/빌드) + P2-High (커버리지)
Supersedes: 이전 DISPATCH (IN_PROGRESS, 체크 0/7)

## Design 역할 재확인 (rules/teams/team-design.md)

- **소유 모듈**: HnVue.UI (Views, Styles, Themes, Components, Converters, Assets, DesignTime)
- **금지 참조**: Data, Security, Workflow, Imaging, Dicom, Dose 등
- **코드 비하인드**: 순수 UI 이벤트만, 비즈니스 로직 금지
- **접근성**: WCAG 2.1 AA, 44x44px 터치 타겟, 키보드 네비게이션
- **PPT Scope**: Issue #59 — 지정 페이지 외 UI 구현 금지

## How to Execute

1. **Task 1 (P0)부터** — temp_ppt_extract/ 제거
2. **Task 2 (P0)** — ConverterTests 빌드 오류 수정
3. Task 3-5 순서대로
4. 체크박스 + Status 업데이트

## Task 1: temp_ppt_extract/ 오염 제거 (P0-Blocker)

**문제**: PPT 추출 임시파일 2,329개(14MB) 커밋됨
**수행**: `git rm -r temp_ppt_extract/` + `.gitignore` 추가

**검증 기준**:
- [ ] temp_ppt_extract/ 삭제됨
- [ ] .gitignore에 패턴 추가됨

## Task 2: ConverterTests 빌드 오류 수정 (P0-Blocker)

**오류**: `TestStatus` 접근성 불일치 (CS0051)
**파일**: `tests/HnVue.UI.Tests/ConverterTests.cs`

**검증 기준**:
- [ ] HnVue.UI.Tests 빌드 오류 0건
- [ ] 기존 UI 테스트 통과

## Task 3: Converter 0% 클래스 테스트 (P1-Critical)

**12개 Converter**: 순수 변환 로직, Convert/ConvertBack + 경계값(null, empty, DependencyProperty.UnsetValue)

**검증 기준**:
- [ ] 12개 Converter 모두 70%+
- [ ] 빌드 + 테스트 통과

## Task 4: ThemeRollbackService 테스트 (P2-High)

**규칙**: MahApps.Metro 3테마(Light/Dark/HighContrast) 런타임 전환

**검증 기준**:
- [ ] ThemeRollbackService 70%+
- [ ] 빌드 + 테스트 통과

## Task 5: 저커버리지 Component (P3-Medium)

**대상**: RelayCommand(50%→80%), RelayCommand<T>(0%→70%), StatusBarItem(55%→75%)

**검증 기준**:
- [ ] HnVue.UI 전체 75%+
- [ ] 빌드 + 테스트 통과

## Constraints

- HnVue.UI 외 파일 수정 금지
- 금지 모듈 참조 추가 금지
- PPT Scope 준수 (Issue #59)

## Status

- **State**: NOT_STARTED
- **Results**: Task 1→PENDING, Task 2→PENDING, Task 3→PENDING, Task 4→PENDING, Task 5→PENDING
