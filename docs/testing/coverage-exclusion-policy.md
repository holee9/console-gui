# 커버리지 제외 정책 (Coverage Exclusion Policy)

**문서 번호:** DOC-QA-COV-001  
**버전:** 1.0  
**작성일:** 2026-04-11  
**담당:** QA Team  
**참조 SPEC:** SPEC-GOVERNANCE-001 (REQ-GOV-002)

---

## 1. 목적

이 문서는 HnVue 프로젝트의 코드 커버리지 측정에서 특정 코드를 제외하는 정책을 정의합니다.
제외 기준, 정당성, 승인 프로세스, 유효 커버리지 계산 공식을 포함합니다.

---

## 2. 제외 항목

### 2.1 WPF Views (Code-Behind)

**패턴:** `**/Views/*.xaml.cs`, `**/Components/**/*.xaml.cs`

**제외 대상:**
- `HnVue.UI/Views/*.xaml.cs`
- `HnVue.UI/Components/**/*.xaml.cs`
- `HnVue.UI/DesignTime/**/*.cs`

**정당성:**

WPF 코드비하인드 파일은 다음 이유로 커버리지 측정에서 제외합니다:

1. **UI 자동화 불가**: WPF UI 테스트는 FlaUI 같은 별도 UI 자동화 프레임워크를 필요로 하며, 표준 xUnit 단위 테스트로는 XAML 렌더링 파이프라인을 실행할 수 없습니다.
2. **코드비하인드 최소화 정책**: 팀 규칙 (team-design.md)에 따라 code-behind는 `InitializeComponent()`, ViewModel 생성자 주입, WPF 보안 제약 이벤트 핸들러만 포함합니다. 비즈니스 로직이 없으므로 커버리지 측정 의미가 없습니다.
3. **MVVM 패턴 준수**: 모든 테스트 가능한 로직은 ViewModel에 위치하며, ViewModel은 별도 테스트됩니다.

**대안 검증 방법:**
- DesignTime mock을 통한 VS2022 Designer 렌더링 확인
- UI 자동화 테스트 (`tests.e2e/` 디렉토리의 FlaUI 기반 E2E 테스트)

### 2.2 EF Core Migrations

**패턴:** `**/Migrations/**/*.cs`

**제외 대상:**
- `HnVue.Data/Migrations/*.cs`
- `**/Migrations/*Snapshot.cs`

**정당성:**

EF Core Migration 파일은 다음 이유로 제외합니다:

1. **자동 생성 코드**: EF Core tooling (`dotnet ef migrations add`)이 자동 생성하는 코드입니다. 수동 작성 코드가 아닙니다.
2. **인프라 코드**: 데이터베이스 스키마 변경 스크립트이며, 비즈니스 로직이 없습니다.
3. **통합 테스트로 검증**: Migration의 정확성은 통합 테스트(`tests.integration/`)에서 실제 데이터베이스 적용을 통해 검증합니다.

**대안 검증 방법:**
- `dotnet ef database update` 를 CI 파이프라인에서 실행하여 Migration 유효성 검증
- Integration test에서 In-memory SQLite로 schema 적용 확인

### 2.3 DesignTime Mock ViewModels

**패턴:** `**/DesignTime/**/*.cs`

**정당성:**
- VS2022 디자이너 전용 목 데이터. 실제 앱에서 실행되지 않습니다.

---

## 3. 제외 기준 및 승인 프로세스

### 3.1 제외 기준

새로운 코드를 커버리지에서 제외하려면 다음 기준을 모두 충족해야 합니다:

| 기준 | 설명 |
|------|------|
| 자동생성 코드 | tooling이 자동 생성한 코드 (EF Core, T4, Source Generator) |
| 비즈니스 로직 없음 | 테스트 가능한 도메인/비즈니스 로직이 없음 |
| 대안 검증 존재 | 다른 방법으로 품질 검증이 가능함 |
| 테스트 비용 과도 | UI 자동화 등 과도한 인프라를 요구함 |

### 3.2 승인 프로세스

1. **제안**: 팀 리드가 GitHub Issue 생성 (`qa-policy` 레이블)
2. **검토**: QA Lead + 관련 팀 리드 검토 (최소 2인 승인)
3. **문서화**: 이 문서에 새 섹션 추가
4. **CI 적용**: `coverage.runsettings` 또는 `Generate-CoverageReport.ps1` 업데이트

---

## 4. 유효 커버리지 계산 공식

### 4.1 정의

```
유효 커버리지(%) = (커버된 유효 라인 수 / 전체 유효 라인 수) × 100

유효 라인 수 = 전체 라인 수 - 제외된 라인 수 (Views + Migrations + DesignTime)
```

### 4.2 모듈별 목표

| 모듈 | 분류 | 목표 커버리지 |
|------|------|--------------|
| HnVue.Dose | Safety-Critical | ≥ 90% |
| HnVue.Incident | Safety-Critical | ≥ 90% |
| HnVue.Security | Safety-Critical | ≥ 90% |
| HnVue.Update | Safety-Critical | ≥ 90% |
| HnVue.Imaging | Safety-Adjacent | ≥ 85% |
| HnVue.Workflow | Safety-Adjacent | ≥ 85% |
| HnVue.Dicom | Standard | ≥ 85% |
| HnVue.Detector | Standard | ≥ 85% |
| HnVue.PatientManagement | Standard | ≥ 85% |
| HnVue.CDBurning | Standard | ≥ 85% |
| HnVue.Common | Standard | ≥ 85% |
| HnVue.Data | Standard | ≥ 85% |
| HnVue.SystemAdmin | Standard | ≥ 85% |
| HnVue.UI.ViewModels | Standard | ≥ 85% |
| HnVue.UI.Contracts | Standard | ≥ 85% |
| HnVue.UI | Excluded (Views) | N/A |

### 4.3 전체 솔루션 목표

- **유효 커버리지 전체 목표: 85% 이상**
- Views, Migrations, DesignTime 제외 후 계산
- `Generate-CoverageReport.ps1`에서 자동 계산

---

## 5. coverlet 제외 설정

### 5.1 coverage.runsettings 방법

```xml
<DataCollector friendlyName="XPlat Code Coverage">
  <Configuration>
    <ExcludeByFile>
      **/Views/*.xaml.cs,
      **/Components/**/*.xaml.cs,
      **/DesignTime/**/*.cs,
      **/Migrations/**/*.cs
    </ExcludeByFile>
    <ExcludeByAttribute>
      ExcludeFromCodeCoverage,
      GeneratedCodeAttribute,
      CompilerGeneratedAttribute
    </ExcludeByAttribute>
  </Configuration>
</DataCollector>
```

### 5.2 코드 수준 제외

자동 생성 코드 또는 예외적으로 제외가 필요한 경우:

```csharp
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(
    Justification = "WPF code-behind - UI automation tested via E2E")]
public partial class LoginView : MetroWindow
{
    // ...
}
```

---

## 6. 관련 문서

- `coverage.runsettings`: Coverlet 수집 설정
- `scripts/qa/Generate-CoverageReport.ps1`: 커버리지 리포트 생성 스크립트
- `team-common.md`: Quality Standards (단일 소스 of truth)
- DOC-012: Unit Test Plan
- DOC-022: UT Report

---

**문서 이력:**

| 버전 | 날짜 | 변경 내용 | 작성자 |
|------|------|----------|--------|
| 1.0 | 2026-04-11 | 최초 작성 (SPEC-GOVERNANCE-001 REQ-GOV-002) | QA Team |
