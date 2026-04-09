# HnVue 개발 진행 현황 및 로드맵

> 원본: README.md "개발 진행 현황" + "개발 로드맵" 섹션에서 분리 (2026-04-09)

## 현재 진도 요약

> **현재 1.15M차 / 12M, 소진 2.3 MM / 24~36 MM** | 상세: [PROGRESS-001](PROGRESS-001_Status_Report_v1.0.md)

### 계획 대비 진도

| 항목 | 계획 (WBS v2.0) | 현재 (1.15M차) | 판정 |
|------|-----------------|---------------|------|
| **경과 / 전체** | 12M | 1.15M (9.6%) | - |
| **소진 MM / 전체** | 24~36 MM | 2.3 MM (9.6%) | - |
| **기능 완성도** | 100% @12M차 | ~45% @1.15M차 | 선행 |
| **WBS 완료율** | 100% @12M차 | ~30-35% @1.15M차 | 선행 |
| **MM 효율** | 1.0배 (계획) | 3.1~3.6배 (AI 병행) | - |

### 마일스톤

| MS | Milestone | 목표 | 잔여M | 잔여MM | 전망 |
|----|-----------|------|-------|--------|------|
| **MS1** | 설계 완료 | 2.5M차 | 1.35M | ~0.2 | ON TRACK |
| **MS2** | Tier 1 구현 | 6M차 | 4.85M | ~3.0 | AT RISK -- Generator/Detector HW |
| **MS3** | Tier 2 구현 | 8M차 | 6.85M | ~2.85 | AT RISK -- 영상처리 Stub, FPD SDK |
| **MS4** | 통합 테스트 | 9.5M차 | 8.35M | ~3.0 | WATCH |
| **MS5** | 시스템 테스트 | 10.5M차 | 9.35M | ~2.5 | WATCH |
| **MS6** | 릴리스 | 12M차 | 10.85M | ~2.5 | WATCH |

> CRIT 차단: Generator(0.5MM) + FPD SDK(0.55MM) + 영상처리(0.7MM) + PHI(0.2MM) = **1.95 MM**

### WBS 섹션별 MM 현황

| WBS 섹션 | 계획 기간 | 계획 MM | 소진 MM | 잔여 MM | 완료율 | 판정 |
|----------|----------|---------|---------|---------|--------|------|
| 0~4. 설계/문서 | 1~3M차 | 5.5~8.0 | 1.9 | 0.4~1.2 | 85%+ | 선행 |
| **5. Tier 1 구현** | **2~6M차** | **5.0~8.0** | **0.2** | **2.6~4.2** | **48%** | **정시** |
| **6. Tier 2 구현** | **2~8M차** | **5.0~7.0** | **0.1** | **3.3~4.6** | **34%** | **정시** |
| 7. 검증 | 3~10M차 | 4.0~6.0 | 0.1 | 3.2~4.8 | 20% | 선행 |
| 8~11. 시험/인허가/릴리스 | 9~12M차 | 4.5~7.5 | 0 | 4.5~7.5 | 5% | 정시 |
| **합계** | **1~12M차** | **24~36** | **2.3** | **21.7~33.7** | **30~35%** | - |

### CRIT 차단 항목 (합계 1.95 MM)

| 항목 | 잔여 MM | 차단 MS | 원인 |
|------|---------|---------|------|
| Generator RS-232/TCP | 0.5 MM | MS2 (6M차) | HW 프로토콜/장비 미확보 |
| FPD Detector SDK | 0.55 MM | MS2/MS3 | 벤더 SDK 미확보 |
| 영상처리 W/L/Zoom | 0.7 MM | MS3 (8M차) | 핵심 알고리즘 Stub |
| PHI AES-256-GCM | 0.2 MM | MS2 (6M차) | 구현 미착수 |

---

## 개발 단계별 요약

### Pre-Wave (완료)
- 빌드 인프라 구성 (.NET 8.0.419, global.json, Directory.Build.props)
- 솔루션 스캐폴딩 (28개 프로젝트, 의존성 그래프)
- HnVue.Common 구현 (Result<T> Monad, 17개 인터페이스, Enum)

