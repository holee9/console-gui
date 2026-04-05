# HnVue.Incident

> 인시던트 대응 및 기록 서비스

## 목적

시스템 인시던트(장비 오류, 소프트웨어 장애 등)를 기록하고 대응 절차를 관리합니다. FDA 21 CFR Part 803 MDR 보고 요구사항을 지원합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `IncidentResponseService` | 인시던트 대응 서비스 구현체 |
| `IncidentRecord` | 인시던트 기록 모델 |
| `IIncidentRepository` | 인시던트 데이터 리포지토리 인터페이스 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Data`
- `HnVue.Security`

### NuGet 패키지

없음

## DI 등록

없음 (App에서 직접 등록)

## 비고

- IncidentSeverity 열거형은 HnVue.Common에 정의
- 감사 로그와 연동하여 인시던트 추적
