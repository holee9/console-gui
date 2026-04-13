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

### SDK 정보 (dumpbin 분석 완료)
- DLL: `sdk/own-detector/bluesdk/AbyzSdk.dll` (185,344 bytes) + `AbyzSdk.Imaging.dll` (20,016 bytes)
- **타입 확인**: .NET managed (IL Only, CLR 2.05 runtime)
- 네임스페이스: `AbyzSdk.Application.UseCases`, `AbyzSdk.Domain.Detectors`, `AbyzSdk.Infrastructure.Adapters`
- 아키텍처: Clean Architecture (UseCase 기반)
- 연결: TCP/IP (IP + Port)
- **중요**: .NET managed DLL이므로 P/Invoke 불필요. Direct Reference로 사용.

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
- **OwnDetectorNativeMethods.cs**: managed SDK를 사용하므로 이 파일은 유지하되 `#if OWN_DETECTOR_NATIVE_SDK`로 분리 (README Case A/B 참고)

### 검증
```bash
dotnet build src/HnVue.Detector/ --configuration Release 2>&1 | tail -5
dotnet test tests/HnVue.Detector.Tests/ 2>&1 | tail -5
```

---

## Task 2 (P2): HME libxd2 어댑터 스텁 생성

### SDK 정보 (dumpbin 분석 완료)
- DLL: `sdk/third-party/hme-licence/dll/libxd2.dll` (Native C, 146 exports)
- DLL: `sdk/third-party/hme-licence/dll/libxd.dll` (Native C, 100 exports) — 1G SDK
- DLL: `sdk/third-party/hme-licence/dll/CIB_Mgr.dll` (9 exports) — 회전/노출 제어용 CIB (CR/DR)
- 지원 모델:
  - **S4335-WA** (S4335-CA): 3072x2560, CutSize L15/T2/R17/B92
  - **S4335-WF** (SZ4335-W): 3072x2560, CutSize L15/T2/R17/B92
  - **S4343-WA** (S4343-CA): 3072x3072, CutSize L15/T2/R17/B34
- 파라미터 파일: `sdk/third-party/hme-licence/HME/2G_SDK/XAS_W_2G_SampleCode/{Debug,Release}/param/`
- 샘플코드: VS2019 C++ 프로젝트 (Test.sln, x86/x64)

### 네트워크 프로토콜 (5-소켓 연결)
```
IP: 단일 디텍터 IP (예: 192.168.197.80)
Port 25000: Control Socket (명령/제어)
Port 25001: Data Socket (이미지 데이터)
Port 25002: Trigger Socket (X-ray 트리거)
Port 25003: Status Socket (상태 모니터링)
Port 25004: S-Align Socket (센서 정렬)
```