### Wave 1 (완료)
병렬 개발: 3개 worktree에서 동시 구현
- WT-1: HnVue.Data (EF Core + SQLCipher)
- WT-2: HnVue.Security (bcrypt + JWT + RBAC + 감사 로그)
- WT-3: HnVue.UI (MahApps.Metro 테마, LoginView, MainWindow)

### REF 루프 (완료)
Review-Evaluate-Fix 10-사이클. Wave 1 기반에서 누락 모듈 구현.

### Phase 1d (완료)
UI 통합 + 통합 테스트 (DI 완전 연결, 4가지 시나리오)

### 2차 품질 검증 (완료, 2026-04-05)
IMAPIComWrapper 테스트 추가, CDBurning 커버리지 53% -> 95.6%

### Wave A + B 병렬 개발 (완료, 2026-04-05)
3개 worktree 병렬 개발 + UI 통합 + 보안 강화 (테스트: 812개)

### Phase 2 품질 강화 (완료, 2026-04-06)
Gitea 이슈 23개 해결 + 전체 모듈 README 업데이트

### Phase 3 품질 강화 (완료, 2026-04-06)
이슈 #27~#40 해결, 영상 파이프라인 완성

### Phase 4 완료 (완료, 2026-04-07)
JWT Denylist, RDSR, FPD 검출기 추상화, GUI 교체 가능 아키텍처 (테스트: 1,135개)

### 6팀 운영 인프라 구축 (완료, 2026-04-08)
Worktree 기반 팀 분리 개발, QA/RA 자동화, UISPEC 문서 체계, DOC-042 CMP v1.0 발행

### 현재 진행중 (2026-04-08 ~)
- UISPEC 기반 WPF UI 리디자인 (LoginView, PatientListView 착수)
- M1 설계 완료 준비 (STRIDE 상세화, RTM TC 매핑)
- ANALYSIS-003 교정 계획 Phase A 착수 대기

---

## 로드맵

### Phase 1: 기초 인프라 완성 (완료)
Tier 1+2 총 31개 MR 코드 기반 구축 완료.

> Phase 1은 코드 기반 완성을 의미하며, 제품 릴리즈 준비 완료가 아님.

### Phase 1.5: Gap 분석 & 정합성 복원 (현재 단계)

| 작업 | 상태 | 비고 |
|------|:----:|------|
| 기존 HnVUE IFU 기반 Feature Gap 매핑 | 미착수 | `Instructions for Use(EN) HnVUE 250714.docx` 기준 |
| 시험 보고서 현실화 (DOC-022~028) | 미착수 | 실 구현 기능 기준으로 재작성 |
| DOC-034 릴리즈 예정일 수정 | 미착수 | 2026-09-01 -> 2027 Q2~Q3로 수정 필요 |

### Phase 2: 핵심 기능 구현 (다음 단계)

| 작업 | 참조 자료 |
|------|---------|
| **HnVue.Imaging 실구현** (fo-dicom 기반 DICOM 파싱, 16-bit 렌더링, W/L/Pan/Zoom) | `API_MANUAL_241206.pdf`, `DICOM-001` |
| **WPF UI 화면 완성** (PatientListView, WorkflowView, ImageViewerView, DoseMonitorView) | `★HnVUE UI 변경 최종안_251118.pptx` |
| **Generator 실 프로토콜 구현** (RS-232/RS-422, Sedecal/CPI) | `GENERATOR-001` |
| **자사 FPD SDK 연동** (OwnDetectorAdapter NotImplementedException -> 실 SDK 호출 교체) | `sdk/own-detector/README.md` |

### Phase 3: 품질 완성 & 인허가 준비

- Production 보안 설정 (환경변수 기반 Secret 관리)
- DICOM 실환경 검증 (DCM4CHEE 또는 고객 PACS)
- 시험 재수행 (UT/IT/ST 실 데이터)
- KTL 사이버보안 모의침투 (IEC 81001-5-1)
- DHF/510(k)/CE 문서 최종화 및 인허가 제출

### Phase 4: 경쟁 차별화 (인허가 취득 후)

- 영상 처리 자체 엔진 고도화 (AI 기반 자동 분석)
- 클라우드 연동, 웹 UI 추가, 성능 최적화

**현실적 1차 릴리즈: 2027년 Q2~Q3**

---

문서 최종 업데이트: 2026-04-09
