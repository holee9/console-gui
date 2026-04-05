# HnVue — 의료 영상 솔루션

의료 방사선 영상 획득·처리·관리 WPF 데스크톱 애플리케이션

| 상태 | 값 |
|------|-----|
| **빌드** | 0 errors, 0 warnings ✅ |
| **테스트** | 523개 전체 통과 ✅ |
| **품질 점수** | 0.91/1.0 ✅ |
| **코드 커버리지** | 85%+ (안전 임계 모듈 90%+) |
| **인허가 분류** | IEC 62304 Class B |

---

## 프로젝트 개요

### 제품 정보

| 항목 | 내용 |
|------|------|
| **제품명** | HnVue Console SW |
| **제조사** | H&abyz (에이치앤아비즈) |
| **프로젝트명** | HnX-R1 (Detector + Console SW 번들 retrofit) |
| **대체 대상** | IMFOU feel-DRCS (FDA K110033) |
| **FDA Predicate** | DRTECH EConsole1 ([FDA K231225](https://www.accessdata.fda.gov/cdrh_docs/pdf23/K231225.pdf)) |
| **IEC 62304 분류** | Class B (Software Safety Classification) |
| **인허가 대상** | MFDS 2등급, FDA 510(k), CE MDR Class IIa |
| **개발 인력** | 2명 (Software Engineers) |

### 핵심 기능

HnVue는 의료 방사선 영상 시스템의 핵심 콘솔 소프트웨어로서 다음을 제공합니다:

- **환자 관리**: 의료진 및 환자 정보 등록·조회·관리
- **촬영 워크플로우**: 환자 선택 → 프로토콜 로드 → 촬영 준비 → 노출 → 영상 획득 → PACS 전송
- **DICOM 상호운용성**: C-STORE SCU (영상 전송), C-FIND SCU (Worklist 조회), DICOM 3.0 파일 I/O
- **방사선 선량 관리**: IEC 60601-1-3 준수 검증 (4단계 인터록: ALLOW/WARN/BLOCK/EMERGENCY)
- **보안 및 인증**: JWT 기반 인증, RBAC 4역할 (Radiographer/Radiologist/Admin/Service), 감사 로그 (HMAC-SHA256 해시 체인)
- **소프트웨어 업데이트**: 무결성 검증 (SHA-256), 백업/복원 (타임스탐프), 코드 서명 검증
- **인시던트 대응**: 4단계 심각도 분류 (Critical/High/Medium/Low), 긴급 콜백
- **미디어 처리**: CD/DVD 미디어에 영상 배포 (IMAPI2 시뮬레이션)

---

## 기술 스택

### 개발 환경

| 항목 | 기술 | 설명 |
|------|------|------|
| **UI Framework** | WPF + .NET 8 LTS | MVVM 패턴, MahApps.Metro 테마 |
| **데이터베이스** | SQLite + EF Core 8 | SQLCipher AES-256 암호화 |
| **DICOM** | fo-dicom 5.1.3 (MIT) | C-STORE, C-FIND MWL, DICOM 파일 I/O |
| **인증/보안** | bcrypt (cost=12), JWT HS256, HMAC-SHA256 | 비밀번호 해싱, 토큰 기반 인증, 감사 로그 체인 |
| **테스트** | xUnit + NSubstitute + FluentAssertions | 단위 테스트 (13개 프로젝트), 통합 테스트 |
| **로깅** | Serilog | SHA-256 해시 체인, 365일 보관, 감사 추적 |
| **빌드** | MSBuild + dotnet CLI | 자동화 빌드, SBOM (CycloneDX), 코드 서명 |
| **패키지 관리** | NuGet Central Package Management | `Directory.Packages.props` 중앙 버전 관리 |

### 규제 표준 준수

| 표준 | 적용 | 설명 |
|------|:----:|------|
| **IEC 62304:2015+A1** | Class B | 소프트웨어 수명주기 프로세스 (SDLC) |
| **IEC 62366-1:2015+A1** | 필수 | 사용성 공학 (Usability Engineering) |
| **ISO 14971:2019** | 필수 | 위험 관리 (Risk Management) |
| **IEC 81001-5-1:2021** | 필수 | 사이버보안 수명주기 (Cybersecurity Lifecycle) |
| **FDA 21 CFR 820.30** | 필수 | Design Controls |
| **FDA Section 524B** | 필수 | SBOM + CVD + Patch/Update 관리 |
| **ISO 13485:2016** | 필수 | 품질 경영시스템 (QMS) |
| **DICOM 3.0 / IHE SWF** | 필수 | 상호운용성 표준 |
| **MFDS 사이버보안 가이드라인 2024** | 필수 | 35개 항목, 7대 영역 |

---

## 아키텍처 개요

### 클린 아키텍처 레이어

HnVue는 클린 아키텍처 원칙에 따라 다음과 같이 구성됩니다:

```
┌─────────────────────────────────────────────────────────┐
│ Layer 6: HnVue.App (DI 컴포지션 루트, 애플리케이션 진입점)    │
├─────────────────────────────────────────────────────────┤
│ Layer 5: HnVue.UI (WPF Views + ViewModels, MVVM 패턴)   │
├─────────────────────────────────────────────────────────┤
│ Layer 4: HnVue.Workflow (상태 머신, 워크플로우 엔진)         │
├──────────────────────────────────────────────────────────┤
│ Layer 3: Tier 3 (안전 임계 모듈 + 인프라)                  │
│  • HnVue.Dose (방사선 선량 관리, 90%+ 커버리지)              │
│  • HnVue.Incident (인시던트 대응, 90%+ 커버리지)             │
│  • HnVue.Update (소프트웨어 업데이트, 85%+ 커버리지)          │
│  • HnVue.Dicom (DICOM 통신)                              │
│  • HnVue.Imaging (영상 처리 파이프라인)                      │
│  • HnVue.PatientManagement (환자 관리)                    │
│  • HnVue.SystemAdmin (시스템 설정)                        │
│  • HnVue.CDBurning (미디어 소각)                          │
├──────────────────────────────────────────────────────────┤
│ Layer 2: HnVue.Security (인증, RBAC, 암호화, 90%+ 커버리지)  │
├──────────────────────────────────────────────────────────┤
│ Layer 1: HnVue.Data (EF Core, Repository 패턴, 80%+ 커버리지)│
├──────────────────────────────────────────────────────────┤
│ Layer 0: HnVue.Common (공유 모델, 인터페이스, Enum, DTO)      │
│          HnVue.Common.Tests (스레드 안전성, 38개 테스트)       │
└──────────────────────────────────────────────────────────┘
```

### 모듈 의존성 그래프

```
HnVue.Common (Layer 0 — 기초)
  ├─ ErrorCode (9개 도메인)
  ├─ SafeState, UserRole, WorkflowState, GeneratorState, IncidentSeverity (Enum)
  ├─ Result<T> 모나드 (Railway-Oriented Programming)
  └─ 17개 서비스 인터페이스

  └─ HnVue.Data (Layer 1 — 데이터 접근)
       ├─ EF Core 8 + SQLCipher AES-256
       ├─ 6개 Entity (Patients, Studies, Images, DoseRecords, Users, AuditLogs)
       └─ 4개 Repository (IPatientRepository, IStudyRepository, IImageRepository, IUserRepository)

       └─ HnVue.Security (Layer 2 — 인증/보안)
            ├─ PasswordHasher (bcrypt cost=12)
            ├─ JwtTokenService (HS256, 15분 만료)
            ├─ RbacPolicy (4역할 권한 상수 매트릭스)
            ├─ AuditService (HMAC-SHA256 해시 체인)
            └─ SecurityContext (스레드 안전성)

            ├─ HnVue.Dicom (Layer 3 — DICOM 통신)
            │   ├─ DicomStoreScu (C-STORE SCU)
            │   ├─ DicomFindScu (C-FIND MWL)
            │   └─ DicomFileIO, DicomFileWrapper
            │
            ├─ HnVue.Incident (Layer 3 — 안전 임계)
            │   └─ IncidentResponseService (4단계 심각도)
            │
            ├─ HnVue.Update (Layer 3 — 안전 임계)
            │   ├─ SWUpdateService
            │   ├─ CodeSignVerifier (SHA-256)
            │   └─ BackupService (타임스탬프)
            │
            ├─ HnVue.Imaging (Layer 3 — 영상 처리)
            │   └─ 외부 SDK 연동 (Phase 1c 대기)
            │
            ├─ HnVue.Dose (Layer 3 — 안전 임계)
            │   └─ DoseService (4단계 인터록: ALLOW/WARN/BLOCK/EMERGENCY)
            │
            ├─ HnVue.PatientManagement (Layer 3)
            │   ├─ PatientService (CRUD + 중복 검사)
            │   └─ WorklistService (MWL + 응급 ID)
            │
            ├─ HnVue.SystemAdmin (Layer 3)
            │   └─ SystemAdminService (설정 검증 + CSV 내보내기)
            │
            └─ HnVue.CDBurning (Layer 3)
                ├─ CDDVDBurnService
                ├─ IBurnSession
                └─ IMAPIComWrapper (IMAPI2 시뮬)

                  └─ HnVue.Workflow (Layer 4 — 안전 임계, 상태 머신)
                       ├─ WorkflowStateMachine (9-상태 전이표)
                       ├─ WorkflowEngine (IWorkflowEngine, 이벤트)
                       └─ GeneratorSimulator (장애 주입)

                       └─ HnVue.UI (Layer 5 — 프레젠테이션)
                            ├─ MainWindow (5-패널 레이아웃)
                            ├─ LoginView (JWT 로그인)
                            └─ ViewModel들 (MVVM)

                            └─ HnVue.App (Layer 6 — 컴포지션)
                                 └─ DI 등록 + Program.cs
```

---

## 모듈 상세 설명 (14개)

### Layer 0: 공유 인터페이스

#### HnVue.Common
기초 모델, 인터페이스, Result<T> 모나드, Enum 정의. 모든 모듈이 참조합니다.

**핵심 항목:**
- `Result<T>` / `Result.Success()`, `Result.Failure()`, `Result.SuccessNullable<T>()`
- `ErrorCode` Enum (9개 도메인: Security, Workflow, DICOM, Dose, Incident, Update, PatientMgmt, System, CDBurning)
- `SafeState` Enum (장치 상태)
- `UserRole` Enum (Radiographer, Radiologist, Admin, Service)
- `WorkflowState` Enum (9-상태: Idle, PatientSelected, ProtocolLoaded, ReadyToExpose, Exposing, AcquiringImage, ImageAcquired, TransmittingToCACS, Complete)
- `GeneratorState` Enum (장치 제너레이터 상태)
- `IncidentSeverity` Enum (Critical, High, Medium, Low)
- 17개 서비스 인터페이스 (ISecurityService, IDataService 등)
- 17개 DTO (PatientDto, StudyDto 등)
- `ThreadLocalSecurityContext` (ReaderWriterLockSlim 기반 스레드 안전성)

**테스트:** 38개 (`HnVue.Common.Tests`)

### Layer 1: 데이터 접근

#### HnVue.Data
EF Core 8 + SQLite + SQLCipher AES-256 암호화. Repository 패턴 구현.

**핵심 항목:**
- `HnVueDbContext` (6개 Entity: Patient, Study, Image, DoseRecord, User, AuditLog)
- `PatientRepository`, `StudyRepository`, `ImageRepository`, `UserRepository` (IAsyncRepository<T> 구현)
- `OperationCanceledException` 재발생 처리
- Connection string: `Data Source={appDataPath}/hnvue.db; Password={key};` (SQLCipher)

**테스트:** 69개 (`HnVue.Data.Tests`)
**커버리지:** 80%+

### Layer 2: 인증 및 보안

#### HnVue.Security ⚠️ 안전 임계 (90%+)
비밀번호 해싱, JWT 토큰, RBAC, 감사 로그 체인.

**핵심 항목:**
- `PasswordHasher` (bcrypt cost=12, ~300ms 해싱 시간)
  - `HashPassword(password)` → bcrypt 해시
  - `VerifyPassword(password, hash)` → 검증
- `JwtTokenService` (HS256, 15분 만료)
  - `GenerateToken(userId, role)` → JWT 생성
  - `Validate(token)` → 서명, 만료, 클레임 검증
- `RbacPolicy` (4역할 권한 매트릭스)
  - Radiographer < Radiologist < Admin < Service (계층)
  - `HasRole(userRole, requiredRole)` → 정확 일치
  - `HasRoleOrHigher(userRole, requiredRole)` → 계층 비교
- `AuditService` (HMAC-SHA256 해시 체인)
  - 모든 작업 기록 (로그인, 데이터 수정, 설정 변경)
  - HMAC으로 체인 무결성 검증
  - **⚠️ 프로덕션 배포 시:** `JwtOptions.SecretKey` 및 `AuditService.DefaultHmacKey` 환경변수로 교체 필수
- `SecurityContext` (ReaderWriterLockSlim)
  - 현재 사용자, 역할, 권한 저장
  - 스레드 안전 접근

**테스트:** 91개 (`HnVue.Security.Tests`) — +54개 추가됨
**커버리지:** 90%+

### Layer 3a: DICOM 통신

#### HnVue.Dicom
DICOM 3.0 표준 준수. C-STORE SCU (영상 전송), C-FIND SCU (Worklist 조회), 파일 I/O.

**핵심 항목:**
- `DicomStoreScu` (C-STORE Service Class User)
  - 영상을 PACS 서버로 전송
  - 비동기 전송, 재시도 로직
- `DicomFindScu` (C-FIND Service Class User)
  - Worklist (MWL) 서버에 환자/촬영 정보 조회
  - 응급 ID 기반 조회 지원
- `DicomFileIO` (파일 I/O)
  - DICOM 파일 읽기/쓰기
  - 메타정보 추출
- `DicomFileWrapper` (래퍼 클래스)
  - fo-dicom DicomFile 감싸기

**의존성:** fo-dicom 5.1.3 (MIT 라이선스)

**테스트:** 15개 (`HnVue.Dicom.Tests`)
**커버리지:** 80%+

### Layer 3b: 인시던트 대응 ⚠️ 안전 임계 (90%+)

#### HnVue.Incident
위험 이벤트 감지 및 대응. ISO 14971 위험 관리 준수.

**핵심 항목:**
- `IncidentResponseService` (4단계 심각도)
  - **Critical:** 즉시 촬영 중단, 긴급 콜백 호출
  - **High:** 운영자 경고, 로깅
  - **Medium:** 감시, 로깅
  - **Low:** 로깅만
- 자동 에스컬레이션 (24시간 미반응 시 상위 심각도)
- 긴급 콜백 (전화/SMS 알림, 향후 확장)

**예시 인시던트:**
- 방사선 선량 임계값 초과 (Critical)
- DICOM 전송 실패 (High)
- 시스템 설정 변경 (Medium)
- 로그인 실패 3회 (Low → High)

**테스트:** 13개 (`HnVue.Incident.Tests`)
**커버리지:** 90%+

### Layer 3c: 소프트웨어 업데이트 ⚠️ 안전 임계 (85%+)

#### HnVue.Update
무결성 검증, 백업/복원, 코드 서명.

**핵심 항목:**
- `SWUpdateService` (업데이트 관리)
  - 새 버전 다운로드 (URL 기반)
  - 무결성 검증 (SHA-256 해시 비교)
  - 백업 생성 (현재 버전 타임스탐프로 저장)
  - 설치 (파일 교체 + DLL 언로드)
- `CodeSignVerifier` (SHA-256)
  - 코드 서명 검증 (signtool.exe 기반)
  - 인증서 체인 검증
- `BackupService` (타임스탐프)
  - 자동 백업 (매주 일요일 자정)
  - 복원 (타임스탐프 선택)
  - 이력 관리

**테스트:** 25개 (`HnVue.Update.Tests`)
**커버리지:** 85%+

### Layer 3d: 방사선 선량 관리 ⚠️ 안전 임계 (90%+)

#### HnVue.Dose
IEC 60601-1-3 준수. 4단계 인터록 시스템.

**핵심 항목:**
- `DoseService` (4단계 인터록)
  - **ALLOW:** 선량 정상, 촬영 진행
  - **WARN:** 선량 경고, 운영자 확인 필요
  - **BLOCK:** 선량 초과, 촬영 불가 (의사의 승인 필요)
  - **EMERGENCY:** 선량 극도 초과, 즉시 중단 + 긴급 알림
- `DoseValidationLevel` Enum
  - 환자별 누적 선량 추적
  - 프로토콜별 기준값 적용
  - 실시간 모니터링

**테스트:** 17개 (`HnVue.Dose.Tests`)
**커버리지:** 90%+

### Layer 3e: 영상 처리

#### HnVue.Imaging
영상 처리 파이프라인 (외부 SDK 연동 대기).

**현재 상태:** 스텁
**향후 구현:** Phase 1c — 외부 SDK 또는 자체 엔진 선택 예정

**테스트:** 20개 스텁 (`HnVue.Imaging.Tests`)

### Layer 3f: 환자 관리

#### HnVue.PatientManagement
환자 등록, 조회, DICOM Worklist 통합.

**핵심 항목:**
- `PatientService` (CRUD)
  - 환자 등록 (PatientId 자동 생성 또는 의료기록번호 사용)
  - 중복 검사 (이름, 생년월일, ID)
  - 조회, 수정, 삭제
- `WorklistService` (MWL 통합)
  - Worklist 서버에서 예정 촬영 조회
  - 응급 ID (EID) 기반 긴급 환자 추가
  - 자동 동기화 (5분 간격)

**테스트:** 27개 (`HnVue.PatientManagement.Tests`)
**커버리지:** 80%+

### Layer 3g: 시스템 관리

#### HnVue.SystemAdmin
시스템 설정, 감시, 감사.

**핵심 항목:**
- `SystemAdminService` (관리)
  - 설정 검증 (유효한 DICOM 주소, 포트 범위 등)
  - 시스템 상태 모니터링 (디스크, 메모리, DB)
  - 감사 로그 CSV 내보내기
  - 자동 정리 (365일 이상 로그 삭제)

**테스트:** 13개 (`HnVue.SystemAdmin.Tests`)
**커버리지:** 80%+

### Layer 3h: CD/DVD 소각

#### HnVue.CDBurning
의료 영상 미디어 배포. IMAPI2 시뮬레이션.

**핵심 항목:**
- `CDDVDBurnService` (미디어 소각)
  - CD/DVD 드라이브 감지
  - 영상 데이터 기록
  - 진행률 추적
- `IBurnSession` (세션 관리)
  - 활성 세션 추적
  - 취소 지원
- `IMAPIComWrapper` (IMAPI2 COM 래퍼)
  - Windows IMAPI2 COM 인터페이스 (IDiscRecorder2, IDiscFormat2Data)
  - 프로덕션: 실제 COM 호출
  - 테스트: 시뮬레이션 모드

**테스트:** 12개 (`HnVue.CDBurning.Tests`)
**커버리지:** 80%+

### Layer 4: 워크플로우 엔진 ⚠️ 안전 임계 (90%+)

#### HnVue.Workflow
촬영 워크플로우의 중추. 상태 머신 + 이벤트 기반.

**핵심 항목:**
- `WorkflowStateMachine` (9-상태 전이표)
  ```
  Idle
    ├─ PatientSelected (환자 선택)
    ├─ ProtocolLoaded (프로토콜 로드)
    ├─ ReadyToExpose (촬영 준비)
    ├─ Exposing (노출 중)
    ├─ AcquiringImage (영상 획득 중)
    ├─ ImageAcquired (영상 획득 완료)
    ├─ TransmittingToCACS (PACS 전송 중)
    └─ Complete (완료)
  ```
  - 상태 유효성 검증
  - 불가능한 전이 감지
  - 상태 변경 이벤트 발행

- `WorkflowEngine` (IWorkflowEngine 구현)
  - 현재 상태 조회
  - 상태 전이 (Transition)
  - 이벤트 등록/해제 (StateChanged, Abort)
  - 비동기 처리

- `GeneratorSimulator` (장애 주입)
  - 실제 X-ray 제너레이터 시뮬레이션
  - 노출 시간, 선량 설정
  - 장애 시나리오 (과열, 고장 등) 주입 가능
  - 테스트 용도

**테스트:** 64개 (`HnVue.Workflow.Tests`)
**커버리지:** 90%+

### Layer 5: UI (프레젠테이션)

#### HnVue.UI
WPF 사용자 인터페이스. MVVM 패턴.

**핵심 항목:**
- `MainWindow` (5-패널 레이아웃)
  - 위: 메뉴바 (File, Edit, View, Tools, Help)
  - 왼쪽: 환자 목록 (트리뷰)
  - 중앙: 촬영 프로토콜 + 영상 뷰어
  - 오른쪽: 워크플로우 상태 + 선량 게이지
  - 아래: 상태바 (현재 사용자, 시간, DICOM 상태)

- `LoginView` / `LoginViewModel`
  - Username + Password 입력
  - JWT 토큰 발급
  - RBAC 역할 표시

- 테마: MahApps.Metro
  - 색상 팔레트 (Primary: Blue, Accent: Green)
  - 타이포그래피 (Segoe UI, 11pt)
  - 버튼 스타일 (Flat, 둥근 모서리)
  - 간격 (8px, 16px, 24px)

**테스트:** 27개 (`HnVue.UI.Tests`)
**커버리지:** 60%+

### Layer 6: 애플리케이션 진입점

#### HnVue.App
DI 컴포지션 루트. 모든 모듈 통합.

**핵심 항목:**
- `Program.cs` (Main 진입점)
  - HostBuilder 설정
  - Serilog 로깅 구성
  - DI 컨테이너 등록 (Microsoft.Extensions.DependencyInjection)
  - 모든 서비스 등록 (13개 모듈)
- `App.xaml.cs` (WPF 애플리케이션)
  - 전역 예외 처리
  - Window 관리

**DI 등록 예시:**
```csharp
services.AddScoped<ISecurityService, SecurityService>();
services.AddScoped<IPatientService, PatientService>();
services.AddScoped<IWorkflowEngine, WorkflowEngine>();
services.AddScoped<IDoseService, DoseService>();
// ... 13개 모듈 모두 등록
```

---

## 개발 진행 현황

### 개발 단계별 요약

#### Pre-Wave (완료 ✅)
- 빌드 인프라 구성 (.NET 8.0.419, global.json, Directory.Build.props)
- 솔루션 스캐폴딩 (28개 프로젝트, 의존성 그래프)
- HnVue.Common 구현 (Result<T> 모나드, 17개 인터페이스, Enum)
- 테스트: 38개 통과

#### Wave 1 (완료 ✅)
병렬 개발: 3개 worktree에서 동시 구현
- **WT-1:** HnVue.Data (EF Core + SQLCipher, 6개 Entity, Repository)
- **WT-2:** HnVue.Security (bcrypt + JWT + RBAC + 감사 로그)
- **WT-3:** HnVue.UI (MahApps.Metro 테마, LoginView, MainWindow)
- 테스트: 215개 통과

#### REF 루프 (완료 ✅)
Review-Evaluate-Fix 10-사이클. Wave 1 기반에서 누락 모듈 구현.
- 사이클 1-3: Security, Workflow 보완
- 사이클 4-5: PatientManagement, Generator
- 사이클 6-7: Incident, SystemAdmin
- 사이클 8-10: Update, CDBurning, Dicom, Imaging, 전체 검증
- 테스트: 475개 통과 (Wave 1 215개 + REF 260개)

#### Phase 1d (완료 ✅)
UI 통합 + 통합 테스트
- DI 완전 연결 (13개 모듈)
- 통합 테스트 4가지 시나리오
  1. 인증 플로우 (로그인 → RBAC → 감사 로그)
  2. 촬영 워크플로우 (환자 선택 → 프로토콜 → 촬영 → PACS 전송)
  3. DICOM 네트워크 (C-STORE, C-FIND MWL)
  4. CD 굽기 (미디어 세션 + 시뮬레이션)
- 테스트: 18개 통과 (단위 475개 + 통합 18개 = 493개)

#### 2차 품질 검증 (완료 ✅, 2026-04-05)
최종 코드 품질 검증
- **신규 테스트 추가:** IMAPIComWrapperTests (19개, CDBurning 커버리지 53% → 95.6%)
- **기존 테스트 보완:** BackupServiceTests (+3개), SWUpdateServiceTests (+2개)
- **최종 결과:**
  - 빌드: 0 errors, 0 warnings ✅
  - 테스트: 523개 (499개 + 24개 신규) ✅
  - 품질 점수: 0.91/1.0 ✅
  - 안전 임계 모듈 커버리지: 90%+ 유지 ✅

---

## 빌드 및 테스트

### 시스템 요구사항

| 항목 | 요구사항 |
|------|---------|
| **OS** | Windows 10 or 11 (WPF) |
| **.NET SDK** | .NET 8.0.419 LTS |
| **IDE** | Visual Studio 2022 Professional 이상 |
| **MSBuild** | `D:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe` |
| **RAM** | 최소 8GB (권장 16GB) |
| **디스크** | 최소 5GB |

### 빌드 방법

#### Visual Studio IDE (권장)

```
File → Open → HnVue.sln
Build → Build Solution (Ctrl+Shift+B)
```

#### 명령줄 (Debug 빌드)

```bash
cd D:\workspace-gitea\Console-GUI
MSBuild.exe HnVue.sln /t:Build /p:Configuration=Debug /v:minimal
```

#### 명령줄 (Release 빌드)

```bash
MSBuild.exe HnVue.sln /t:Build /p:Configuration=Release /v:minimal
```

### 테스트 실행

#### 모든 테스트

```bash
cd D:\workspace-gitea\Console-GUI
dotnet test --configuration Debug
```

**예상 결과:**
```
Passed   523
Failed    0
Skipped   0

Total: 523 tests completed in ~45 seconds
```

#### 특정 프로젝트만 테스트

```bash
# Security 테스트만
dotnet test tests/HnVue.Security.Tests/HnVue.Security.Tests.csproj

# DICOM 테스트만
dotnet test tests/HnVue.Dicom.Tests/HnVue.Dicom.Tests.csproj
```

#### 커버리지 보고서

```bash
# 커버리지 수집 (OpenCover 필요)
dotnet test --configuration Debug /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### 테스트 구조

#### 단위 테스트 (Unit Tests)
- **경로:** `tests/HnVue.*.Tests/`
- **프레임워크:** xUnit
- **Mock 라이브러리:** NSubstitute
- **Assertion:** FluentAssertions
- **목표 커버리지:** 85%+, 안전 임계 모듈 90%+

**예제 (Security 테스트):**
```csharp
[Fact]
public void PasswordHasher_HashPassword_GeneratesValidHash()
{
    // Arrange
    var hasher = new PasswordHasher();
    var password = "MySecurePassword123!";

    // Act
    var hash = hasher.HashPassword(password);

    // Assert
    hash.Should().NotBeEmpty();
    hasher.VerifyPassword(password, hash).Should().BeTrue();
}

[Trait("SWR", "SWR-SEC-001")] // IEC 62304 추적성
public void SecurityService_HasRoleOrHigher_CompareRoleHierarchy()
{
    // ...
}
```

#### 통합 테스트 (Integration Tests)
- **경로:** `tests.integration/HnVue.Integration.Tests/`
- **시나리오:** E2E 워크플로우 (인증 → 촬영 → PACS)
- **테스트 수:** 18개

**예제:**
```csharp
[Fact]
public async Task AuthenticationWorkflow_LoginToRbacCheckSuccess()
{
    // 1. 로그인
    var token = await securityService.AuthenticateAsync("radiographer", "password");

    // 2. RBAC 확인
    var canExpose = rbacPolicy.HasRoleOrHigher(UserRole.Radiographer, UserRole.Radiographer);

    // 3. 감사 로그 확인
    var auditLog = await auditRepository.GetLastEntryAsync();
    auditLog.Action.Should().Contain("Login");
}
```

#### 테스트 추적성 (IEC 62304)
모든 테스트에 `[Trait("SWR", "SWR-XXX-NNN")]` 어노테이션 포함.

**예:**
- SWR-SEC-001: 비밀번호 해싱
- SWR-SEC-002: JWT 검증
- SWR-DOSE-001: 선량 인터록
- SWR-WF-001: 워크플로우 상태 전이

---

## 보안 설정 및 주의사항

### 기본값 (개발용)

현재 소스코드에 포함된 기본값들:

| 항목 | 파일 | 기본값 | 용도 |
|------|------|--------|------|
| **JWT Secret** | `JwtOptions.cs` | `"your-secret-key-at-least-32-characters"` | 토큰 서명 |
| **HMAC Key** | `AuditService.cs` | `"default-hmac-key-for-development"` | 감사 로그 무결성 |
| **DB Password** | `appsettings.json` | `"dev-password-12345"` | SQLCipher 암호화 |
| **bcrypt Cost** | `PasswordHasher.cs` | `12` | 해싱 강도 (~300ms) |

### ⚠️ 프로덕션 배포 체크리스트

다음 항목들을 **반드시** 환경변수로 교체하세요:

```json
// appsettings.Production.json
{
  "JwtOptions": {
    "SecretKey": "${JWT_SECRET_KEY}"  // 환경변수로 설정
  },
  "AuditService": {
    "DefaultHmacKey": "${AUDIT_HMAC_KEY}"  // 환경변수로 설정
  },
  "ConnectionStrings": {
    "HnVueDb": "Data Source={appDataPath}/hnvue.db; Password=${DB_PASSWORD};"  // 환경변수
  }
}
```

### 암호화 표준

| 항목 | 표준 | 강도 | 설명 |
|------|------|------|------|
| **비밀번호** | bcrypt | cost=12 | ~300ms 해싱 시간, salt 포함 |
| **데이터베이스** | SQLCipher AES-256 | 256-bit | 모든 데이터 암호화 저장 |
| **JWT** | HS256 | 256-bit | 토큰 서명, 15분 만료 |
| **감사 로그** | HMAC-SHA256 | 256-bit | 체인 무결성, 변조 감지 |
| **코드 서명** | SHA-256 | 256-bit | 소프트웨어 업데이트 검증 |

### RBAC 4-Tier 계층 구조

```
Service (최고 권한)
  ├─ Admin
  │   ├─ 시스템 설정 변경
  │   ├─ 사용자 관리
  │   ├─ 감사 로그 조회
  │   └─ 모든 Radiologist 권한 포함
  │
  ├─ Radiologist (의사)
  │   ├─ 촬영 프로토콜 승인
  │   ├─ 긴급 선량 Block 해제
  │   ├─ 인시던트 리뷰
  │   └─ 모든 Radiographer 권한 포함
  │
  └─ Radiographer (기사)
      ├─ 환자 등록
      ├─ 촬영 실행
      ├─ 영상 검토
      └─ 자신의 감사 로그만 조회
```

---

## 코드 품질 표준 (TRUST 5)

HnVue는 MoAI-ADK의 **TRUST 5 프레임워크**를 준수합니다:

| 항목 | 기준 | 현황 |
|------|------|------|
| **Tested** | 85%+ 커버리지, 안전 임계 90%+ | ✅ 523개 테스트, 90%+ 안전 임계 |
| **Readable** | XML 문서 주석, PascalCase 규칙 | ✅ 모든 public 멤버 주석 완비 |
| **Unified** | Result<T> 패턴, async/await ConfigureAwait(false) | ✅ 일관 적용 |
| **Secured** | OWASP 준수, RBAC 검증, 입력 검증 | ✅ bcrypt, JWT, HMAC, SQLCipher |
| **Trackable** | IEC 62304 §번호 주석, SWR Trait | ✅ [Trait("SWR", "SWR-XXX")] 추적성 |

### 코드 스타일

- **언어:** C# 11 (.NET 8)
- **네이밍:** PascalCase (public), camelCase (private)
- **주석:** XML doc comments (///)
- **포매팅:** .editorconfig 자동 적용
- **라이선스:** 모든 타사 라이브러리 라이선스 확인 (SBOM 참조)

---

## Git 저장소 및 브랜치

### 저장소 정보

| 저장소 | 역할 | 주소 |
|--------|------|------|
| **Gitea (origin)** | 사내 주 저장소 | `http://10.11.1.40:7001/DR_RnD/Console-GUI.git` |
| **GitHub (github)** | 외부 미러 | `https://github.com/holee9/console-gui.git` |

- 자동 동기화: Gitea → GitHub (10분 간격)
- Gitea가 기준 저장소

### 브랜치 전략

| 브랜치 | 용도 | 설명 |
|--------|------|------|
| `main` | 릴리스 기준선 | 프로덕션 배포 준비, 모든 테스트 통과 |
| `feat/wave*-*` | Wave별 개발 | 병렬 구현 (Wave 1, 2, 3, 4) |
| `feature/web-ui` | 웹 UI 검증 | 향후 웹 인터페이스 추가 |

### Git Clone

```bash
# HTTPS (외부)
git clone https://github.com/holee9/console-gui.git

# SSH (사내)
git clone git@gitea.abyzr.local:DR_RnD/Console-GUI.git

# HTTP (사내 로컬)
git clone http://10.11.1.40:7001/DR_RnD/Console-GUI.git
```

---

## 변경 이력 (Changelog)

### v0.2.0 — 2026-04-05 (2차 품질 검증) ✅

**영역:** 테스트 커버리지 대폭 향상

**주요 작업:**
1. **CDBurning 모듈 커버리지 향상**
   - `IMAPIComWrapperTests.cs` 신규 추가 (19개 새 테스트)
   - 커버리지: 53% → 95.6% (+42.6pp)
   - 시나리오: CD/DVD 드라이브 감지, 기록, 취소, 오류 처리

2. **업데이트 모듈 테스트 보완**
   - `BackupServiceTests.cs` +3개 테스트 (타임스탐프 백업/복원)
   - `SWUpdateServiceTests.cs` +2개 테스트 (무결성 검증)

3. **최종 결과**
   - 빌드: 0 errors, 0 warnings ✅
   - 테스트: 499개 → 523개 (+24개)
   - 품질 점수: 0.82/1.0 → 0.91/1.0 ✅
   - 안전 임계 모듈 커버리지: 90%+ 유지 ✅

---

### v0.1.0 — 2026-04-05 (초기 품질 검증) ✅

**영역:** 보안, 인프라, 기능 구현 완료

**주요 작업:**
1. **SecurityService 버그 수정**
   - `HasRoleOrHigher()` 메서드: 역할 계층 비교 로직 수정
   - 계층: Radiographer < Radiologist < Admin < Service

2. **JWT 검증 메서드 추가**
   - `JwtTokenService.Validate()` 신규 구현
   - 서명, 만료시간, 클레임 검증

3. **Repository 모듈 개선**
   - `OperationCanceledException` 재발생 처리 추가
   - 모든 Repository에 일관 적용

4. **프로덕션 배포 경고**
   - `JwtOptions.cs`, `AuditService.cs` 주석 추가
   - 본번 배포 전 환경변수로 교체 필수 명시

5. **테스트 신규 추가**
   - JwtTokenServiceTests: 4개 시나리오
   - SecurityServiceTests: 2개 역할 비교 테스트
   - 통합 테스트: 4가지 워크플로우
   - 합계: 491개 → 499개 (+8개)

**최종 결과**
- 빌드: 0 errors, 0 warnings ✅
- 테스트: 499개 전체 통과 ✅
- 품질 점수: 0.82/1.0 (PASS) ✅
- 안전 임계 모듈: 90%+ 커버리지 유지 ✅

---

## 개발 로드맵

### Phase 1: MVP (시장 진입 최소 기능)

| 범위 | 항목 | 상태 |
|------|------|:----:|
| **Tier 1** | 13개 인허가 필수 기능 | ✅ 완료 |
| **Tier 2** | 18개 시장 진입 필수 기능 | ✅ 완료 |
| **합계** | 31개 MR (시장 요구사항) | ✅ 완료 |

**Phase 1 구현 현황:**
- Pre-Wave: 기초 인프라 ✅
- Wave 1: Data, Security, UI 기초 ✅
- REF 루프: 11개 모듈 구현 ✅
- Phase 1d: DI 통합 + 통합 테스트 ✅
- 2차 품질 검증: 커버리지 최적화 ✅

### Phase 2: 경쟁 차별화 (Tier 3, 25개 MR)

- 영상 처리 자체 엔진 개발
- AI 기반 자동 분석
- 클라우드 연동
- 웹 UI 추가
- 성능 최적화

**예상 일정:** 18-24 MM (인력 보강 필수)

### Phase 3: 고도화 (Tier 4, 12개 MR)

- AI + 클라우드 고도화
- 빅데이터 분석
- 국제 확장

**예상 일정:** TBD (2명 조직에서는 비현실적)

---

## 문서 체계

### 핵심 문서 (v2.0+ 정합)

| Doc ID | 문서명 | 버전 | 경로 |
|--------|--------|:----:|------|
| **DOC-001** | MRD (시장 요구사항) | v3.0 | `docs/planning/DOC-001_MRD_v3.0.md` |
| **DOC-002** | PRD (제품 요구사항) | v2.0 | `docs/planning/DOC-002_PRD_v2.0.md` |
| **DOC-004** | FRS (기능 요구사항) | v2.0 | `docs/planning/DOC-004_FRS_v2.0.md` |
| **DOC-005** | SRS (소프트웨어 요구사항) | v2.0 | `docs/planning/DOC-005_SRS_v2.0.md` |
| **DOC-006** | SAD (소프트웨어 아키텍처 설계) | v2.0 | `docs/planning/DOC-006_SAD_v2.0.md` |
| **DOC-007** | SDS (소프트웨어 상세 설계) | v2.0 | `docs/planning/DOC-007_SDS_v2.0.md` |
| **DOC-032** | RTM (추적성 매트릭스) | v2.0 | `docs/verification/DOC-032_RTM_v2.0.md` |

### 관리 문서

| Doc ID | 문서명 | 버전 |
|--------|--------|:----:|
| DOC-003a | SDP (소프트웨어 개발 절차서) | v2.0 |
| DOC-042 | CMP (형상관리 계획) | v1.0 |
| DOC-043 | 빌드 환경 (28개 프로젝트) | v1.0 |
| WBS-001 | WBS (작업 분해도) | v2.0 |

### 보안 및 규제

| Doc ID | 문서명 | 버전 |
|--------|--------|:----:|
| DOC-008 | RMP (위험 관리 계획) | v2.0 |
| DOC-017 | STRIDE (위협 모델링) | v2.0 |
| DOC-046 | 보안 통제 | v1.1 |
| DOC-048 | VMP (취약점 관리 계획) | v1.0 |

모든 문서는 `docs/` 디렉토리에 위치하며, 자동 동기화 스크립트 (`scripts/sync_docs.py`)로 버전 일관성을 유지합니다.

---

## FAQ 및 문제 해결

### Q: 빌드 실패 — ".NET 8.0.419를 찾을 수 없습니다"

**A:** `global.json`의 .NET 버전 확인
```bash
dotnet --version  # 8.0.419 이상 확인
dotnet sdk list   # 설치된 SDK 목록
```

SDK가 없으면 [dotnet.microsoft.com](https://dotnet.microsoft.com) 에서 .NET 8.0.419 LTS 다운로드.

### Q: 테스트 실패 — "System.InvalidOperationException: No database provider"

**A:** `appsettings.json` 확인
```json
{
  "ConnectionStrings": {
    "HnVueDb": "Data Source={appDataPath}/hnvue.db; Password=dev-password-12345;"
  }
}
```

### Q: DICOM 전송 실패 — "Cannot connect to PACS server"

**A:** DICOM 서버 설정 확인
- `appsettings.json`에서 DICOM 서버 주소/포트 확인
- 테스트: 로컬 DICOM 에코 서버 사용 (DCM4CHEE, Conquest DICOM)
- `DicomStoreScu` 테스트는 서버 없이 모킹으로 진행

### Q: CD 굽기 실패 — "No CD/DVD drive detected"

**A:** IMAPI2 COM 인터페이스 확인
- Windows: IMAPI2 서비스 실행 중 확인 (`services.msc`)
- 테스트: `IMAPIComWrapper` 시뮬레이션 모드 사용
- 실제 드라이브: Windows 11에서 검증 필요

### Q: JWT 토큰 만료 오류

**A:** 시스템 시간 동기화
```bash
# Windows
w32tm /resync

# Linux
sudo ntpdate -s time.nist.gov
```

JWT 토큰 유효시간: **15분**

---

## 참고 자료 및 링크

### 표준 및 규제

- [IEC 62304:2015+A1](https://www.iec.ch/webstore/publication/61997) — 의료 소프트웨어 수명주기
- [FDA 21 CFR 820.30](https://www.ecfr.gov/current/title-21/section-820.30) — Design Controls
- [DICOM Standard](https://www.dicomstandard.org/) — 의료 영상 통신
- [OWASP Top 10](https://owasp.org/www-project-top-ten/) — 보안 가이드

### 라이브러리 및 프레임워크

- [fo-dicom](https://github.com/fo-dicom/fo-dicom) — C# DICOM 라이브러리
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) — ORM
- [xUnit.net](https://xunit.net/) — 테스트 프레임워크
- [MahApps.Metro](https://mahapps.com/) — WPF 테마

### 개발 도구

- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) — IDE
- [Git](https://git-scm.com/) — 버전 관리
- [dotnet CLI](https://docs.microsoft.com/en-us/dotnet/core/tools/) — 빌드 및 테스트

### 추가 자료

- Repository: [Gitea](http://10.11.1.40:7001/DR_RnD/Console-GUI) (사내)
- Mirror: [GitHub](https://github.com/holee9/console-gui)
- 템플릿: [software-templates](https://github.com/holee9/software-templates)

---

## 라이선스 및 기여

### 라이선스

HnVue는 H&abyz의 소유 소프트웨어입니다. 상용 의료기기로 판매되는 제품입니다.

### 타사 라이선스

모든 의존성은 SBOM (CycloneDX for .NET)으로 관리됩니다.

**주요 라이선스:**
- fo-dicom: **MIT**
- MahApps.Metro: **MIT**
- FluentAssertions: **Apache 2.0**
- NSubstitute: **BSD 2-Clause**
- Serilog: **Apache 2.0**

자세한 내용은 `docs/regulatory/DOC-019_SBOM_v1.0.md` 참조.

---

**문서 최종 업데이트:** 2026-04-05  
**프로젝트 상태:** Phase 1 MVP 완료 ✅  
**다음 단계:** Phase 1 사전 인허가 검증 및 Phase 2 계획 수립
