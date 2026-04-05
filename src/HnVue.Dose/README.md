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

## 테스트 현황

- 테스트 프로젝트: `tests/HnVue.Dose.Tests`
- 테스트 메서드 수: 16개 (`DoseServiceTests.cs`)

## 비고

- DRL(Diagnostic Reference Level) 기반 선량 검증 — IEC 60601-2-54
- `ExposureParameters`, `DoseRecord`, `DoseValidationResult`, `DoseValidationLevel` 모델은 `HnVue.Common`에 정의
- `WorkflowEngine.PrepareExposureAsync()`에서 호출되는 핵심 안전 서비스
