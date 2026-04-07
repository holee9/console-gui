# HnVue.Dose

> 방사선량 관리 서비스

## 목적

X-ray 촬영 시 방사선량(DAP, mAs, kVp)을 기록하고 ALARA 원칙에 따른 DRL 기준 검증을 수행합니다.
IEC 62304 Class B — 방사선 안전 보호 모듈.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `DoseService` | `IDoseService` 구현체 — 선량 검증 및 기록 |
| `DoseRepository` | `IDoseRepository` 구현체 |
| `IDoseRepository` | 선량 데이터 리포지토리 인터페이스 |

## 선량 검증 인터락 (4단계)

`DoseService.ValidateExposureAsync()`는 `DoseValidationLevel` 열거형 4단계를 반환합니다.

| 단계 | DAP 조건 | 동작 |
|------|---------|------|
| `Allow` | DAP ≤ DRL | 정상 촬영 허용 |
| `Warn` | DRL < DAP ≤ 2×DRL | 경고 메시지, 촬영 허용 |
| `Block` | 2×DRL < DAP ≤ 5×DRL | 촬영 차단 |
| `Emergency` | DAP > 5×DRL | 비상 인터락 활성화, 물리적 리셋 필요 |

### 신체 부위별 DRL (EC RP 185 기준, mGy·cm²)

| 부위 | DRL |
|------|-----|
| CHEST | 10.0 |
| ABDOMEN / PELVIS | 25.0 |
| SPINE | 40.0 |
| SKULL | 30.0 |
| SHOULDER | 15.0 |
| KNEE | 5.0 |
| HAND / FOOT | 3.0 |
| 기타 (기본값) | 20.0 |

### DAP 추정 공식

```
DAP (mGy·cm²) ≈ (kVp² × mAs) / 500,000
```

IEC 60601-2-54 선형 근사 모델.

## 의존성

### 프로젝트 참조

- `HnVue.Common`

### NuGet 패키지

없음

## DI 등록

`AddHnVueDose()` 미제공 — App에서 직접 등록:

```csharp
services.AddSingleton<IDoseRepository, NullDoseRepository>(); // Phase 1d
services.AddScoped<IDoseService, DoseService>();
```

## RDSR 및 선량 이력 (Issue #41 구현)

### GenerateRdsrSummaryAsync()

fo-dicom 기반 DICOM Radiation Dose Structured Report(RDSR) 요약을 생성합니다.

#### 반환 필드

| 필드 | 타입 | 설명 |
|------|------|------|
| `TotalDap` | `decimal` | 누적 DAP (mGy·cm²) |
| `ExposureCount` | `int` | 노출 횟수 |
| `StudyInstanceUid` | `string` | DICOM 스터디 UID |
| `GeneratedAt` | `DateTime` | 생성 타임스탬프 |
| `Exposures` | `List<DoseExposure>` | 개별 노출 목록 |

#### DoseExposure 구조

```csharp
public class DoseExposure
{
    public decimal Dap { get; set; }          // 개별 DAP (mGy·cm²)
    public decimal kVp { get; set; }          // 튜브 전압 (kV)
    public decimal mAs { get; set; }          // 튜브 전류·시간 (mAs)
    public DateTime AcquisitionDateTime { get; set; }  // 촬영 시간
}
```

#### 사용 예

```csharp
var rdsr = await doseService.GenerateRdsrSummaryAsync(
    studyInstanceUid: "1.2.3.4.5.6.7.8",
    cancellationToken: ct
);
Console.WriteLine($"총 DAP: {rdsr.TotalDap} mGy·cm²");
Console.WriteLine($"노출 횟수: {rdsr.ExposureCount}");
```

### GetDoseHistoryAsync()

SQLite 데이터베이스에서 날짜 범위 기반으로 선량 이력을 조회합니다.

#### 메서드 서명

```csharp
Task<List<DoseRecord>> GetDoseHistoryAsync(
    DateTime startDate,
    DateTime endDate,
    CancellationToken cancellationToken = default
);
```

#### 반환 데이터 구조

| 필드 | 설명 |
|------|------|
| `PatientId` | 환자 ID |
| `PatientName` | 환자명 |
| `BodyPart` | 신체 부위 (CHEST, ABDOMEN 등) |
| `Dap` | DAP (mGy·cm²) |
| `AcquisitionDateTime` | 촬영 시간 |

#### 사용 예

```csharp
var history = await doseService.GetDoseHistoryAsync(
    startDate: new DateTime(2026, 04, 01),
    endDate: new DateTime(2026, 04, 07),
    cancellationToken: ct
);

foreach (var record in history)
{
    Console.WriteLine($"{record.PatientName}: {record.Dap} mGy·cm² ({record.BodyPart})");
}
```

### SWR 준수

- **SWR-DM-044**: RDSR 생성 기능
- **SWR-DM-046**: 구조화된 방사선량 보고
- **SWR-DM-051**: 선량 이력 저장
- **SWR-DM-052**: 선량 이력 조회

---

## 테스트 현황

| 항목 | 내용 |
|------|------|
| 테스트 프로젝트 | `tests/HnVue.Dose.Tests` |
| 테스트 메서드 수 | **25개** (기존 16 + RDSR/History 9개) |
| 테스트 커버리지 | **≥85%** (SWR-NF-MT-051 충족) |
| 신규 테스트 | `GenerateRdsrSummaryAsync` 4개, `GetDoseHistoryAsync` 3개, 데이터 모델 2개 |

## 비고

- DRL(Diagnostic Reference Level) 기반 선량 검증 — IEC 60601-2-54
- `ExposureParameters`, `DoseRecord`, `DoseValidationResult`, `DoseValidationLevel` 모델은 `HnVue.Common`에 정의
- `WorkflowEngine.PrepareExposureAsync()`에서 호출되는 핵심 안전 서비스
- RDSR 기능: 의료 영상 정보 시스템(PACS) 연계 및 규정 준수 지원
