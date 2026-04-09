# HnVue Console SW -- 배포 파일 매니페스트

> 버전: 1.0.0 (초안)
> 최종 업데이트: 2026-04-09
> 상태: Phase 1 기준 (인스톨러 미구축, `dotnet publish` 출력 기반)

## 개요

이 문서는 HnVue Console SW 배포에 필요한 모든 파일을 추적합니다.
빌드/모듈 추가/NuGet 변경 시 이 매니페스트를 함께 업데이트하세요.

---

## 필수 실행 파일

| 파일 | 설명 | 모듈 |
|------|------|------|
| `HnVue.App.exe` | 메인 실행 파일 (WPF 진입점) | HnVue.App |
| `HnVue.App.dll` | 메인 어셈블리 | HnVue.App |
| `HnVue.Common.dll` | 공유 모델, 인터페이스, Enum | HnVue.Common |
| `HnVue.Data.dll` | EF Core + SQLCipher 데이터 접근 | HnVue.Data |
| `HnVue.Security.dll` | 인증, RBAC, 감사 로그 | HnVue.Security |
| `HnVue.Dicom.dll` | DICOM C-STORE/C-FIND/파일 I/O | HnVue.Dicom |
| `HnVue.Detector.dll` | FPD 검출기 추상화 + 어댑터 | HnVue.Detector |
| `HnVue.Workflow.dll` | 촬영 워크플로우 엔진 | HnVue.Workflow |
| `HnVue.Imaging.dll` | 영상 처리 파이프라인 | HnVue.Imaging |
| `HnVue.Dose.dll` | 방사선 선량 관리 | HnVue.Dose |
| `HnVue.PatientManagement.dll` | 환자 관리 + Worklist | HnVue.PatientManagement |
| `HnVue.Incident.dll` | 인시던트 대응 | HnVue.Incident |
| `HnVue.Update.dll` | SW 업데이트 + 백업 | HnVue.Update |
| `HnVue.SystemAdmin.dll` | 시스템 관리 | HnVue.SystemAdmin |
| `HnVue.CDBurning.dll` | CD/DVD 미디어 소각 | HnVue.CDBurning |
| `HnVue.UI.dll` | WPF Views + Themes | HnVue.UI |
| `HnVue.UI.Contracts.dll` | UI 인터페이스 계약 | HnVue.UI.Contracts |
| `HnVue.UI.ViewModels.dll` | ViewModel 구현 | HnVue.UI.ViewModels |

**총 18개 프로젝트 DLL**

---

## 필수 설정 파일

| 파일 | 설명 | 비고 |
|------|------|------|
| `appsettings.json` | 런타임 설정 (DB 경로, DICOM, 로깅) | 배포 필수 |
| `appsettings.Production.json` | 프로덕션 오버라이드 (환경변수 참조) | 프로덕션 전용 |

**주의:** `appsettings.Development.json`은 배포에 포함하지 않음 (개발 전용, git 추적 제외)

---

## 필수 런타임

| 런타임 | 버전 | 비고 |
|--------|------|------|
| .NET 8.0 Desktop Runtime (x64) | 8.0.x LTS | WPF 런타임 필수 |
| Windows 10/11 (x64) | 10.0+ | WPF 플랫폼 요구사항 |
| Visual C++ Redistributable | 2015-2022 | SQLCipher 네이티브 의존성 |

---

## NuGet 의존성 (주요)

> 전체 목록: `docs/regulatory/DOC-019_SBOM_v1.0.md` (CycloneDX 형식, 42개 컴포넌트)

| 패키지 | 버전 | 용도 | 라이선스 |
|--------|------|------|---------|
| fo-dicom | 5.2.5 | DICOM 통신 | MIT |
| Microsoft.EntityFrameworkCore.Sqlite | 8.x | DB 접근 | MIT |
| SQLitePCLRaw.bundle_e_sqlcipher | 2.x | SQLCipher AES-256 암호화 | Apache 2.0 |
| MahApps.Metro | 2.x | WPF 테마 | MIT |
| CommunityToolkit.Mvvm | 8.x | MVVM 프레임워크 | MIT |
| Serilog | 3.x | 구조화 로깅 | Apache 2.0 |
| Microsoft.Extensions.Hosting | 8.x | DI + 호스팅 | MIT |
| BCrypt.Net-Next | 4.x | 비밀번호 해싱 | MIT |
| System.IdentityModel.Tokens.Jwt | 7.x | JWT 토큰 | MIT |

---

## 선택적 SDK 파일 (하드웨어 연동)

| 경로 | 설명 | 필수 여부 |
|------|------|----------|
| `sdk/own-detector/net8.0-windows/*.dll` | 자사 CsI FPD SDK (managed) | 실 검출기 연동 시 |
| `sdk/own-detector/x64/*.dll` | 자사 FPD 네이티브 라이브러리 | 실 검출기 연동 시 |
| `sdk/third-party/{vendor}/` | 타사 SDK DLL | 타사 검출기 사용 시 |

> SDK DLL 없이도 빌드 성공 (MSBuild 조건부 참조). 시뮬레이터 모드로 동작.

---

## 배포 폴더 구조 (예상)

```
HnVue/
+-- HnVue.App.exe                  <- 메인 실행 파일
+-- HnVue.App.dll
+-- HnVue.*.dll                    <- 17개 모듈 DLL
+-- appsettings.json               <- 런타임 설정
+-- appsettings.Production.json    <- 프로덕션 오버라이드
+-- runtimes/
|   +-- win-x64/
|       +-- native/
|           +-- e_sqlcipher.dll    <- SQLCipher 네이티브
+-- sdk/                           <- (선택) 검출기 SDK
|   +-- own-detector/
|   +-- third-party/
+-- Logs/                          <- 런타임 자동 생성
+-- Data/                          <- 런타임 자동 생성
    +-- hnvue.db                   <- SQLCipher 암호화 DB
```

---

## 인스톨러 (미구축)

현재 인스톨러는 구축되지 않았습니다. MS4(통합 테스트) 시점에 구축 예정입니다.

### 후보 기술

| 기술 | 장점 | 단점 |
|------|------|------|
| **InnoSetup** | 무료, 간단, 의료기기 업계 실적 | 스크립트 기반 |
| **WiX Toolset** | MSI 생성, 엔터프라이즈급 | 학습 곡선 |
| **MSIX** | Windows 네이티브, 자동 업데이트 | Store 의존성 |

### 인스톨러 구축 시 추가 필요 항목

- [ ] .NET Desktop Runtime 사전 설치 체크
- [ ] Visual C++ Redistributable 사전 설치 체크
- [ ] 코드 서명 인증서 (SHA-256)
- [ ] 설치 경로 기본값 (`C:\Program Files\HnVue\`)
- [ ] 바탕화면 바로가기 생성
- [ ] Windows 서비스 등록 (해당 시)
- [ ] 언인스톨러 + 데이터 보존 옵션

---

## 업데이트 이력

| 날짜 | 변경 내용 |
|------|----------|
| 2026-04-09 | 초안 작성 (Phase 1 빌드 출력 기준) |

---

## 관련 문서

- [DOC-019 SBOM](../regulatory/DOC-019_SBOM_v1.0.md) -- 전체 컴포넌트 목록 (CycloneDX)
- [DOC-033 SOUP Report](../verification/DOC-033_SOUP_Report_v1.0.md) -- 타사 소프트웨어 목록
- [DOC-043 Build Environment](../management/DOC-043_Build_Environment_v1.0.md) -- 빌드 환경 정의
- [보안 설정](../development/security.md) -- 프로덕션 배포 시 보안 체크리스트
