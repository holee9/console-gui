# Main 브랜치 사용자 보고서

작성일: 2026-04-09
작성 주체: `team/qa` worktree
대상: `main` 브랜치 사용자

## 1. 스냅샷과 적용 범위

- 현재 분석 작업 위치: `team/qa` @ `8d8295a`
- 현재 `main`: `aee89c4`
- `team/qa`는 `main` 대비 `0`커밋 앞서고 `10`커밋 뒤처져 있습니다. 즉 현재 `main`이 더 최신입니다.
- 다만 `HEAD..main -- src` 비교 결과, 차이는 `README`, `appsettings.json`, `MainWindow.xaml`, `LoginView.xaml`, `PatientListView.xaml`, 테마 토큰 등 UI/문서 중심이며, 모듈 추가/삭제나 DI 구성 변경은 확인되지 않았습니다.

따라서 이 보고서는 `team/qa` 기준 소스 구조를 분석하되, 현재 `main`에도 그대로 적용 가능한 "모듈 역할 분석" 문서로 봐도 무방합니다. 단, `main`의 UI 화면 구성과 시각적 디테일은 이 worktree보다 더 앞서 있을 수 있습니다.

## 2. 전체 판단

현재 구현된 모듈 구조는 단순한 프로젝트 분리 수준이 아니라, 실제 책임 분리가 반영된 구조입니다. 특히 `Security`, `Workflow`, `Dose`, `Update`, `PatientManagement`, `Data`는 독립 서비스 모듈로서 의미 있는 책임을 수행하고 있습니다.

반면, 현재 런타임 조립은 아직 "개발/시뮬레이션 중심" 성격이 강합니다. 앱 조립 루트에서 시뮬레이터와 null repository가 여전히 사용되고 있고, 실장비 경로와 일부 UI 설정/응급 플로우는 placeholder 단계에 머물러 있습니다.

정리하면, 현재 `main`에 전달해야 할 핵심 메시지는 다음입니다.

- 모듈 구조는 이미 실질적으로 성립돼 있다.
- 핵심 도메인 로직은 여러 모듈에서 구현이 깊게 들어가 있다.
- 다만 제품 런타임 관점에서는 실장비 연동, 일부 저장소 구현, 일부 UI 플로우가 아직 닫히지 않았다.

## 3. 역할 기준 모듈 분석

| 역할 구역 | 모듈 | 현재 역할 | 현재 상태 판단 |
|---|---|---|---|
| 앱 조립과 런타임 부트스트랩 | `HnVue.App` | 전체 DI 조립, 보안 키 로딩, DB 시드, 메인 윈도우 시작 | 개발용 composition root로는 충분하지만, 프로덕션 배선은 아직 아님 |
| 공통 모델과 계약 | `HnVue.Common`, `HnVue.UI.Contracts` | 결과 타입, 공통 인터페이스, 이벤트, enum, UI 계약 | 의존성 방향을 안정시키는 기반층 역할이 명확함 |
| 영속성 계층 | `HnVue.Data` | EF Core DbContext, 엔티티, 매퍼, 사용자/감사/환자/스터디 저장소 | 가장 닫힌 모듈 중 하나이나 환자 필드 암호화 TODO가 남아 있음 |
| 인증·권한·감사 | `HnVue.Security` | 인증, RBAC, JWT, denylist, 감사 로그 | 현재 코드베이스에서 가장 성숙한 축 중 하나 |
| 촬영 워크플로·방사선 안전 | `HnVue.Workflow`, `HnVue.Dose` | 상태 머신, 노출 준비, 선량 검증, interlock | 실제 비즈니스 규칙이 깊게 구현된 핵심 도메인 |
| 환자/워크리스트 | `HnVue.PatientManagement` | 환자 등록, 검색, 수정, 응급 등록, 워크리스트 처리 | 환자 서비스는 충분히 구현됐지만 런타임 worklist 저장소는 아직 stub |
| 사고 대응 | `HnVue.Incident` | incident 기록, 대응, 알림 | 역할은 분리돼 있으나 런타임 저장소는 아직 Phase 1d 형태 |
| 장비/영상/상호운용 | `HnVue.Detector`, `HnVue.Imaging`, `HnVue.Dicom`, `HnVue.CDBurning` | 검출기 추상화, 이미지 처리, DICOM 전송/파일 IO, CD/DVD export | 시뮬레이션과 내부 로직은 구현됐지만 실장비 검출기 경로는 미완 |
| 운영/유지보수 | `HnVue.SystemAdmin`, `HnVue.Update` | 설정 관리, 감사 export, 백업, 업데이트 staging, 롤백 | 운영 책임은 분명하지만 일부 저장소 구현은 아직 닫히지 않음 |
| 프레젠테이션 | `HnVue.UI`, `HnVue.UI.ViewModels` | WPF 화면, 컴포넌트, 컨버터, ViewModel, 세션 흐름 | 전체 뼈대는 넓게 구현됐지만 일부 플로우는 placeholder 상태 |