### libxd2.dll 전체 Export API (146개, dumpbin 확인)
```c
// === Detector Lifecycle ===
SD_CreateDetector()          // 디텍터 인스턴스 생성
SD_DestroyDetector()         // 디텍터 인스턴스 소멸
SD_CheckConnection()         // 연결 상태 확인
SD_GetStatus()               // 디텍터 상태 조회
SD_GetDetectorInfo()         // 디텍터 정보 조회
SD_SetDetectorInfo()         // 디텍터 정보 설정
SD_GetFrameSize()            // 프레임 크기 조회
SD_GetThumbSize()            // 썸네일 크기 조회
SD_GetSDKVersion()           // SDK 버전 조회
SD_GetVariable() / SD_SetVariable()  // 변수 읽기/쓰기

// === Sleep/Power Management ===
SD_SetSleepMethod()          // 슬립 방법 설정
SD_Sleep() / SD_WakeUp()     // 슬립/웨이크
SD_PowerOff()                // 전원 끄기
SD_Reboot()                  // 재부팅

// === Operation Mode ===
SD_SetOperationMode() / SD_SetOperationModeEx()
SD_SetStationMode()
SD_SetRegistered()
SD_SetIndicator()
SD_GetWindowTime() / SD_SetWindowTime()
SD_SetWindowMode()
SD_GetAEDConfig() / SD_SetAEDConfig()  // AED (Auto Exposure Detection)
SD_SetAEDPulseWidth()
SD_CalSensor()               // 센서 교정

// === Acquisition (SDAcq_*) ===
SDAcq_CreateEx_Normal()      // 일반 촬영 모드 생성
SDAcq_CreateEx_AED_Bright()  // AED 밝은 환경 모드
SDAcq_CreateEx_AED_Dark()    // AED 어두운 환경 모드
SDAcq_CreateEx_AED_ValidBright()
SDAcq_CreateEx_Dual()        // 듀얼 모드
SDAcq_CreateEx_Stitch()      // 스티치 모드
SDAcq_CreateEx_InstantWindow()
SDAcq_CreateEx_Lag1st()      // 랙 측정
SDAcq_CreateEx_QuickExposure()
SDAcq_CreateEx_Tomo_Bright() // 토모그래피 밝은
SDAcq_CreateEx_Tomo_Dark()   // 토모그래피 어두운
SDAcq_CreateEx_Retrans()     // 재전송
SDAcq_CreateEx_Retrans_AED()
SDAcq_Create_FastNormal()    // 고속 일반
SDAcq_Create_Normal_Dark()
SDAcq_Destroy()              // 획득 객체 소멸
SDAcq_Execute()              // 획득 실행
SDAcq_Notify()               // 획득 알림
SDAcq_PrepareReady()         // 준비 완료
SDAcq_ResetReady()           // 준비 상태 리셋
SDAcq_Abort()                // 획득 중단
SDAcq_SetAcquisitionID()
SDAcq_SetContextData()
SDAcq_SetDataHandler()       // 이미지 데이터 콜백
SDAcq_SetEventHandler()      // 이벤트 콜백
SDAcq_SetExceptionHandler()  // 예외 콜백
SDAcq_SetProgressHandler()   // 진행 상황 콜백
SDAcq_SetStatusHandler()     // 상태 콜백
SDAcq_SetThumbnailHandler()  // 썸네일 콜백

// === Calibration (SDCal_*) ===
SDCal_Create() / SDCal_Destroy()
SDCal_Create_DC() / SDCal_Destroy_DC()
SDCal_Process() / SDCal_Process_DC() / SDCal_Process_DCEx()
SDCal_Process_Thumbnail()
SDCal_AverageFrames()
SDCal_SendCommand() / SDCal_SendCommand_DC()
SDCal_GenerateBPM()          // Bad Pixel Map 생성
SDCal_ValidateBrightSource()
SDCal_ValidateDataSet()

// === Diagnostic ===
SDDiag_AutoLineDefectDetection()   // 라인 결함 자동 감지
SDDiag_AutoPixelDefectDetection()  // 픽셀 결함 자동 감지
SDDiag_ValidLineDefect()
SDDebug_GetRegisters() / SDDebug_SetRegisters()
SD_GetSelfTest()
SD_RefreshDiagnosticData()
SD_SetDiagnosticLogPath() / SD_SetDiagnosticTagData()

// === Firmware Update (SDUpdater_*) ===
SDUpdater_UpdateFirmware()
SDUpdater_CheckFirmwareFileCRC()
SDUpdater_GetFirmwareFileVersion() / GetFirmwareFileGeneration()
SDUpdater_GetDesiredFirmwareVersion()
SDUpdater_ValidateUpdatedFirmware()
SDUpdater_GetAPListFromDetector() / GetWiFiCountryList()
SDUpdater_ReadEngineerInfo() / WriteEngineerInfo()
SDUpdater_ReadFirmwareInfo() / WriteFirmwareInfo()
SDUpdater_SetHostAddress()
SDUpdater_SendSystemCommand()

// === File Transfer (SDFile_*, SDRemote_*) ===
SDFile_Begin() / SDFile_End() / SDFile_Exists()
SDFile_GetList() / SDFile_MoveFile() / SDFile_RemoveFile()
SDFile_RecvFile() / SDFile_SendFile()
SDRemote_GetFileList() / SDRemote_RecvFile() / SDRemote_SendFile()
SDRemote_RemoveFile() / SDRemote_RenameFile()
SDRemote_RecvShockLog() / RecvShockLogDelta() / ResetShockLogDelta()
SDRemote_BackupLogs()

// === Retransmission ===
SD_ConfirmData() / SD_GetRetransInfo() / SD_ClearRetransInfo()
SD_DiscardRetransInfo() / SD_RetransExists() / SD_RetransmissionExists()

// === S-Align ===
SD_GetSAlign()

// === Inspect ===
SDInspect_ResetDetectorMode()
SDInspect_SetFeedbackCap()
SDInspect_SetupNormalNoRefresh()

// === Utility ===
SDTest_SaveToFile()
SD_CheckFirmwareCompatible()
```

