# DISPATCH: Design — 빌드 오류 수정 + UI 커버리지

Issued: 2026-04-10
Issued By: Main (MoAI Commander Center)
Priority: **P0-Blocker** (빌드 오류) + P2-High (커버리지)

## Design 역할 재확인 (.claude/rules/teams/team-design.md)

- **소유 모듈**: HnVue.UI (Views, Styles, Themes, Components, Converters, Assets, DesignTime)
- **금지 참조**: Data, Security, Workflow, Imaging, Dicom, Dose 등
- **코드 비하인드**: 순수 UI 이벤트만, 비즈니스 로직 금지
- **접근성**: WCAG 2.1 AA, 44x44px 터치 타겟, 키보드 네비게이션
- **PPT Scope**: Issue #59 — 지정 페이지 외 UI 구현 금지
- **스크린샷**: WindowHandle 캡처만 허용, Region 캡처 금지

## How to Execute

1. **Task 1 (P0)** — ConverterTests 빌드 오류 수정
2. Task 2-4 순서대로
3. 체크박스 + Status 업데이트

## Task 1: ConverterTests 빌드 오류 수정 (P0-Blocker)

**오류**: `TestStatus` 접근성 불일치 (CS0051)
**파일**: `tests/HnVue.UI.Tests/ConverterTests.cs`

**검증 기준**:
- [ ] HnVue.UI.Tests 빌드 오류 0건
- [ ] 기존 UI 테스트 통과

## Task 2: Converter 0% 클래스 테스트 (P1-Critical)

**12개 Converter**: 순수 변환 로직, Convert/ConvertBack + 경계값(null, empty, DependencyProperty.UnsetValue)

**검증 기준**:
- [ ] 12개 Converter 모두 70%+
- [ ] 빌드 + 테스트 통과

## Task 3: ThemeRollbackService 테스트 (P2-High)

**규칙**: MahApps.Metro 3테마(Light/Dark/HighContrast) 런타임 전환

**검증 기준**:
- [ ] ThemeRollbackService 70%+
- [ ] 빌드 + 테스트 통과

## Task 4: 저커버리지 Component (P3-Medium)

**대상**: RelayCommand(50%→80%), RelayCommand<T>(0%→70%), StatusBarItem(55%→75%)

**검증 기준**:
- [ ] HnVue.UI 전체 75%+
- [ ] 빌드 + 테스트 통과

## Final Verification [HARD — 이 섹션 미완료 시 COMPLETED 보고 금지]

1. 자기 모듈 빌드: `dotnet build` → 오류 0건
2. 자기 테스트: `dotnet test tests/HnVue.UI.Tests/` → 전원 통과
3. 전체 솔루션 빌드: `dotnet build HnVue.sln -c Release` → 결과 기록
4. 빌드 출력 요약을 Status에 복사

## Constraints

- HnVue.UI 외 파일 수정 금지
- 금지 모듈 참조 추가 금지
- PPT Scope 준수 (Issue #59)
- 스크린샷: WindowHandle 캡처만, Region 캡처 금지

## Git Completion Protocol [HARD]

1. git add (DISPATCH.md + 변경 파일)
2. git commit (conventional commit 형식)
3. git push origin team/team-design
4. PR 생성 (기존 open PR 확인 후 중복 방지)
5. PR URL을 Status에 기록

## Status

- **State**: NOT_STARTED
- **Build Evidence**: (미완료)
- **PR**: (미생성)
- **Results**: Task 1→PENDING, Task 2→PENDING, Task 3→PENDING, Task 4→PENDING