## 4. 모듈별 핵심 해석

### 4.1 `HnVue.App`: 완성된 "앱 조립 루트", 미완인 "최종 런타임 배선"

`App.xaml.cs`는 전체 13개 서비스 모듈을 실제로 조립하는 composition root입니다. 공통, 데이터, 보안, 워크플로, 선량, 환자, 사고, 업데이트, 시스템 설정, CD 굽기, DICOM, 이미징, UI ViewModel까지 한 곳에서 등록하고 있습니다.

하지만 여기서 동시에 현재 한계도 드러납니다.

- `IGeneratorInterface`는 `GeneratorSimulator`
- `IDetectorInterface`는 `DetectorSimulator`
- `IDoseRepository`, `IWorklistRepository`, `IIncidentRepository`, `IUpdateRepository`, `ISystemSettingsRepository`, `HnVue.CDBurning.IStudyRepository`는 null/stub 구현

즉, 앱은 "모듈 구조를 제대로 조립"하고 있지만, 런타임 배선은 아직 production adapter와 real repository가 아니라 개발용 대체재 중심입니다.

## 4.2 `HnVue.Security`: 역할이 가장 분명한 모듈

`SecurityService`는 단순 로그인 서비스가 아니라 다음 책임을 모두 가집니다.

- 비밀번호 기반 인증
- 로그인 실패 누적 잠금
- RBAC 검사
- JWT 발급
- 로그아웃 시 JTI denylist 철회
- 비밀번호 변경 정책 검증
- Quick PIN 설정 및 잠금
- 감사 로그 기록

즉 이 모듈은 이미 "보안 부속 기능"이 아니라 독립 운영 정책 모듈로 동작합니다. 현재 구현 수준만 보면 `main`의 보안 책임 축은 가장 안정적인 쪽에 속합니다.

## 4.3 `HnVue.Workflow` + `HnVue.Dose`: 현재 시스템의 핵심 도메인

`WorkflowEngine`은 상태 전이만 관리하는 얇은 엔진이 아닙니다.

- 워크플로 상태 머신 관리
- 노출 전 RBAC 검사
- 선량 검증 연계
- `Allow / Warn / Block / Emergency` 안전 상태 반영
- 장비 arm/abort 연계

`DoseService`도 단순 저장소 wrapper가 아닙니다.

- body part별 DRL
- DAP/ESD/EI 계산
- 4단계 interlock 분류
- RDSR summary용 계산 보강

따라서 `main` 사용자 관점에서 이 두 모듈은 제품의 핵심 의료 로직이 이미 모듈 단위로 분리되어 있다는 증거입니다.

## 4.4 `HnVue.PatientManagement`: CRUD를 넘어 응급 경로까지 있음

`PatientService`는 등록, 검색, 수정, 삭제뿐 아니라 응급 등록 fast-path도 별도로 구현하고 있습니다.

- 일반 등록 시 duplicate 방지
- 검색/수정/삭제
- `EMERG-` prefix 기반 응급 등록
- 최소 정보 기반 emergency patient 생성

즉 이 모듈은 환자 관리 CRUD에 머물지 않고, 실제 임상 운용 시나리오를 의식한 구조입니다. 다만 `CreatedBy`가 아직 `"SYSTEM"`으로 하드코딩되어 있어 보안 컨텍스트 연결은 완전히 닫히지 않았습니다.

## 4.5 `HnVue.Update`: 실제 운영 모듈로 볼 수 있음

`SWUpdateService`는 다음 단계를 책임집니다.

- 업데이트 확인
- SHA-256 sidecar 검증
- Authenticode 서명 검증
- 백업 생성
- staged update 마커 기록
- rollback 실행
- 감사 로그 기록

즉 이 모듈은 이름만 update가 아니라, 실제 배포 안전성과 운영 복구를 담당하는 운영 모듈입니다. 다만 live binary replacement는 아직 restart 이후 적용하는 staged 방식에 머무릅니다.

