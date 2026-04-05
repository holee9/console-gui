# HnVue.Dose

> 방사선량 관리 서비스

## 목적

X-ray 촬영 시 방사선량(DAP, mAs, kVp)을 기록하고 ALARA 원칙에 따른 DRL 기준 검증을 수행합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `DoseService` | 선량 기록 및 검증 서비스 구현체 |
| `IDoseRepository` | 선량 데이터 리포지토리 인터페이스 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`

### NuGet 패키지

없음

## DI 등록

없음 (App에서 직접 등록)

## 비고

- DRL(Diagnostic Reference Level) 기반 선량 검증
- ExposureParameters, DoseRecord, DoseValidationResult 모델은 HnVue.Common에 정의
