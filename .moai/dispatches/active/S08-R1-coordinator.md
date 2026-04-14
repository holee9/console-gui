# DISPATCH: Coordinator — S08 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center |
| **대상** | Coordinator |
| **브랜치** | team/coordinator |
| **유형** | S08 R1 — StudylistViewModel + DI 등록 |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S08-R1-coordinator.md)만 Status 업데이트.

---

## 컨텍스트

S08-R1은 StudylistView (PPT slides 5-7) 구현 라운드.
Coordinator는 StudylistViewModel 생성, UI.Contracts 인터페이스 정의, DI 등록 담당.
Design Team이 XAML 구현을 병렬 진행. Coordinator는 ViewModel을 먼저 완성하여 Design이 바인딩 가능하도록 지원.

---

## 사전 확인

```bash
git checkout team/coordinator
git pull origin main
```

---

## Task 1 (P1): IStudylistViewModel 인터페이스 정의

StudylistView에 필요한 ViewModel 인터페이스를 UI.Contracts에 정의.

**수행 사항**:
- PPT slides 5-7 분석 → 필요한 속성/커맨드 식별
- `IStudylistViewModel.cs` 인터페이스 생성 (UI.Contracts)
- 스터디 목록, 필터링, 정렬, 환자 선택 관련 프로퍼티/커맨드 정의
- 기존 IPatientListViewModel 패턴 참조

**목표**: IStudylistViewModel 인터페이스 완성

---

## Task 2 (P1): StudylistViewModel 구현

IStudylistViewModel 구현체 생성.

**수행 사항**:
- `StudylistViewModel.cs` 생성 (UI.ViewModels)
- CommunityToolkit.Mvvm [ObservableProperty] / [RelayCommand] 사용
- 스터디 데이터 로드, 필터링, 정렬 로직
- DesignTime Mock ViewModel 생성 (DesignTime/)

**목표**: StudylistViewModel 구현 완료

---

## Task 3 (P2): DI 등록 + 통합테스트

App.xaml.cs에 StudylistViewModel DI 등록.

**수행 사항**:
- ServiceCollectionExtensions에 IStudylistViewModel → StudylistViewModel 등록
- 통합테스트: DI 해결 확인
- 통합테스트: ViewModel 기본 동작 확인

**목표**: DI 등록 + 통합테스트 2건 이상

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI.Contracts/ src/HnVue.UI.ViewModels/ src/HnVue.App/ tests.integration/
git commit -m "feat(coordinator): StudylistViewModel + DI 등록 + 통합테스트 (#issue)"
git push origin team/coordinator
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: IStudylistViewModel 인터페이스 (P1) | COMPLETED | 2026-04-14 | 기존 구현 완성됨 (45 lines, all props/commands) |
| Task 2: StudylistViewModel 구현 (P1) | COMPLETED | 2026-04-14 | 기존 구현 완성 + DesignTime Mock 추가 |
| Task 3: DI 등록 + 통합테스트 (P2) | COMPLETED | 2026-04-14 | 기존 DI 등록 확인 + 통합테스트 2건 추가 (SWR-COORD-070) |
| Git 완료 프로토콜 | IN_PROGRESS | | 빌드 0에러, 통합테스트 55/55 통과 |

**빌드 증뢰**:
- `MSBuild HnVue.sln`: 0 errors, warnings only (기존 StyleCop)
- `dotnet test`: 55/55 passed (53 기존 + 2 신규 StudylistViewModel)
- 신규 파일: `DesignTimeStudylistViewModel.cs`, `CoordinatorIntegrationTests.cs` Scenario 8 (2 tests)