## 4.6 `HnVue.Detector`: 인터페이스는 준비됐지만 제품 경로는 아직 비어 있음

이 모듈은 시뮬레이터 경로와 실제 장비 경로를 분리하려는 설계가 분명합니다. 문제는 실제 장비 adapter인 `OwnDetectorAdapter`가 아직 skeleton 상태라는 점입니다.

현재 아래 핵심 메서드가 모두 `NotImplementedException`입니다.

- `ConnectAsync`
- `DisconnectAsync`
- `ArmAsync`
- `AbortAsync`
- `GetStatusAsync`

따라서 detector 모듈의 "구조"는 맞지만, 제품 출하 기준의 실장비 연동은 아직 완료되지 않았습니다.

## 4.7 `HnVue.Data`: 구현은 강하지만 보안 마감이 남아 있음

`HnVue.Data`는 EF Core 기반 영속성 계층으로 역할이 선명합니다. 다만 `PatientEntity`에 명시적으로 남아 있는 TODO는 무시하면 안 됩니다.

- `Name`
- `DateOfBirth`
- `CreatedBy`

위 필드에 대해 AES-256-GCM 암호화 TODO가 직접 남아 있습니다. 이건 일반적인 리팩터링 항목이 아니라, 개인정보 보호와 규제 대응에 직결되는 미완입니다.

## 4.8 `HnVue.UI` / `HnVue.UI.ViewModels`: 범위는 넓지만 일부는 아직 placeholder

UI 계층은 분리 자체는 잘 되어 있습니다.

- `HnVue.UI.Contracts`: 인터페이스/메시지 계약
- `HnVue.UI.ViewModels`: 화면별 상태와 명령
- `HnVue.UI`: 실제 WPF View, Component, Converter, Theme

하지만 아직 미완인 경로가 남아 있습니다.

- `MainViewModel.Emergency()`는 화면 전환 TODO 상태
- `SettingsViewModel.SaveAsync()`는 `Task.Delay(1)` 기반 placeholder

즉 UI 계층은 "넓게 구현"되어 있으나, 모든 화면이 실제 서비스/저장소까지 완전히 연결되었다고 보기는 어렵습니다.

## 5. 최근 증적 기준 구현 밀도

가장 최근 로컬 커버리지 요약 기준으로, 구현 밀도가 높은 축은 다음과 같습니다.

- `HnVue.CDBurning` `100%`
- `HnVue.Data` `100%`
- `HnVue.Incident` `94.2%`
- `HnVue.Common` `94.1%`
- `HnVue.Workflow` `91.4%`
- `HnVue.Security` `86.2%`

상대적으로 낮은 축은 다음과 같습니다.

- `HnVue.Detector` `42.6%`
- `HnVue.UI.Contracts` `42.8%`
- `HnVue.UI.ViewModels` `42.0%`

이 수치는 "코드 품질 우열"보다, 현재 구현이 실장비/실플로우까지 닫힌 영역과 아직 skeleton 또는 placeholder가 남아 있는 영역을 잘 보여줍니다.

## 6. main 사용자에게 바로 전달할 결론

현재 구현된 모듈들은 역할 분리가 명확하며, 특히 보안, 워크플로, 선량, 업데이트, 환자 관리, 데이터 저장 계층은 단순 골격이 아니라 실제 책임을 수행하는 수준까지 구현되어 있습니다.

반면, 지금 `main`을 제품 런타임 기준으로 보면 아직 아래 항목이 남아 있습니다.

1. `App` 조립은 여전히 simulator/null repository 중심이다.
2. detector 실장비 adapter는 아직 skeleton이다.
3. patient 민감 필드 암호화가 남아 있다.
4. 일부 UI 플로우는 placeholder 상태다.

따라서 현재 `main`은 "모듈 구조가 성립된 강한 개발 베이스라인"으로는 충분하지만, "실장비 연동과 운영 배선까지 닫힌 최종 런타임"으로 보기는 아직 이릅니다.

## 7. 권고

`main` 사용자 관점에서 다음 순서가 합리적입니다.

1. `HnVue.App`의 null/stub repository 제거 우선순위 정리
2. `HnVue.Detector` 실장비 adapter 구현 완료
3. `HnVue.Data`의 환자 민감 필드 암호화 마감
4. `HnVue.UI.ViewModels`의 Emergency/Settings placeholder 제거
5. 이후 UI/테마 개선분은 별도 UX 레인으로 계속 병행
