# HnVue.Detector

> FPD (Flat Panel Detector) 검출기 인터페이스 및 어댑터

## 목적

CsI FPD 검출기와의 통신을 추상화하고 DR (Digital Radiography) 영상 획득 흐름을 제공합니다.  
자사 SDK 연동 준비 완료 상태이며, 개발/테스트용 시뮬레이터를 포함합니다.  
타사 검출기 SDK 연동 시 `ThirdParty/VendorAdapterTemplate.cs`를 기준으로 확장합니다.

---

## 주요 타입

| 타입 | 설명 |
|---|---|
| `IDetectorInterface` | 검출기 추상화 인터페이스 (HnVue.Common.Abstractions) |
| `DetectorSimulator` | 개발/테스트용 시뮬레이터. 하드웨어 없이 전체 흐름 검증 가능 |
| `OwnDetectorAdapter` | 자사 CsI FPD 프로덕션 어댑터 (SDK 연동 전까지 NotImplementedException) |
| `OwnDetectorNativeMethods` | 자사 native SDK P/Invoke 선언 (`#if OWN_DETECTOR_NATIVE_SDK`) |
| `OwnDetectorConfig` | 자사 검출기 설정 (DetectorConfig 상속, CalibrationPath, BitsPerPixel) |
| `VendorAdapterTemplate` | 타사 SDK 어댑터 구현 패턴 가이드. 복사하여 사용 |
| `DetectorConfig` | 기본 설정 record (Host, Port, ReadoutTimeoutMs, ArmTimeoutMs) |

---

## IDetectorInterface 상세

### 인터페이스 계약 (HnVue.Common.Abstractions)

```
ConnectAsync()     → Disconnected → Idle
DisconnectAsync()  → Any → Disconnected
ArmAsync(triggerMode)  → Idle → Armed → Acquiring → ImageReady → Idle
AbortAsync()       → Any → Error
GetStatusAsync()   → DetectorStatus (State, IsReadyToArm, SerialNumber, TemperatureCelsius)
```

### 이벤트

| 이벤트 | 발생 시점 | 페이로드 |
|--------|----------|----------|
| `StateChanged` | 상태 전이 시 | `DetectorStateChangedEventArgs` (PreviousState, NewState, Reason) |
| `ImageAcquired` | 영상 획득 완료 시 | `ImageAcquiredEventArgs` (Image: RawDetectorImage) |

### 트리거 모드

| `DetectorTriggerMode` | 설명 |
|-----------------------|------|
| `Sync` | X-ray 제너레이터 하드웨어 동기 트리거 (프로덕션 기본) |
| `FreeRun` | 소프트웨어 트리거 (개발/테스트용) |

---

## DetectorSimulator 상세

개발 및 CI/CD 환경에서 실 하드웨어 없이 동작합니다.

| 프로퍼티 | 기본값 | 설명 |
|---|---|---|
| `ArmDelayMs` | 100 ms | ARM 후 영상 준비 시뮬레이션 지연 |
| `ReadoutDelayMs` | 50 ms | 영상 readout 시뮬레이션 지연 |
| `SimulatedImageWidth` | 2048 | 시뮬레이션 영상 가로 픽셀 |
| `SimulatedImageHeight` | 2048 | 시뮬레이션 영상 세로 픽셀 |
| `FailNextConnectWith` | `null` | 설정 시 다음 ConnectAsync 강제 실패 |
| `FailNextArmWith` | `null` | 설정 시 다음 ArmAsync 강제 실패 |

**시뮬레이션 영상:** 12-bit 노이즈 (2048 ± 400, 16-bit LE per pixel, 2 bytes/pixel)

---

## OwnDetectorAdapter (자사 SDK 연동)

자사 CsI FPD 검출기의 프로덕션 어댑터입니다.

### SDK 연동 절차

1. `sdk/own-detector/` 폴더에 SDK DLL 배치:
   - managed SDK: `net8.0-windows/OwnDetectorSdk.dll`
   - native SDK: `x64/OwnDetectorNative.dll`
2. `OwnDetectorNativeMethods.cs` 또는 managed wrapper 호출로 `NotImplementedException` 교체
3. `#if OWN_DETECTOR_NATIVE_SDK` 조건부 컴파일 플래그 제거
4. `HnVue.App/App.xaml.cs`에서 `DetectorSimulator` → `OwnDetectorAdapter` 교체

