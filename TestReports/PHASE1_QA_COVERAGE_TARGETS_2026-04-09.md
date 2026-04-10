# Phase 1: QA 커버리지 갭 분석

작성일: 2026-04-09
작성 주체: QA worktree
대상: Team A / Team B / Team Design / Coordinator / QA

## 1. 결론

Phase 1 커버리지 갭 분석의 팀별 정확한 목표치는 아래로 확정합니다.

- 전사 공통 interim 목표: 전체 line coverage `80%+`
- 전사 최종 release 목표: 전체 line coverage `85%+`
- 안전성-중요 모듈 hard gate: branch coverage `90%+`

이번 문서에서 말하는 "팀별 목표치"는 두 종류로 나뉩니다.

1. 기존 문서에 이미 명시된 hard target
2. 기존 문서에 없는 영역에 대해 QA가 이번 Phase 1 갭 분석에서 확정한 working target

## 2. 기준 출처

### 2.1 기존 hard target

기존 프로젝트 문서에 명시된 기준은 그대로 유지합니다.

- 전체 코드 커버리지: `85%+`
- 안전 임계 모듈: `90%+`
- `HnVue.Update`: `85%+`
- `HnVue.Security`: `90%+`
- `HnVue.Data`: `80%+`
- `HnVue.Dicom`: `80%+`
- `HnVue.PatientManagement`: `80%+`
- `HnVue.SystemAdmin`: `80%+`
- `HnVue.CDBurning`: `80%+`
- `HnVue.Detector`: `85%+`
- `HnVue.UI`: `60%+`
- Team B 안전성-중요 모듈 (`HnVue.Dose`, `HnVue.Incident`) branch coverage: `90%+`

### 2.2 팀 소유 범위

팀 소유 범위는 운영 문서 기준으로 다음과 같이 봅니다.

- Team A: `HnVue.Common`, `HnVue.Data`, `HnVue.Security`, `HnVue.SystemAdmin`, `HnVue.Update`
- Team B: `HnVue.Dicom`, `HnVue.Detector`, `HnVue.Imaging`, `HnVue.Dose`, `HnVue.Incident`, `HnVue.Workflow`, `HnVue.PatientManagement`, `HnVue.CDBurning`
- Team Design: `HnVue.UI`
- Coordinator: `HnVue.UI.Contracts`, `HnVue.UI.ViewModels`, `HnVue.App`

## 3. 현재 기준선

최근 로컬 coverage summary 기준 line coverage는 다음과 같습니다.

| 모듈 | 현재 line coverage |
|---|---:|
| HnVue.Common | 94.1% |
| HnVue.Data | 100.0% |
| HnVue.Security | 86.2% |
| HnVue.SystemAdmin | 66.6% |
| HnVue.Update | 77.3% |
| HnVue.Dicom | 66.9% |
| HnVue.Detector | 42.6% |
| HnVue.Imaging | 87.5% |
| HnVue.Dose | 67.6% |
| HnVue.Incident | 94.2% |
| HnVue.Workflow | 91.4% |
| HnVue.PatientManagement | 72.7% |
| HnVue.CDBurning | 100.0% |
| HnVue.UI | 71.4% |
| HnVue.UI.Contracts | 42.8% |
| HnVue.UI.ViewModels | 42.0% |

전체 현재치:

- line coverage `75.6%`
- branch coverage `65.7%`
- method coverage `58.9%`

## 4. 팀별 확정 목표치

## 4.1 Team A

### 확정 목표

- 팀 평균 line coverage: `85%+`
- 모듈별 floor:
  - `HnVue.Common`: `90%+`
  - `HnVue.Data`: `85%+`
  - `HnVue.Security`: `90%+`
  - `HnVue.SystemAdmin`: `80%+`
  - `HnVue.Update`: `85%+`

### 현재 대비 갭

| 모듈 | 현재 | 목표 | 갭 |
|---|---:|---:|---:|
| HnVue.Common | 94.1% | 90.0% | 0.0pp |
| HnVue.Data | 100.0% | 85.0% | 0.0pp |
| HnVue.Security | 86.2% | 90.0% | 3.8pp |
| HnVue.SystemAdmin | 66.6% | 80.0% | 13.4pp |
| HnVue.Update | 77.3% | 85.0% | 7.7pp |

팀 평균:

- 현재 `84.8%`
- 목표 `85.0%`
- 평균 갭 `0.2pp`

### Team A 우선순위

1. `HnVue.SystemAdmin`
2. `HnVue.Update`
3. `HnVue.Security`

## 4.2 Team B

### 확정 목표

- 팀 평균 line coverage: `85%+`
- 모듈별 floor:
  - `HnVue.Dicom`: `80%+`
  - `HnVue.Detector`: `85%+`
  - `HnVue.Imaging`: `85%+`
  - `HnVue.Dose`: `90%+`
  - `HnVue.Incident`: `90%+`
  - `HnVue.Workflow`: `90%+`
  - `HnVue.PatientManagement`: `80%+`
  - `HnVue.CDBurning`: `80%+`
- hard gate:
  - `HnVue.Dose` branch coverage `90%+`
  - `HnVue.Incident` branch coverage `90%+`

