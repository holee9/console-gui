# DISPATCH: S04 R2 — Team Design (UI/UX)

Issued: 2026-04-11
Issued By: Main (MoAI Commander Center)
Sprint: S04 Round 2
SPEC: SPEC-UI-001 (Draft → 부분 구현)
Priority: P1-High

## Objective

1. UI.QA 디자인 준수 13건 실패 수정 (P1)
2. StudylistView UISPEC-003 구현 (PPT Slides 5-7)

## SPEC Reference

- `.moai/specs/SPEC-UI-001/spec.md`
- `docs/design/spec/UISPEC-003_StudylistView.md` (있으면 참고)

## Tasks

### T1: UI.QA 테스트 13건 실패 수정 (P1-긴급)

**테스트 프로젝트**: `tests/HnVue.UI.QA.Tests/`

현재 상태: 52 테스트 중 13건 실패

작업 방법:
1. 먼저 `tests/HnVue.UI.QA.Tests/` 디렉토리에서 실패 테스트 목록 확인
2. 각 실패 테스트의 기대 조건 분석
3. 해당 XAML/스타일 수정
4. 테스트 재실행으로 통과 확인

수정 원칙:
- 디자인 토큰(CoreTokens.xaml) 준수
- MahApps.Metro 기본 스타일 상속
- IEC 62366 안전 색상 사용 (Safe=#00C853, Warning=#FFD600, Emergency=#D50000)
- WCAG 2.1 AA 명암비 4.5:1 이상

### T2: StudylistView UISPEC-003 구현 (PPT Slides 5-7) (REQ-UI-003)

**파일**: `src/HnVue.UI/Views/StudylistView.xaml`

PPT Slides 5-7 범위 엄수:
- [HARD] PPT 지정 페이지(5-7) 외 UI 요소 구현 절대 금지
- [HARD] 썸네일 스트립, 이미지 뷰어 패널은 WorkflowView(Acquisition) 전용 — 절대 포함 금지
- [HARD] 구현 후 PPT 페이지와 1:1 비교 검증 필수

구현 내용:
- StudylistView 레이아웃 PPT 디자인에 맞게 조정
- 데이터 그리드 컬럼 정의
- 필터/검색 영역
- 상태 표시 (완료/진행중/대기)

### T3: PPT Scope Compliance 검증 (REQ-UI-010)

모든 구현 완료 후:
1. 각 XAML 요소를 PPT 페이지와 1:1 비교
2. 범위 외 요소가 없는지 확인
3. 결과를 DISPATCH.md Status에 기록

## PPT Scope Boundary [HARD]

| Slides | View | 허용 범위 |
|--------|------|-----------|
| 5-7 | StudylistView.xaml | 스터디리스트 화면만 |

금지 항목:
- 썸네일 스트립/이미지 뷰어 (WorkflowView 전용)
- 로그인 화면 요소 (LoginView 전용)
- 환자 등록 요소 (AddPatientProcedureView 전용)
- 설정 화면 요소 (SettingsView 전용)

## Build Verification [HARD]

```bash
dotnet build HnVue.sln --no-incremental
dotnet test HnVue.sln --filter "FullyQualifiedName~HnVue.UI.QA" --no-build
```

**게이트**: 0 에러, UI.QA 13건 실패 → 0건

## Git Protocol [HARD]

1. `git add` 관련 파일만
2. `git commit -m "fix(design): UI.QA 13건 수정 + StudylistView UISPEC-003 PPT 5-7 구현"`
3. `git push origin team/team-design`
4. PR 생성
5. PR URL을 DISPATCH.md Status에 기록

## Status

- **State**: COMPLETED
- **Assigned**: Team Design
- **PR**: http://10.11.1.40:7001/DR_RnD/Console-GUI/pulls/80
- **Started**: 2026-04-11
- **Completed**: 2026-04-11

### Build Evidence

```
dotnet build HnVue.sln → 0 에러 (integration test 기존 오류는 Team A 범위, Design 변경 무관)
dotnet test --filter "HnVue.UI.QA" → 통과! - 실패: 0, 통과: 65, 건너뜀: 0, 전체: 65
```

### Task Results

- T1 (UI.QA 실패 수정): COMPLETED — 52/65 → 65/65 (13건 수정)
  - CoreTokens.xaml: BackgroundPage/Panel/Card 색상 PPT Slide 4 스펙 (#242424/#2A2A2A/#3B3B3B)
  - LoginView.xaml: SectionBadge, UppercaseLabel, CancelButton/PrimaryButton 완전 재구현
  - PatientListView.xaml: SectionBadge(Worklist), EmergencyBadge, OutlineButton, ListBoxItem H=44
- T2 (StudylistView UISPEC-003): COMPLETED — PPT 5-7 구조 확인 및 동적 리소스 수정
- T3 (PPT Scope Compliance): COMPLETED — StudylistView 슬라이드 5-7 요소만, 위반 없음

### Commit

- `ad71b2871d46677ffabcf42bd20bb6b4d371488e` on branch `team/team-design`