### OwnDetectorConfig

```csharp
var config = new OwnDetectorConfig(
    Host: "192.168.1.100",
    Port: 8888)
{
    CalibrationPath = @"C:\Calibration\det_cal.bin",
    BitsPerPixel = 14
};
```

---

## 타사 SDK 연동 (VendorAdapterTemplate)

타사 검출기 SDK 연동 시 `VendorAdapterTemplate.cs`를 복사합니다.

```
src/HnVue.Detector/ThirdParty/{VendorName}/{VendorName}DetectorAdapter.cs
```

지원 패턴:

| 패턴 | 설명 |
|------|------|
| Managed DLL | .NET managed SDK (Task-based 또는 이벤트 콜백) |
| Native C DLL | P/Invoke (DET_Open, DET_Arm, DET_GetImage 등) |
| COM/RCW | COM-based SDK (STA 스레드 래퍼 필요) |
| GigE Vision | GenICam/GigE Vision 표준 (GigEV SDK 필요) |
| TCP/IP | 커스텀 네트워크 프로토콜 |

자세한 내용: `sdk/third-party/README.md`

---

## sdk/ 폴더 구조

```
sdk/
├── own-detector/
│   ├── README.md
│   ├── net8.0-windows/     ← managed SDK DLL 배치 (git 추적 제외)
│   └── x64/               ← native SDK DLL 배치 (git 추적 제외)
└── third-party/
    ├── README.md
    └── {vendor-name}/
        ├── net8.0-windows/ ← managed SDK DLL
        └── x64/            ← native SDK DLL
```

SDK DLL은 `.gitignore`에 의해 추적에서 제외됩니다 (`*.dll`, `*.lib`, `*.pdb`).

---

## 상태 머신

```
Disconnected
  └─ ConnectAsync() → Idle
       ├─ ArmAsync(Sync) → Armed → Acquiring → ImageReady → Idle
       │                                        └─ ImageAcquired 이벤트 발행
       ├─ AbortAsync() → Error
       └─ DisconnectAsync() → Disconnected

Any state
  └─ AbortAsync() → Error (즉각 중단, 안전 복귀)
```

---

## 테스트

| 파일 | 테스트 수 | 내용 |
|---|---|---|
| `DetectorSimulatorTests.cs` | 11 | 초기 상태, ConnectAsync (성공/실패 주입), DisconnectAsync, ArmAsync (성공/상태 오류/실패 주입), AbortAsync, GetStatusAsync, StateChanged 이벤트, FreeRun 모드 |

**Trait:** `[Trait("SWR", "SWR-WF-030")]`

---

## 의존성

### 프로젝트 참조

| 프로젝트 | 역할 |
|---|---|
| `HnVue.Common` | `IDetectorInterface`, `DetectorState`, `RawDetectorImage` 등 |

### 조건부 SDK 참조 (csproj)

```xml
<!-- 자사 managed SDK (DLL 배치 후 자동 활성화) -->
<Reference Include="OwnDetectorSdk"
           Condition="Exists('$(SolutionDir)sdk\own-detector\net8.0-windows\OwnDetectorSdk.dll')">
  ...
</Reference>
```

DLL 파일이 없으면 참조가 비활성화되어 빌드가 항상 성공합니다 (DetectorSimulator 사용).

---

## DI 등록

`HnVue.App/App.xaml.cs`에서 직접 등록:

```csharp
// 개발/테스트: 시뮬레이터
services.AddSingleton<IDetectorInterface, DetectorSimulator>();

// 프로덕션 전환 시 (SDK 준비 후):
// services.AddSingleton<IDetectorInterface>(
//     new OwnDetectorAdapter(new OwnDetectorConfig("192.168.1.100")));
```

---

## 비고

- IEC 62304 §5.3.6 traceability: `SWR-WF-030` 계열로 추적
- `DetectorSimulator`로 하드웨어 없이 개발 및 CI/CD 가능
- `OwnDetectorAdapter`의 모든 메서드는 SDK 도착 전까지 `NotImplementedException`을 발생시킵니다
- `VendorAdapterTemplate`은 코드 복사 후 TODO 항목만 교체하면 연동 완료
