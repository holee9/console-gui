# 자사 Detector SDK 배치 가이드

이 폴더에 자사 FPD(CsI) 검출기 SDK 파일을 배치합니다.

## 폴더 구조

```
sdk/own-detector/
├── net8.0-windows/        ← .NET managed wrapper DLL (우선 사용)
│   └── OwnDetectorSdk.dll
├── x64/                   ← Native C/C++ DLL (P/Invoke 사용 시)
│   └── OwnDetectorNative.dll
└── README.md              ← 이 파일
```

## SDK 타입별 작업 절차

### Case A: .NET Managed SDK (`OwnDetectorSdk.dll`)

1. `net8.0-windows/OwnDetectorSdk.dll` 파일을 배치합니다.
2. `HnVue.Detector.csproj`의 Reference 블록이 자동으로 SDK를 참조합니다.
3. `OwnDetector/OwnDetectorAdapter.cs`의 TODO 항목을 managed API 호출로 교체합니다.
4. `OwnDetector/OwnDetectorNativeMethods.cs`는 필요 없으므로 삭제합니다.

### Case B: Native C/C++ SDK (`OwnDetectorNative.dll`)

1. `x64/OwnDetectorNative.dll` 파일을 배치합니다.
2. `HnVue.App/App.xaml.cs`에서 native DLL이 출력 폴더에 복사되도록 설정합니다.
3. `OwnDetector/OwnDetectorNativeMethods.cs`의 P/Invoke 선언에서
   `#if OWN_DETECTOR_NATIVE_SDK` 조건을 제거하고 실제 함수 시그니처를 입력합니다.
4. `OwnDetector/OwnDetectorAdapter.cs`의 TODO 항목을 P/Invoke 호출로 교체합니다.

## DI 등록 변경

SDK 구현 완료 후 `src/HnVue.App/App.xaml.cs`에서:

```csharp
// Before (시뮬레이터):
services.AddSingleton<IDetectorInterface, DetectorSimulator>();

// After (자사 SDK):
services.AddSingleton<IDetectorInterface>(
    new OwnDetectorAdapter(new OwnDetectorConfig(
        Host: "192.168.1.100",
        Port: 8888,
        CalibrationPath: @"C:\HnVue\Calibration\SN-001\")));
```

## 주의사항

- SDK DLL 파일은 `.gitignore`에 의해 버전 관리에서 제외됩니다.
- SDK 파일 없이도 `DetectorSimulator`로 빌드 및 실행이 가능합니다.
- IEC 62304 §5.3.3: SDK 버전은 SBOM(Software Bill of Materials)에 기록해야 합니다.
