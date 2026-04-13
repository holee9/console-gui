# DISPATCH: Team B — S06 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-13 |
| **발행자** | Commander Center |
| **대상** | Team B |
| **브랜치** | team/team-b |
| **유형** | S06 R2 — Detector SDK 어댑터 구현 + Safety-Critical 커버리지 갭 해소 |
| **우선순위** | P1-Critical |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S06-R2-team-b.md)만 Status 업데이트.

---

## 컨텍스트

sdk/ 폴더에 실제 디텍터 SDK가 추가됨:
1. **자사 AbyzSdk**: `sdk/own-detector/bluesdk/AbyzSdk.dll` (managed .NET)
2. **HME SDK**: `sdk/third-party/hme-licence/dll/libxd2.dll` (native C)

Safety-Critical 커버리지 갭 (QA S06-R1 결과):
- Incident: 79.8% (목표 90%, -10.2%)
- Dose: 89.9% (목표 90%, -0.1%)

---

## 사전 확인

```bash
git checkout team/team-b
git pull origin main
```

---

## Task 1 (P1-Critical): AbyzSdk 어댑터 구현

### SDK 정보
- DLL: `sdk/own-detector/bluesdk/AbyzSdk.dll` + `AbyzSdk.Imaging.dll`
- 네임스페이스: `AbyzSdk.Application.UseCases`, `AbyzSdk.Domain.Detectors`, `AbyzSdk.Infrastructure.Adapters`
- 아키텍처: Clean Architecture (UseCase 기반)
- 연결: TCP/IP (IP + Port)

### 핵심 API (AbyzSdk 분석 결과)
```csharp
// Use Cases
ConnectDetectorUseCase   // ConnectDetectorRequest → ConnectDetectorResponse
AcquireImageUseCase      // AcquireImageRequest → AcquireImageResponse
CalibrateDetectorUseCase // CalibrationRequest → CalibrationResponse
DisconnectDetectorUseCase

// IDetectorService
ConnectAsync()
DisconnectAsync() / DisconnectAllAsync()
AcquireImageAsync()
CalibrateAsync()
PingAsync()
IsSleepModeAsync() / SetSleepModeAsync()
GetDetectorInfoAsync() / GetFirmwareVersion() / GetTemperature()

// 이벤트
NotifyAcquisitionEvent (AcquisitionEventHandler)
// AcquisitionEventArgs: FrameData (Width, Height, PixelData, FrameNumber)

// 지원 모드
BitDepth: Mode8bit, Mode14bit, Mode16bit
ResolutionMode, ConnectionType
BatteryStatus (무선 배터리 상태)
```

### 구현 대상 파일
- `src/HnVue.Detector/OwnDetector/OwnDetectorAdapter.cs` — TODO 제거, 실제 AbyzSdk 호출로 교체
- `src/HnVue.Detector/HnVue.Detector.csproj` — AbyzSdk.dll Reference 추가

```xml
<!-- csproj에 추가 -->
<ItemGroup Condition="Exists('$(SolutionDir)sdk\own-detector\bluesdk\AbyzSdk.dll')">
  <Reference Include="AbyzSdk">
    <HintPath>$(SolutionDir)sdk\own-detector\bluesdk\AbyzSdk.dll</HintPath>
    <Private>true</Private>
  </Reference>
  <Reference Include="AbyzSdk.Imaging">
    <HintPath>$(SolutionDir)sdk\own-detector\bluesdk\AbyzSdk.Imaging.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

### 구현 패턴 (OwnDetectorAdapter)
- `_session` 타입: `AbyzSdk.Infrastructure.Adapters.DetectorServiceAdapter` 또는 `IDetectorService`
- `ConnectAsync`: UseCase 통해 연결 + `NotifyAcquisitionEvent` 구독
- `ArmAsync`: `AcquireImageAsync` 호출 (비동기, 이벤트 기반)
- `OnImageAcquired`: `AcquisitionEventArgs.FrameData` → `RawDetectorImage` 변환
- `DisconnectAsync`: 이벤트 구독 해제 + `DisconnectAsync`

### 검증
```bash
dotnet build src/HnVue.Detector/ --configuration Release 2>&1 | tail -5
dotnet test tests/HnVue.Detector.Tests/ 2>&1 | tail -5
```

---

## Task 2 (P2): HME libxd2 어댑터 스텁 생성

### SDK 정보
- DLL: `sdk/third-party/hme-licence/dll/libxd2.dll` (Native C)
- 지원 모델: S4335(WA/WF), S4343(WA)
- 파라미터 파일: `sdk/third-party/hme-licence/HME/2G_SDK/XAS_W_2G_SampleCode/Debug/param/`

### 분석된 API (PE exports)
```c
SD_CheckConnection()     // 연결 상태 확인
SD_GetStatus()           // 디텍터 상태 조회
SD_SetSleepMethod()      // 슬립 설정
SD_Sleep() / SD_WakeUp() // 슬립/웨이크
SDAcq_Abort()            // 획득 중단
SDAcq_ResetReady()       // 준비 상태 리셋
SDAcq_SetStatusHandler() // 상태 콜백 핸들러 등록
WakeUpDetector() / SleepDetector()
GetDiagData()            // 진단 데이터
GetAEDConfig()           // AED 설정
ResetByFPGAReset() / ResetByReboot()
```

### 생성 대상 파일
- `src/HnVue.Detector/ThirdParty/Hme/HmeDetectorAdapter.cs`
- `src/HnVue.Detector/ThirdParty/Hme/HmeDetectorConfig.cs`
- `src/HnVue.Detector/ThirdParty/Hme/HmeNativeMethods.cs`

`VendorAdapterTemplate.cs`를 기반으로 생성. `HmeNativeMethods.cs`에 위 C 함수들의 P/Invoke 선언 작성.

---

## Task 3 (P1-Critical): Incident 커버리지 90% 달성

현재: 79.8% → 목표: 90% (+10.2% 필요)

```bash
dotnet test tests/HnVue.Incident.Tests/ --collect:"XPlat Code Coverage" 2>&1 | tail -10
```

미커버 경로 분석 후 테스트 추가:
- `tests/HnVue.Incident.Tests/` 에 테스트 추가

---

## Task 4 (P2): Dose 커버리지 90% 달성

현재: 89.9% → 목표: 90% (+0.1%)

```bash
dotnet test tests/HnVue.Dose.Tests/ --collect:"XPlat Code Coverage" 2>&1 | tail -10
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.Detector/ tests/HnVue.Incident.Tests/ tests/HnVue.Dose.Tests/
git commit -m "feat(team-b): AbyzSdk 어댑터 구현 + HME 스텁 + Incident/Dose 커버리지 (#issue)"
git push origin team/team-b
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: AbyzSdk 어댑터 구현 (P1) | NOT_STARTED | -- | OwnDetectorAdapter.cs |
| Task 2: HME libxd2 어댑터 스텁 (P2) | NOT_STARTED | -- | ThirdParty/Hme/ |
| Task 3: Incident 90% (P1-Critical) | NOT_STARTED | -- | 79.8% → 90% |
| Task 4: Dose 90% (P2) | NOT_STARTED | -- | 89.9% → 90% |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
