# 타사 Detector SDK 배치 가이드

이 폴더에 타사(third-party) FPD 검출기 vendor SDK 파일을 배치합니다.

## 폴더 구조

```
sdk/third-party/
├── vendor-a/                  ← 타사 vendor A SDK
│   ├── net8.0-windows/
│   │   └── VendorASdk.dll
│   └── x64/
│       └── VendorANative.dll
├── vendor-b/                  ← 타사 vendor B SDK
│   └── ...
└── README.md                  ← 이 파일
```

## 새 타사 Detector 연동 절차

### Step 1: SDK 파일 배치

```
sdk/third-party/{vendor-name}/net8.0-windows/{VendorName}Sdk.dll
```

### Step 2: `src/HnVue.Detector/HnVue.Detector.csproj`에 SDK Reference 추가

```xml
<ItemGroup Condition="Exists('$(SolutionDir)sdk\third-party\{vendor-name}\net8.0-windows\{VendorName}Sdk.dll')">
  <Reference Include="{VendorName}Sdk">
    <HintPath>$(SolutionDir)sdk\third-party\{vendor-name}\net8.0-windows\{VendorName}Sdk.dll</HintPath>
    <Private>true</Private>
  </Reference>
</ItemGroup>
```

### Step 3: 어댑터 구현

`src/HnVue.Detector/ThirdParty/VendorAdapterTemplate.cs`를 복사합니다:

```
src/HnVue.Detector/ThirdParty/{VendorName}/{VendorName}DetectorAdapter.cs
src/HnVue.Detector/ThirdParty/{VendorName}/{VendorName}DetectorConfig.cs
```

`VendorAdapterTemplate.cs`의 TODO 항목을 해당 vendor SDK 호출로 교체합니다.

### Step 4: DI 등록 (`src/HnVue.App/App.xaml.cs`)

```csharp
services.AddSingleton<IDetectorInterface>(
    new VendorADetectorAdapter(new VendorADetectorConfig(
        Host: "192.168.2.100")));
```

## 지원 가능 타사 SDK 패턴

| SDK 유형 | 구현 방법 |
|---------|---------|
| .NET managed DLL | Direct reference + managed API 호출 |
| Native C DLL | P/Invoke (OwnDetectorNativeMethods 패턴 참고) |
| COM 객체 | RCW(Runtime Callable Wrapper) + STA Thread |
| GigE Vision / GenICam | Basler Pylon SDK, Euresys Coaxlink 등 |
| TCP/IP 커스텀 프로토콜 | TcpClient + 프레임 파서 구현 |

## 주의사항

- SDK DLL 파일은 `.gitignore`에 의해 버전 관리에서 제외됩니다.
- 각 vendor의 라이선스 조건을 확인하세요.
- IEC 62304 §5.3.3: 타사 SDK 버전은 SBOM에 기록해야 합니다.