### 현재 대비 갭

| 모듈 | 현재 | 목표 | 갭 |
|---|---:|---:|---:|
| HnVue.Dicom | 66.9% | 80.0% | 13.1pp |
| HnVue.Detector | 42.6% | 85.0% | 42.4pp |
| HnVue.Imaging | 87.5% | 85.0% | 0.0pp |
| HnVue.Dose | 67.6% | 90.0% | 22.4pp |
| HnVue.Incident | 94.2% | 90.0% | 0.0pp |
| HnVue.Workflow | 91.4% | 90.0% | 0.0pp |
| HnVue.PatientManagement | 72.7% | 80.0% | 7.3pp |
| HnVue.CDBurning | 100.0% | 80.0% | 0.0pp |

팀 평균:

- 현재 `77.9%`
- 목표 `85.0%`
- 평균 갭 `7.1pp`

### Team B 우선순위

1. `HnVue.Detector`
2. `HnVue.Dose`
3. `HnVue.Dicom`
4. `HnVue.PatientManagement`

## 4.3 Team Design

### 확정 목표

기존 문서상 `HnVue.UI`는 `60%+`가 최소 기준이지만, 현재 커버리지 보고서를 보면 0%인 View/Converter/Service가 많이 남아 있어 이번 Phase 1 갭 분석에서는 더 높은 작업 목표를 적용합니다.

- `HnVue.UI` line coverage: `75%+`
- `HnVue.UI.QA.Tests`: 전체 green 유지
- 접근성/반응성/UI 회귀 테스트: pass rate `100%`

### 현재 대비 갭

| 모듈 | 현재 | 목표 | 갭 |
|---|---:|---:|---:|
| HnVue.UI | 71.4% | 75.0% | 3.6pp |

### Team Design 우선순위

1. Converter 0% 구간
2. View 0% 구간
3. `ThemeRollbackService`

## 4.4 Coordinator

기존 문서에는 `HnVue.UI.Contracts`, `HnVue.UI.ViewModels`, `HnVue.App`에 대한 정식 커버리지 수치가 명시돼 있지 않습니다. 따라서 아래 값은 이번 QA Phase 1 갭 분석에서 확정하는 working target입니다.

### 확정 목표

- `HnVue.UI.Contracts`: `70%+`
- `HnVue.UI.ViewModels`: `75%+`
- Coordinator 평균 line coverage: `72.5%+`
- `HnVue.App`: 전용 module coverage 수치 대신 integration evidence로 관리
  - 신규 또는 보강 integration scenario `6개 이상`
  - `HnVue.IntegrationTests` green 유지

### 현재 대비 갭

| 모듈 | 현재 | 목표 | 갭 |
|---|---:|---:|---:|
| HnVue.UI.Contracts | 42.8% | 70.0% | 27.2pp |
| HnVue.UI.ViewModels | 42.0% | 75.0% | 33.0pp |

팀 평균:

- 현재 `42.4%`
- 목표 `72.5%`
- 평균 갭 `30.1pp`

### Coordinator 우선순위

1. `HnVue.UI.ViewModels`
2. `HnVue.UI.Contracts`
3. `HnVue.App` integration scenario 보강

## 4.5 QA

QA는 소스 모듈 coverage owner가 아니라 gate owner로 봅니다.

### 확정 목표

- 주간 coverage trend report 발행: `100%`
- 팀별 current/target/gap 표 유지: `100%`
- PR 전 전체 line coverage baseline: `80%+` 확인
- release 전 전체 line coverage gate: `85%+` 확인
- 안전성-중요 branch gate (`Dose`, `Incident`): `90%+` 확인

## 5. 최종 확정값 요약

| 팀 | 현재 평균 | 확정 목표 | 평균 갭 |
|---|---:|---:|---:|
| Team A | 84.8% | 85.0% | 0.2pp |
| Team B | 77.9% | 85.0% | 7.1pp |
| Team Design | 71.4% | 75.0% | 3.6pp |
| Coordinator | 42.4% | 72.5% | 30.1pp |

## 6. 해석

이번 Phase 1 기준에서 가장 급한 팀은 Coordinator와 Team B입니다.

- Coordinator는 `UI.Contracts`와 `UI.ViewModels`가 둘 다 40%대라 가장 큰 갭을 가집니다.
- Team B는 평균 갭이 크고, 특히 `Detector`, `Dose`, `Dicom`, `PatientManagement`가 집중 보강 대상입니다.
- Team A는 평균은 거의 목표에 근접했지만 `SystemAdmin`, `Update`, `Security`의 개별 floor를 아직 못 맞췄습니다.
- Team Design은 최소 기준은 이미 넘지만, 0% 구간이 넓어 이번 Phase 1에서는 `75%+`로 상향 관리합니다.

## 7. 최종 결정

다음 값을 Phase 1 QA 커버리지 갭 분석의 공식 팀별 목표치로 확정합니다.

1. Team A 평균 `85%+`
2. Team B 평균 `85%+`
3. Team Design `HnVue.UI 75%+`
4. Coordinator 평균 `72.5%+`
5. 전체 interim gate `80%+`
6. 전체 release gate `85%+`
7. 안전성-중요 branch gate `90%+`
