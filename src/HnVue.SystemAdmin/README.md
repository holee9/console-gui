# HnVue.SystemAdmin

> 시스템 관리 서비스

## 목적

시스템 설정 관리, 기기 설정(DICOM AE Title, Generator 파라미터 등), 사용자 관리 기능을 제공합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `SystemAdminService` | ISystemAdminService 구현체 |
| `ISystemSettingsRepository` | 시스템 설정 리포지토리 인터페이스 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Data`
- `HnVue.Security`
- `HnVue.Update`

### NuGet 패키지

없음

## DI 등록

없음 (App에서 직접 등록)

## 비고

- SystemSettings, DicomSettings, GeneratorSettings, SecuritySettings 모델은 HnVue.Common에 정의
- Admin 역할만 접근 가능 (RBAC)
