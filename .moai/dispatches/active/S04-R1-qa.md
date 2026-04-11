# DISPATCH: QA — S04 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-11 |
| **발행자** | Main (MoAI Orchestrator) |
| **대상** | QA 팀 |
| **브랜치** | team/qa |
| **유형** | S04 Round 1 — 커버리지 정책 공식화 + Architecture 테스트 + S04 Gate Report |
| **우선순위** | P0 (Views/Migrations 제외 정책), P1 (S04 Gate Report), P2 (Architecture 테스트) |
| **SPEC 참조** | SPEC-GOVERNANCE-001 |
| **Gitea API** | http://10.11.1.40:7001/api/v1 (repo: drake.lee/Console-GUI) |

---

## 실행 방법

이 문서 전체를 읽고 Task 순서대로 실행하라.
- Task 1 (P0) → Task 2 (P1) → Task 3 (P2)
- 각 Task 완료 후 Status 섹션 업데이트 필수

---

## 컨텍스트

S03 완료 후 전체 커버리지 73.4%로 80% 게이트에 미달.
그러나 WPF Views(0%)와 EF Core Migrations(0%)를 제외하면 ~85% 추정.

이 "제외 정책"이 공식 문서화되지 않아 S04 진입 게이트가 CONDITIONAL 상태임.
QA가 이 정책을 공식화하고 S04 Gate Report를 발행하여 S04 진입을 확정해야 함.

---

## 파일 소유권

```
coverage.runsettings
scripts/qa/
scripts/ci/
.github/workflows/
TestReports/
tests/HnVue.Architecture.Tests/  (없으면 신규)
```

---

## Task 1 (P0): Views/Migrations 커버리지 제외 정책 공식화

### 배경

S03 DISPATCH.md에서 "Views/Migrations 제외 시 ~85%"라고 추정했으나,
이 정책이 coverage.runsettings나 공식 문서에 반영되지 않음.

### 실행

**1. coverage.runsettings 업데이트**

```xml
<!-- 제외 대상 추가 -->
<Exclude>
  <!-- 기존 제외 항목 유지 -->
  *[HnVue.UI]HnVue.UI.Views*  <!-- WPF Views: XAML 코드비하인드 -->
  *[HnVue.Data]HnVue.Data.Migrations*  <!-- EF Core 자동 생성 마이그레이션 -->
</Exclude>
```

**2. 제외 정책 근거 문서화**

S04 Gate Report에 다음 내용 포함:
- WPF Views: XAML 코드비하인드는 UI 자동화 없이 단위테스트 불가. DesignTime VM + UI.QA 테스트로 대체 검증 중
- EF Core Migrations: 자동 생성 코드로 커버리지 측정 대상에서 제외하는 것이 업계 표준

### 수용 기준

- coverage.runsettings에 제외 규칙 명시
- 제외 후 공식 커버리지 수치 측정 및 기록
- 제외 전/후 수치 비교 테이블 작성

---

## Task 2 (P1): S04 입장 게이트 보고서 발행

### 실행

`TestReports/S04_GATE_REPORT_2026-04-11.md` 생성:

```markdown
# S04 진입 게이트 보고서

## 1. S03 최종 상태
- 빌드: 0 에러
- 테스트: 2039 통과 / 1 flaky (RelayCommand)
- 커버리지: 73.4% (제외 전), ~85% (제외 후)

## 2. S04 진입 게이트 평가
| 게이트 | 기준 | 결과 | 판정 |
|--------|------|------|------|
| 빌드 에러 | 0 | 0 | PASS |
| Safety-Critical | 90%+ | Dose 99.4%, Security 95.5% | PASS |
| 전체 커버리지 (제외 후) | 80%+ | ~85% | PASS |
| UI.QA | 0F | 13F (Design S04에서 수정) | CONDITIONAL |
| 커버리지 제외 정책 | 문서화 | Task 1에서 공식화 | PENDING |

## 3. 이월 항목
1. UI.QA 13F → Design S04 R1
2. RelayCommand flaky → Design S04 R1
3. 커버리지 제외 정책 → QA S04 R1 Task 1

## 4. S04 진입 판정
CONDITIONAL PASS — 이월 항목이 S04 R1에서 해결될 예정.
```

### 수용 기준

- 보고서 파일 존재
- S03 최종 상태 정확 반영
- S04 진입 판정 명확

---

## Task 3 (P2): Architecture 테스트 실행

### 배경

SPEC-UI-001에서 "아키텍처 독립성 선언은 있으나 강제 메커니즘 없음"이 갭으로 식별됨.
NetArchTest를 사용한 아키텍처 규칙 검증 필요.

### 실행

**1. HnVue.Architecture.Tests 프로젝트** (없으면 신규 생성)

```csharp
// ArchitectureTests.cs
[Fact]
public void HnVue_UI_ShouldNotReference_HnVue_Data()
{
    // HnVue.UI는 HnVue.Data를 직접 참조하면 안 됨
    var result = Types.InAssembly(typeof(HnVue.UI.App).Assembly)
        .ShouldNot()
        .HaveDependencyOn("HnVue.Data")
        .GetResult();
    result.IsSuccessful.Should().BeTrue();
}

[Fact]
public void HnVue_UI_ShouldNotReference_HnVue_Security()
{
    var result = Types.InAssembly(typeof(HnVue.UI.App).Assembly)
        .ShouldNot()
        .HaveDependencyOn("HnVue.Security")
        .GetResult();
    result.IsSuccessful.Should().BeTrue();
}
// ... (UI → 모든 비즈니스 모듈 직접 참조 금지)
```

**2. 전체 아키텍처 규칙 테스트**

| 규칙 | 설명 |
|------|------|
| UI → Data 금지 | Views가 직접 DbContext 참조 불가 |
| UI → Security 금지 | Views가 직접 보안 서비스 참조 불가 |
| UI → Workflow 금지 | Views가 직접 워크플로 참조 불가 |
| UI는 Contracts만 참조 | UI는 UI.Contracts 인터페이스만 사용 |

### 수용 기준

- HnVue.Architecture.Tests 프로젝트 빌드 성공
- 아키텍처 규칙 테스트 4개 이상 작성
- 현재 코드베이스에서 전원 PASS (또는 위반 발견 시 보고)

---

## 빌드 검증

```bash
dotnet build HnVue.sln
dotnet test --filter "ArchitectureTests"
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add coverage.runsettings
git add scripts/qa/
git add TestReports/S04_GATE_REPORT_2026-04-11.md
git add tests/HnVue.Architecture.Tests/  # 신규 시
git commit -m "feat(qa): S04 gate report + coverage exclusion policy + architecture tests (SPEC-GOVERNANCE-001)"
git push origin team/qa
# PR 생성
```

---

## Status (작업 후 업데이트)

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: 커버리지 제외 정책 공식화 | NOT_STARTED | -- | -- |
| Task 2: S04 Gate Report 발행 | NOT_STARTED | -- | -- |
| Task 3: Architecture 테스트 | NOT_STARTED | -- | -- |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |

### 빌드 검증 결과

```
dotnet build: ?
Architecture 테스트: ?
제외 후 공식 커버리지: ?
```
