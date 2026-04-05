# HnVue.Update

> 소프트웨어 업데이트 및 백업 서비스

## 목적

OTA(Over-The-Air) 소프트웨어 업데이트, 코드 서명 검증, 업데이트 전 자동 백업 기능을 제공합니다.

## 주요 타입

| 타입 | 설명 |
|------|------|
| `SWUpdateService` | ISWUpdateService 구현체 — 업데이트 다운로드/적용 |
| `CodeSignVerifier` | Authenticode 코드 서명 검증 |
| `BackupService` | 업데이트 전 자동 백업 |
| `IUpdateRepository` | 업데이트 메타데이터 리포지토리 인터페이스 |

## 의존성

### 프로젝트 참조

- `HnVue.Common`
- `HnVue.Security`

### NuGet 패키지

없음

## DI 등록

없음 (App에서 직접 등록)

## 비고

- 코드 서명 검증 필수 (FDA §524B 무결성 요구사항)
- UpdateInfo 모델은 HnVue.Common에 정의
- 롤백 지원을 위한 자동 백업
