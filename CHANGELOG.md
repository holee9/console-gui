# HnVue 변경 이력

이 문서는 HnVue Console SW의 주요 변경 사항을 기록합니다.
형식: [Keep a Changelog](https://keepachangelog.com/ko/1.0.0/)

## [Unreleased]

### 추가
- 팀 기반 Worktree 분리 개발 운영 체계 구축
- QA 자동화 인프라 (SonarCloud, OWASP, Stryker.NET)
- RA 문서 자동화 스크립트 (SBOM, RTM)
- 이슈 추적 스크립트 (Gitea + GitHub, 한글 안전)
- 팀별 Claude Context 최적화 규칙
- CODEOWNERS 및 PR 체크리스트 템플릿
- role-matrix.md (CONSTITUTIONAL): 7팀 역할 경계 최상위 규약, CC 자가점검 4문, QA 독립성 보장
- 디렉토리 단위 소유권 테이블: DesignTime/ Design 단독 소유, 아키텍처 테스트로 검증

#### S08 R2 (2026-04-14)

- **Coordinator**: AddPatientProcedureViewModel DI 등록 + 통합테스트 보완
- **Design**: DesignTimeStudylistViewModel 정비 (샘플 데이터 5→10개 확장, Body Part 다양화)
- **Team A**: 디렉토리 소유권 아키텍처 테스트 추가 (DesignTime/ Design 단독 소유 검증)
- **RA**: role-matrix v2.0 반영, SAD/SDS 디렉토리 소유권 업데이트
- **QA**: S08-R2 품질게이트 PASS

#### S08 R1 (2026-04-14)

- **Coordinator**: StudylistViewModel 구현 (CommunityToolkit.Mvvm) + DI 등록 + DesignTime Mock + 통합테스트 2건
- **Design**: StudylistView.xaml 구현 (PPT slides 5-7, #102)
- **RA**: RTM v2.4 + 문서 동기화
- **Team B**: IDLE CONFIRM + DoseBranchCoverageTests 추가
- **QA**: 품질게이트 PASS (빌드 0에러, 482/482 테스트, 아키텍처 11/11, 모듈 경계 100%)

#### S07 R5 (2026-04-14)

- **QA**: 릴리즈 준비도 PASS (2,539/2,539 테스트, Flaky 0건, Safety-Critical 전원 PASS)
- **Team A**: Security flaky test 안정화 + 커버리지 보강
- **Team B**: 의료모듈 커버리지 보강

#### S07 R4 (2026-04-14)

- **Coordinator**: Flaky Test 안정화 + CDBurning race condition 수정
- **Team B**: Dicom 커버리지 86% 달성

#### S07 R3 (2026-04-14)

- **RA**: DOC-006 SAD v2.1 업데이트 (StudyItem 아키텍처 수정 반영, IStudyItem 인터페이스 분리), DOC-008 RMP v2.0 Approved 상태 확인

#### S07 R2 (2026-04-14)

- **RA**: DOC-032 RTM v2.2 승급 (Draft → Approved), DOC-042 CMP v2.1 Approved 상태 확인, DOC-006 SAD v2.0 아키텍처 규칙 추가 (구체 클래스 금지)
- **Team A**: Data 47.4% → 85% 커버리지 향상, SystemAdmin 66.7% → 85%, Update 75.7% → 90%, Common 83.9% → 85%
- **Team B**: Incident 70.2% → 90% (Safety-Critical), Detector 73.9% → 85%
- **Coordinator**: StudyItem 아키텍처 위반 수정 (구체 클래스 → 인터페이스 분리), 통합테스트 53→70개 확대

#### S07 R1 (2026-04-14)

- **RA**: DOC-042 CMP v2.1 완성 (IEC 62304 Section 8.2.2 백업/복구 절차 신설, CI-006 SDK 5종 상세화)
- **RA**: DOC-032 RTM v2.2 업데이트 (SWR-DET-010, SWR-DT-060, SWR-DT-061, SWR-WF-030 TC 매핑 12건)

### 변경

#### S05 R2 (2026-04-13)

- **Design**: WorkflowView PPT Slide 9-11 3-column layout (PatientInfoCard + AcquisitionPreview + Thumbnail strip)
- **Coordinator**: WorkflowView ViewModel enhancement (PreviewImage, ThumbnailList, SelectedPatient, WorkflowState) + 89 unit tests
- **Team A**: PHI AES-256-GCM encryption — PhiKeyDerivation (HKDF-SHA256) + NullPhiEncryptionService replaced
- **RA**: DOC-008 RMP v2.0 — 4-Tier priority system + MR-072 integrated, ISO 14971 compliant
- Issue tracking cleanup: PR #77-82 closed, Issue #59/60/61/62 closed with completion comments
- team-common.md: Issue Tracking Protocol added (mandatory pre-work issue registration)
- gitea-api.sh: Korean-safe API wrapper (curl U+FFFD workaround)

#### S05 R1 (2026-04-12)

- **Team A**: HnVue.Data EfRepository test reinforcement — 50 new tests, 135/135 passed
- **Team B**: Dicom/Update/Workflow coverage enhancement
- **Design**: LoginView/PatientListView/StudylistView PPT redesign verification
- **Coordinator**: MergeView ViewModel + UI.Contracts interface expansion
- **QA**: SPEC-GOVERNANCE-001 architecture tests + coverage policy
- **RA**: DOC-042 CMP v2.0 + DOC-019 SBOM v2.0

#### S04 R1+R2 (2026-04-11)

- CI 파이프라인 확장 (커버리지 게이트, 보안 스캔)
- S04 R1 QA 진입 게이트 평가: **CONDITIONAL PASS**
  - 테스트: 2,043 통과 / 14 실패 (1 flaky + 13 디자인준수)
  - Safety-Critical 커버리지: Dose 100%, Incident 94.4%, Security 92.3% (전원 90%+ PASS)
  - 전체 커버리지: 14/16 모듈 PASS (Views/Migrations/DesignTime 제외 정책 공식화)
  - Architecture Tests 4/4, Integration Tests 26/26 전원 통과
- S03 완료결과 main 통합, P0 빌드 에러 8건 수정 (commit 77a94bd)

---

## [0.6.0] - 2026-04-07 (FPD 검출기 추상화 + SDK 연동 체계)

### 추가
- **HnVue.Common 검출기 타입 추가** (6개 파일): DetectorState, DetectorTriggerMode, DetectorStatus, RawDetectorImage, IDetectorInterface 등
- **HnVue.Detector 신규 프로젝트** (8개 파일): DetectorSimulator (12-bit 노이즈 영상), OwnDetectorAdapter (자사 CsI FPD), VendorAdapterTemplate (타사 SDK 패턴)
- **SDK 폴더 체계 구축**: `sdk/own-detector/`, `sdk/third-party/`, 조건부 MSBuild 참조
- **WorkflowEngine 검출기 통합** (SWR-WF-030): PrepareExposureAsync에서 ArmAsync 호출, AbortAsync 병렬 중단
- **HnVue.Detector.Tests** (11개 테스트)

### 결과
- 빌드: 0 errors, 0 warnings
- 테스트: 1,124개 -> **1,135개** (+11)
- 변경 파일: 15개 신규, 4개 수정

---

## [0.5.0] - 2026-04-06 (GUI 교체 가능 아키텍처)

### 추가
- **HnVue.UI.Contracts 프로젝트** (20개 파일): INavigationService, IDialogService, IThemeService, 10개 ViewModel 인터페이스
- **HnVue.UI.ViewModels 프로젝트**: 11개 ViewModel을 HnVue.UI에서 분리
- **Design Token 3-Level 구조** (7개 XAML): CoreTokens, SemanticTokens, ComponentTokens, 3개 테마
- **DataTemplate ViewMappings + Shell Region 패턴**
- IEC 62366 안전 색상 (Safe/Warning/Blocked/Emergency) 3개 테마 모두 보장

### 수정
- HnVue.UI -> HnVue.UI.ViewModels 직접 참조 제거 (CRITICAL)
- 8개 code-behind에서 구체 ViewModel -> 인터페이스 타입 전환
- DI 등록을 인터페이스 기반으로 전환

### 결과
- 빌드: 0 errors
- 테스트: 812개 전체 통과
- 변경 파일: 60+ 파일

---

## [0.4.0] - 2026-04-06 (이미징/보안/워크플로우 확장)

### 추가
- **HnVue.Imaging** 8개 신규 메서드 (SWR-IP-039~052): GainOffset, NoiseReduction, EdgeEnhancement, ScatterCorrection, AutoTrimming, CLAHE, BrightnessOffset, BlackMask
- **HnVue.Security** 3개 신규: LogoutAsync, SetQuickPinAsync, VerifyQuickPinAsync (3회 실패 시 5분 잠금)
- **HnVue.Workflow** ISerialPortAdapter 인터페이스, 감사 로그 강화 (EXPOSURE_PREPARE/ABORT/SAFESTATE_CHANGED)
- **HnVue.Update** ApplyPackageAsync 완전 구현 (SHA-256 검증, ZipFile 추출, pending_update.json)
- **HnVue.UI** SafeState 색상 인디케이터 (SafeStateToColorConverter)

### 수정
- HnVue.Data cascade delete 변경: Patient->Study, Study->DoseRecord를 Restrict로 (IEC 62304 S5.5)
- DicomFindScu deprecated 처리

### 결과
- 빌드: 0 errors, 0 warnings
- 테스트: 743개 -> **812개** (+69)

---

## [0.3.0] - 2026-04-05 (코드 문서화)

### 추가
- GenerateDocumentationFile 활성화 (Directory.Build.props)
- 14개 모듈별 README.md 작성
- DocFX 설정 (docfx.json, modern template)

---

## [0.2.0] - 2026-04-05 (2차 품질 검증)

### 추가
- CDBurning IMAPIComWrapperTests (19개 테스트, 커버리지 53% -> 95.6%)
- BackupServiceTests +3개, SWUpdateServiceTests +2개

### 결과
- 테스트: 499개 -> 523개 (+24)
- 품질 점수: 0.82 -> 0.91

---

## [0.1.0] - 2026-04-05 (초기 품질 검증)

### 수정
- SecurityService HasRoleOrHigher() 역할 계층 비교 로직 수정
- OperationCanceledException 재발생 처리 전체 Repository 적용

### 추가
- JwtTokenService.Validate() 신규 구현
- 프로덕션 배포 경고 주석 추가
- JwtTokenServiceTests 4개, SecurityServiceTests 2개, 통합 테스트 4개

### 결과
- 테스트: 491개 -> 499개 (+8)
- 품질 점수: 0.82

---

## [1.0.0] - 2026-04-07

### 추가
- HnVue Console SW 초기 릴리즈
- 17개 프로덕션 모듈 (WPF .NET 8)
- 1,135개 테스트 (단위 1,117 + 통합 18)
- 216개 규제 문서 (FDA 510(k), CE MDR, KFDA)
- IEC 62304 Class B 준수