### CIB_Mgr.dll Export (9개 — 회전/노출 제어)
```c
ComOpen()                    // 시리얼 포트 열기
ComClose()                   // 시리얼 포트 닫기
CIB_SetEventHandler()        // 이벤트 핸들러 등록
CIB_SetEventFwVerHandler()   // 펌웨어 버전 이벤트 핸들러
CIB_ExposeOn()               // 노출(촬영) 시작
CIB_FwVerInfoReq()           // 펌웨어 버전 정보 요청
CIB_CR_DR_Mode_SelectConfirm() // CR/DR 모드 선택 확인
CIB_CR_DR_Mode_Lock_Unlock()   // CR/DR 모드 잠금/해제
CIB_GenRotorUp_Delay()       // 로터 업 지연
```

### 파라미터 파일 구조 (.par)
```
Model: S4335-CA / SZ4335-W / S4343-CA
FrameSize: 3072x2560 or 3072x3072
CutSize: 모델별 상이
CalStepCount: 20 (교정 단계)
CeilValue: 12800
Operation Modes: Normal(3), AED(3), Stitch(6), Dual(3)
WakeupTimeout: 25000ms
UseTopLineCompensation: 1
```

### 생성 대상 파일
- `src/HnVue.Detector/ThirdParty/Hme/HmeDetectorAdapter.cs` — IDetectorInterface 구현
- `src/HnVue.Detector/ThirdParty/Hme/HmeDetectorConfig.cs` — 설정 모델 (IP, ParamPath, ModelType)
- `src/HnVue.Detector/ThirdParty/Hme/HmeNativeMethods.cs` — P/Invoke 146개 함수 선언
- `src/HnVue.Detector/ThirdParty/Hme/HmeAcquisitionMode.cs` — 촬영 모드 enum (Normal, AED, Stitch, Dual, Tomo, Quick)
- `src/HnVue.Detector/ThirdParty/Hme/HmeDetectorModel.cs` — 모델 enum (S4335WA, S4335WF, S4343WA)
- `src/HnVue.Detector/ThirdParty/Hme/HmeFrameSize.cs` — 프레임 크기 레코드

### HmeNativeMethods.cs P/Invoke 패턴
```csharp
// 핸들 타입
using SD_HANDLE = System.IntPtr;
using SD_ACQ_HANDLE = System.IntPtr;

internal static class HmeNativeMethods
{
    private const string DllName = "libxd2.dll";

    // Lifecycle
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SD_HANDLE SD_CreateDetector(string address);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SD_DestroyDetector(SD_HANDLE handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SD_CheckConnection(SD_HANDLE handle);

    // Acquisition - callback delegates
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DataHandler(IntPtr data, int size, IntPtr context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void EventHandler(int eventType, IntPtr context);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SD_ACQ_HANDLE SDAcq_CreateEx_Normal(SD_HANDLE handle);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void SDAcq_SetDataHandler(SD_ACQ_HANDLE acq, DataHandler handler, IntPtr context);

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SDAcq_Execute(SD_ACQ_HANDLE acq);

    // ... 나머지 146개 함수
}
```

### HmeDetectorAdapter.cs 핵심 패턴
```csharp
// 5-소켓 연결은 SD_CreateDetector(address)로 자동 처리
// 콜백 기반 비동기 → IDetectorInterface 이벤트로 브릿지
// 획득 모드별 SDAcq_CreateEx_* 분기
// SDAcq_SetDataHandler로 이미지 수신 → RawDetectorImage 변환
// Thumbnail은 SDAcq_SetThumbnailHandler로 별도 수신
// .par 파일은 HmeDetectorConfig.ParamPath에서 로드
```

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
