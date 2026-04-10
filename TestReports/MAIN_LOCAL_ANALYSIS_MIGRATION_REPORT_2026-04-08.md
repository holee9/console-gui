# Main 작업 사용자 보고

작성일: 2026-04-08
대상: `main` 작업 사용자
작성 주체: QA worktree (`team/qa`)

## 1. 작업 목적

기존 SonarCloud 기반 정적 분석 구성을 제거하고, `dotnet build` 및 `dotnet test`에 직접 통합되는 완전 로컬, 무상(static + dynamic) 분석 체계로 전환했습니다.

이번 작업 범위는 인프라 변경에 한정했으며, `src/**/*.cs` 소스 코드는 수정하지 않았습니다.

## 2. 반영 완료 항목

### 2.1 Analyzer 패키지 추가

중앙 패키지 관리 파일에 아래 analyzer 버전을 추가했습니다.

- `StyleCop.Analyzers` `1.2.0-beta.556`
- `Roslynator.Analyzers` `4.12.9`
- `SecurityCodeScan.VS2019` `5.6.7`

적용 파일:

- `Directory.Packages.props`

### 2.2 전 프로젝트 공통 analyzer 참조

모든 프로젝트가 별도 설정 없이 analyzer를 사용하도록 공통 `PackageReference`를 추가했습니다.

- `StyleCop.Analyzers`
- `Roslynator.Analyzers`
- `SecurityCodeScan.VS2019`

모든 참조는 `PrivateAssets="all"`로 적용했습니다.

적용 파일:

- `Directory.Build.props`

### 2.3 .NET 기본 analyzer 활성화

공통 빌드 설정에 다음 속성을 반영했습니다.

- `EnableNETAnalyzers=true`
- `AnalysisLevel=latest-recommended`
- `EnforceCodeStyleInBuild=true`
- `TreatWarningsAsErrors=false`

적용 파일:

- `Directory.Build.props`

### 2.4 로컬 dotnet tool 매니페스트 구성

로컬 분석용 도구 매니페스트를 생성하고 아래 도구를 설치했습니다.

- `dotnet-reportgenerator-globaltool`
- `dotnet-stryker`
- `dotnet-outdated-tool`

적용 파일:

- `.config/dotnet-tools.json`

### 2.5 로컬 분석 스크립트 추가

`scripts/qa/Invoke-LocalAnalysis.ps1`를 신규 작성했습니다.

지원 모드:

- `All`
- `Build`
- `Test`
- `Coverage`
- `Security`

스크립트 기능:

1. 솔루션 restore
2. `dotnet build` 실행
3. `dotnet test` 실행
4. 커버리지 수집 및 `reportgenerator` HTML 리포트 생성
5. `dotnet-outdated` 및 `dotnet list package --vulnerable` 기반 보안 점검
6. JSON 요약 파일 출력

적용 파일:

- `scripts/qa/Invoke-LocalAnalysis.ps1`

### 2.6 SonarCloud 제거

기존 SonarCloud workflow를 삭제했습니다.

삭제 파일:

- `.github/workflows/sonar.yml`

### 2.7 Desktop CI 연동

기존 desktop CI workflow에 아래를 반영했습니다.

- QA 스크립트 경로와 `.config/dotnet-tools.json`을 트리거/캐시 키에 포함
- 빌드 후 `Invoke-LocalAnalysis.ps1 -Mode Coverage` 실행
- `TestReports/coverage/**`를 artifact로 업로드

적용 파일:

- `.github/workflows/desktop-ci.yml`

## 3. 검증 결과

### 3.1 Restore / Build

실행:

- `dotnet restore HnVue.sln`
- `dotnet build HnVue.sln --configuration Release --no-restore -v minimal`

결과:

- 빌드 성공
- 오류 `0`

빌드 로그:

- `TestReports/build-release.log`

### 3.2 Analyzer 동작 확인

Release build 로그 기준 고유 warning 수는 총 `9,378`건입니다.

분류:

- `StyleCop` (`SA*`): `8,194`
- `Roslynator` (`RCS*`): `4`
- `SecurityCodeScan` (`SCS*`): `4`

따라서 세 analyzer 모두 실제 빌드에 통합되어 동작하는 것을 확인했습니다.

### 3.3 Security 점검 확인

로컬 보안 점검 스크립트는 정상 동작했습니다.

산출물:

- `TestReports/local-security/security/dotnet-outdated.txt`
- `TestReports/local-security/security/vulnerable-packages.txt`

기존 전이 의존성 기준으로 High 취약점이 검출되었습니다. 대표 예시는 아래와 같습니다.

- `Microsoft.Extensions.Caching.Memory 8.0.0`
- `System.Text.Json 8.0.0`

이 항목들은 이번 인프라 전환으로 새로 생긴 문제가 아니라, 기존 의존성 상태가 가시화된 결과입니다.

### 3.4 Coverage 실행 확인

실행:

- `.\scripts\qa\Invoke-LocalAnalysis.ps1 -Mode Coverage -Configuration Release -OutputDirectory 'TestReports\local-coverage'`

결과:

- 커버리지 수집 성공
- HTML 리포트 생성 성공
- 커버리지 요약 생성 성공
- 최종 종료 코드는 실패

실패 사유는 스크립트 오류가 아니라 기존 테스트 실패 1건 때문입니다.

커버리지 요약:

- Line coverage: `75.6%`
- Branch coverage: `65.7%`
- Method coverage: `58.9%`

산출물:

- `TestReports/local-coverage/coverage/index.html`
- `TestReports/local-coverage/coverage/Summary.txt`
- `TestReports/local-coverage/local-analysis-summary.json`

## 4. 현재 남은 이슈

### 4.1 기존 테스트 실패 1건

`Coverage` 모드 실패 원인은 아래 기존 테스트입니다.

- `tests/HnVue.UI.Tests/UI/PerformanceTests.cs`
- `ResponseTime_ShouldMeetTarget(targetMs: 50, operation: "HoverEffect")`
- 측정값 `53ms`, 기준 `50ms`

즉, 인프라 전환 자체는 완료되었고, 잔여 이슈는 제품 테스트 안정성에 속합니다.

### 4.2 중복 테스트 케이스 경고

아래 테스트에서 duplicate test case ID 경고가 확인되었습니다.

- `tests/HnVue.UI.Tests/UI/AccessibilityTests.cs`

이 역시 인프라 문제가 아니라 기존 테스트 정의 이슈입니다.

## 5. 최종 판단

이번 QA 작업의 목표였던 "SonarCloud 제거 후 로컬 빌드 통합 analyzer 체계 전환"은 완료되었습니다.

충족 항목:

- analyzer 패키지 중앙 등록 완료
- 전 프로젝트 공통 analyzer 참조 완료
- .NET analyzer 활성화 완료
- 로컬 dotnet tool manifest 구성 완료
- 로컬 분석 스크립트 작성 완료
- SonarCloud workflow 제거 완료
- desktop CI에 coverage 실행 및 artifact 업로드 연동 완료
- Release build 성공 및 오류 0 확인 완료
- warning category 집계 완료

남은 작업은 인프라 전환이 아니라, 기존 테스트 실패 1건과 기존 패키지 취약점 정리입니다.

## 6. 권고 후속 조치

1. `HoverEffect` 성능 테스트 임계값 또는 구현을 재검토해 `Coverage` 파이프라인을 green 상태로 복구
2. `AccessibilityTests` 중복 케이스 정리
3. `Microsoft.Extensions.Caching.Memory`, `System.Text.Json` 등 취약 전이 의존성 업그레이드
4. 이후 각 팀에 analyzer warning 정리 작업 분배
