# HnVue Console SW Sprint 구현계획

| 항목 | 내용 |
|------|------|
| **문서 ID** | SPRINT-001 |
| **버전** | v2.0 |
| **작성일** | 2026-04-10 |
| **최종 개정일** | 2026-04-10 |
| **기준 문서** | WBS-001 v3.1, PROGRESS-002 v1.0, DISPATCH Round 3 |
| **일정 단위** | Sprint (1 Sprint = 0.5 MM, AI 에이전트 1 작업 세션) |
| **현재 시점** | S03 Round 3 (1.5M차, 소진 3.0 MM / 24--36 MM) |
| **6팀 체계** | Team A (인프라), Team B (의료영상), Design (UI), Coordinator (통합), QA (품질), RA (인허가) |

---

## 문서 개정 이력

| 버전 | 일자 | 개정 내용 | 작성자 |
|------|------|-----------|--------|
| v1.0 | 2026-04-10 | 최초 작성 -- WBS v3.0 기반 6팀 Sprint 구현계획 수립 | MoAI |
| **v2.0** | **2026-04-10** | **전면 교차검증 개정**: (1) S03 실제 빌드/테스트 결과 반영 (8에러, 13F); (2) Round 1-2 완료 실적 + Round 3 DISPATCH 교차검증; (3) 빌드 차단(P0) 우선순위 재조정; (4) 커버리지 갭 분석 정밀화 (모듈별 floor 추가); (5) SPEC-DISPATCH 정합성 검증 추가; (6) S04 진입 게이트 조건 명확화 | MoAI |

---

## 1. Sprint 체계 개요

### 1.1 단위 정의

| 단위 | 정의 | 환산 |
|------|------|------|
| **Sprint** | AI 에이전트 1 작업 세션 (DISPATCH 1회 주기) | 0.5 MM |
| **MM** | Man-Month (인월), SW 1명 x 1개월 | 2 Sprint |
| **Phase 1** | S01 -- S24 (12개월, 24--36 MM) | 24 Sprint |
| **Round** | Sprint 내 DISPATCH 발행 주기 (1 Sprint = 다수 Round 가능) | -- |

### 1.2 팀별 Sprint 투입 계획 (검증 갱신)

| Team | 총 MM | Sprint 범위 | 병렬도 | S03 잔여 |
|------|-------|------------|--------|---------|
| Team A | 8.5 MM | S01--S12 | Team B와 병렬 | Round 3: 빌드오류+패키지+StyleCop |
| Team B | 9.0 MM | S01--S12 | Team A와 병렬 | Round 3: 빌드오류+커버리지 |
| Design | 4.5 MM | S01, S03--S07 | 구현팀과 병렬 | Round 3: 빌드오류+Converter |
| Coordinator | 2.0 MM | S03, S07, S13--S14 | 통합 시점 집중 | Round 3: ViewModel+Contracts |
| QA | 4.5 MM | S03--S22 (분산) | 전 마일스톤 게이트 | Round 3: CI Gate+E2E |
| RA | 3.0 MM | S04, S11, S15, S18--S24 | 전 마일스톤 게이트 | Round 3: SBOM/RTM/CMP |

---

## 2. S03 교차검증 현황 (2026-04-10 기준)

### 2.1 빌드 상태

**솔루션 빌드 결과: 8 에러, 9,158 경고**

| 에러 | 파일 | 유형 | 담당팀 | DISPATCH 매핑 |
|------|------|------|--------|-------------|
| CS0051 TestStatus 접근성 | ConverterTests.cs:589 | 접근성 불일치 | Design | Task 1 (P0) |
| CS1061 IUserRepository.AddAsync (x6) | TeamAIntegrationTests.cs:80,118,164,213,245,282 | 메서드 미정의 | Coordinator | 미매핑 -- **신규 추가 필요** |
| CS7036 AuditEntry 생성자 (x1) | TeamAIntegrationTests.cs:184 | 매개변수 누락 | Coordinator | 미매핑 -- **신규 추가 필요** |

### 2.2 Round 1-2 실적 종합

| Team | Round 1 | Round 2 | 주요 산출물 |
|------|---------|---------|-----------|
| Team A | DONE | DONE | SPEC-INFRA-001 12개 구현, 440 테스트, NuGet 9.0.0 업그레이드 |
| Team B | DONE | DONE (허위 보고 포함) | 123개 신규 테스트, Detector 91.7%, Dose 99.5%, Dicom 43%(PARTIAL) |
| Design | DONE | DONE | LoginView+PatientListView PPT 리디자인 완료 |
| Coordinator | -- | 미실행 | DI 교체 미착수 |
| QA | DONE | 미실행 | 로컬 분석기 구축 |
| RA | DONE | 미실행 | Round 1 규제문서 정비 |

### 2.3 커버리지 갭 분석 (모듈별)

| 모듈 | 현재 | 목표 | 갭 | 담당 | DISPATCH |
|------|------|------|-----|------|---------|
| **Detector** | 42.6% | 85% | 42.4pp | Team B | Task 2 |
| **Dose** | 67.6% | 90% | 22.4pp | Team B | Task 3 (Safety) |
| **Security** | 82.5% | 90% | 7.5pp | Team A | 미매핑 |
| **Dicom** | 66.9% | 80% | 13.1pp | Team B | Task 4 |
| **PatientMgmt** | 72.7% | 80% | 7.3pp | Team B | Task 5 |
| **UI.ViewModels** | 42.0% | 75% | 33.0pp | Coordinator | Task 1 |
| **UI.Contracts** | 42.8% | 70% | 27.2pp | Coordinator | Task 2 |
| Imaging | 87.5% | 85% | -- | -- | 달성 |
| Incident | 94.2% | 90% | -- | -- | 달성 |
| Workflow | 91.4% | 85% | -- | -- | 달성 |
| CDBurning | 100% | 80% | -- | -- | 달성 |
| **전체** | **75.6%** | **85%** | **9.4pp** | 전체 | -- |

### 2.4 DISPATCH-SPEC 정합성 검증

| DISPATCH | 매핑 SPEC | 정합성 | 발견 갭 |
|----------|----------|--------|---------|
| Team A Round 3 | SPEC-INFRA-001 | 부분적 | Security 90% 커버리지 목표 미반영 |
| Team B Round 3 | SPEC-TEAMB-COV-001 | 정합 | 빌드오류 Task 1 추가 (Round 2 미탐지) |
| Design Round 3 | SPEC-UI-001 | 부분적 | Converter 테스트만, UISPEC 구현 미포함 |
| Coordinator Round 3 | -- | 미매핑 | 독립 SPEC 부재 (Coordinator 역할 SPEC 필요) |
| QA Round 3 | SPEC-GOVERNANCE-001 | 부분적 | CI Gate 자동화만, 거버넌스 이슈 추적 미포함 |
| RA Round 3 | -- | 부분적 | SBOM/RTM/CMP만, DOC-042 완성 우선순위 낮음 |

---

## 3. Sprint별 상세 구현계획

### Sprint S01--S02 (0 -- 1.0 MM) -- 완료

**상태: DONE** (프로젝트 시작 ~ 2026-04 초)

| Team | 작업 | WBS ID | 상태 | 산출물 |
|------|------|--------|------|--------|
| Team A | RBAC 4역할 + bcrypt(cost=12) + 5회 잠금 | 5.1.1--3 | **DONE** | AuthService, 테스트 |
| Team A | PHI SQLCipher AES-256 기본 설정 | 5.1.4 | 진행중 (60%) | DbContext 암호화 |
| Team B | DICOM C-STORE SCU + MWL C-FIND | 5.1.8--9 | **DONE** | DicomService |
| Team B | 영상처리 W/L, Zoom, Pan, Rotate | 5.2.3--5 | **DONE** | ImageProcessor 13메서드 |
| Design | LoginView + PatientListView PPT 리디자인 | -- | **DONE** | XAML + DesignTime VM |

**Sprint 성과**: 17개 모듈 구조 완성, ~900개 테스트, 서비스 계층 대부분 구현

---

### Sprint S03 (1.0 -- 1.5 MM) -- 현재 진행 (Round 3)

**상태: ACTIVE** (2026-04-10 기준, Round 3 DISPATCH 발행됨)

#### S03 Round 3 DISPATCH 현황 (6팀)

| Team | Round 3 상태 | P0 차단 | P1 | P2+ | 검증 |
|------|-------------|---------|-----|------|------|
| Team A | NOT_STARTED | 빌드오류 1건 | 패키지 업그레이드 | StyleCop | 빌드+테스트 |
| Team B | NOT_STARTED | 빌드오류 1건 | 커버리지 3개 모듈 | -- | 빌드+테스트 |
| Design | NOT_STARTED | 빌드오류 1건 | Converter 12개 | ThemeRollback | 빌드+테스트 |
| Coordinator | NOT_STARTED | **IntegrationTests 7건** | ViewModel+Contracts | Integration | 빌드+테스트 |
| QA | NOT_STARTED | -- | CI Gate | E2E+Perf | 스크립트 실행 |
| RA | NOT_STARTED | -- | SBOM/SOUP | RTM+CMP | 문서 정합성 |

#### S03 Round 3 실행 순서 (의존성 기반)

```
Phase 0 (P0-Blocker 해소, 병렬):
  Team A Task 1 ─── SystemSettingsRepository 빌드오류
  Team B Task 1 ─── VendorAdapterTemplate 빌드오류
  Design Task 1 ─── ConverterTests 빌드오류
  Coordinator ──── IntegrationTests 빌드오류 (신규 Task 0)

Phase 1 (빌드 안정화 확인):
  QA ─── 솔루션 전체 빌드 0 에러 확인

Phase 2 (커버리지 Push, 병렬):
  Team A Task 2-3 ─── 패키지+StyleCop
  Team B Task 2-5 ─── Detector→Dose→Dicom→PM 커버리지
  Design Task 2-4 ─── Converter→Theme→Component
  Coordinator Task 1-3 ─── ViewModel→Contracts→Integration
  QA Task 1-4 ─── CI Gate→E2E→Perf→Trend
  RA Task 1-3 ─── SBOM(blocked by Team A)→RTM→CMP

Phase 3 (통합 검증):
  전체 솔루션 빌드 + 테스트 통과 확인
  커버리지 리포트 갱신
```

#### S03 완료 기준 (Round 3 종료 시)

| 게이트 | 기준 | 현재 | 간격 |
|--------|------|------|------|
| 빌드 에러 | 0 | 8 | **P0** |
| 테스트 실패 | 0 | 13 | **P0** |
| 전체 커버리지 | 80%+ | 75.6% | 4.4pp |
| Safety-Critical | Dose 85%+, Incident 90%+ | Dose 67.6% | 17.4pp |
| UI 테스트 | 전원 통과 | 13F | **P0** |

---

### Sprint S04 (1.5 -- 2.0 MM) -- 계획

**S04 진입 Gate: S03 Round 3 완료 조건 전부 충족**

| Team | 작업 | WBS ID | 선행 조건 | 산출물 |
|------|------|--------|----------|--------|
| Team A | PHI AES-256-GCM 완성 | 5.1.4 | S03 SQLCipher 기반 | GCM 암호화 + 테스트 |
| Team A | Security 82.5% -> 90%+ 커버리지 | -- | S03 완료 | 보안 테스트 보강 |
| Team B | DICOM Print SCU 구현 | 5.1.10 | fo-dicom 5.x 기반 | PrintScuService |
| Team B | Dose 90%+ Branch 커버리지 확보 | -- | S03 기반 유지 | Branch 테스트 |
| Design | WPF MVVM 프레임워크 (1/3) | 5.2.16 | MahApps.Metro 설정 | 공통 컴포넌트 |
| Design | UISPEC-003 StudylistView 구현 | -- | PPT 리디자인 | StudylistView.xaml |
| Coordinator | **Null Stub 6개 교체 완료** | -- | Repository 구현 완료 | App.xaml.cs DI 정상화 |
| QA | xUnit Tier1 단위테스트 시작 | 7.1.1 | CI 파이프라인 완성 | 커버리지 리포트 |
| RA | RTM 초안 + STRIDE 위협모델 검토 | -- | SRS v2.0 확정 | RTM 초안본 |
| RA | DOC-042 CMP Draft -> Review 전환 | -- | 필드 완성 | CMP v1.0 승인 |

**Sprint Gate**: Coordinator I1 준비 -- DI 통합 검증 가능 상태

---

### Sprint S05 (2.0 -- 2.5 MM) -- M1 설계 완료

| Team | 작업 | WBS ID | 산출물 |
|------|------|--------|--------|
| Team A | SW 업데이트 Authenticode + 롤백 (1/2) | 5.1.14--16 | UpdateService 코드서명 |
| Team A | 인시던트 대응 CVD 프로세스 (1/2) | 5.1.12--13 | IncidentService 완성 |
| Team B | 선량 관리 DAP/DRL (1/1) | 5.2.7--8 | DoseDisplayView + 인터락 |
| Team B | CD/DVD 버닝 IMAPI2 (1/2) | 5.2.10--13 | CDBurnService 완성 |
| Design | WPF MVVM 프레임워크 (2/3) | 5.2.16 | View-ViewModel 바인딩 |
| Design | UISPEC-005 AddPatientProcedure 구현 | -- | AddPatientProcedureView.xaml |
| QA | DI 통합테스트 실행 | -- | 통합테스트 Green |

#### 마일스톤 M1: 설계 완료 (5.0 MM)

| 게이트 항목 | 완료 기준 | 담당 |
|------------|----------|------|
| SWR 전체 SAD/SDS 반영 | SRS -> SAD/SDS 추적 100% | RA |
| STRIDE 위협 모델 완성 | DOC-017 v1.0 승인 | Team A |
| Tier 1+2 SWR 추적성 | RTM 초안 검증 | RA |
| 테스트 전략 확정 | 테스트 계획서 승인 | QA |

#### 통합 마일스톤 I1: DI 통합 검증 (5.0 MM)

| 게이트 항목 | 완료 기준 | 담당 |
|------------|----------|------|
| Null Stub 6개 교체 | App.xaml.cs DI 정상 등록 | Coordinator |
| 솔루션 빌드 0 error | dotnet build 성공 | QA |
| DI 통합테스트 Green | 전체 DI 해석 테스트 통과 | Coordinator + QA |
| DOC-042 CMP 승인 | Draft -> Review -> Approved | RA |

---

### Sprint S06--S07 (2.5 -- 3.5 MM)

| Team | 작업 | WBS ID | Sprint |
|------|------|--------|--------|
| Team A | SW 업데이트 완성 + 인시던트 대응 완성 | 5.1.14--16, 5.1.12--13 | S06 |
| Team A | SBOM CycloneDX CI 통합 | 5.1.7 | S07 |
| Team A | STRIDE 보안통제 구현 (1/2) | 5.1.17 | S07 |
| Team B | CD/DVD 버닝 완성 (암호화 + 감사로그) | 5.2.10--13 | S06 |
| Team B | 환자 관리 MWL 폴링 + 검색 | 5.2.1, 5.2.17 | S06 |
| Team B | FPD SDK 통합 시작 | 5.2.9 | S07 (crit: 벤더 SDK) |
| Design | WPF MVVM 프레임워크 완성 (3/3) | 5.2.16 | S06 |
| Design | UISPEC-004 Acquisition + UISPEC-006 Merge 구현 | -- | S07 |
| Coordinator | UI.Contracts 인터페이스 관리 | -- | S07 |
| QA | xUnit Tier1 단위테스트 (계속) | 7.1.1 | S06--S07 |

---

### Sprint S08--S09 (3.5 -- 4.5 MM)

| Team | 작업 | WBS ID | Sprint |
|------|------|--------|--------|
| Team A | STRIDE 보안통제 완성 (2/2) | 5.1.17 | S08 |
| Team A | TLS 1.3 네트워크 암호화 | 5.1.5 | S09 |
| Team A | 세션 자동 잠금 JWT 15분 | 5.1.20 | S09 |
| Team B | Generator RS-232 (1/2) | 5.1.18 | S08 (crit: HW) |
| Team B | Generator RS-232 (2/2) | 5.1.18 | S09 (crit: HW) |
| Design | UISPEC-007 Settings + UISPEC-009 SystemAdmin | -- | S09 |
| QA | xUnit Tier1 단위테스트 (계속) | 7.1.1 | S08--S09 |

---

### Sprint S10--S12 (4.5 -- 6.0 MM) -- M2 Tier 1 구현

| Team | 작업 | WBS ID | Sprint |
|------|------|--------|--------|
| Team A | 에러 처리 매트릭스 완성 | WP-T1-ERR | S10 |
| Team A | 시스템 관리 완성 | -- | S11--S12 |
| Team B | 선량 인터락 안전 로직 | 5.1.19 | S10 |
| Team B | DICOM RDSR 생성 + 전송 | 5.2.18 | S10 |
| Design | UISPEC-008 ImageViewer 구현 | -- | S11--S12 |
| QA | Tier1 정적분석 + STRIDE 보안테스트 | 7.2.3 | S11 |
| RA | RTM 중간본 + SOUP 분석 + SBOM 갱신 | -- | S11 |

#### 마일스톤 M2: Tier 1 구현 (12.0 MM)

| 게이트 항목 | 완료 기준 | 담당 |
|------------|----------|------|
| Tier 1 WBS 전체 구현 | 5.1.1--5.1.20 + WP-T1-ERR 완료 | Team A + B |
| Tier 1 단위테스트 85%+ | 커버리지 리포트 통과 | QA |
| STRIDE 6시나리오 보안테스트 | 테스트 결과 보고 | QA |
| RTM 중간본 승인 | Tier 1 SWR 100% 매핑 | RA |
| SOUP 분석 완료 | DOC-033 갱신 | RA |
| 안전임계 90%+ | Dose, Incident Branch 커버리지 | QA |

#### 통합 마일스톤 I2: Tier 1 통합 빌드 (12.0 MM)

| 게이트 항목 | 완료 기준 | 담당 |
|------------|----------|------|
| 솔루션 전체 빌드 | 0 error, 0 warning (경고 임계 이내) | 전체 |
| 통합테스트 Green | Tier 1 모듈 간 통합테스트 통과 | QA + Coordinator |
| 커버리지 85%+ | 전체 커버리지 게이트 통과 | QA |

---

### Sprint S13--S16 (6.0 -- 8.0 MM) -- M3 Tier 2 구현

| Team | 작업 | WBS ID | Sprint |
|------|------|--------|--------|
| Team B | PACS 비동기 전송 파이프라인 | 5.2.2 | S13 |
| Team B | FPD SDK 통합 완성 | 5.2.9 | S13--S14 (crit) |
| Coordinator | Integration Tests 전체 | -- | S13--S14 |
| QA | xUnit Tier2 단위테스트 | 7.1.2 | S13--S15 |
| RA | RTM 갱신 (Tier 2 SWR 매핑) | -- | S15 |
| QA | 성능 벤치마크 PACS 30초 | 7.3.2 | S16 |

#### 마일스톤 M3: Tier 2 구현 (16.0 MM)

| 게이트 항목 | 완료 기준 | 담당 |
|------------|----------|------|
| Tier 2 WBS 전체 구현 | 5.2.1--5.2.18 완료 | Team B + Design |
| Tier 2 단위테스트 85%+ | 커버리지 리포트 통과 | QA |
| PACS 전송 30초 이내 | 성능 벤치마크 통과 | QA |
| FRS/SRS 추적성 검증 | RTM 갱신 완료 | RA |
| UISPEC 전체 구현 | UISPEC-001~009 구현 완료 | Design |

#### 통합 마일스톤 I3: Tier 2 통합 빌드 (16.0 MM)

| 게이트 항목 | 완료 기준 | 담당 |
|------------|----------|------|
| 전체 회귀테스트 | 모든 기존 테스트 Green | QA |
| UI E2E 기본 통과 | 촬영 워크플로우 시나리오 | QA + Design |
| 커버리지 85%+ 전체 | 모든 모듈 게이트 통과 | QA |
| Architecture 규칙 검증 | NetArchTest 전체 통과 | QA |

---

### Sprint S17--S19 (8.0 -- 9.5 MM) -- M4 통합 테스트

| Team | 작업 | WBS ID | Sprint |
|------|------|--------|--------|
| QA | 통합테스트 Tier 1 (DICOM, 보안, 워크플로우) | 7.2.1 | S17--S18 |
| QA | 통합테스트 Tier 2 (MWL, CD 버닝, 선량) | 7.2.2 | S17--S18 |
| QA | 인시던트 대응 검증 | 7.2.4 | S17 |
| QA | SW 업데이트 검증 (서명/롤백) | 7.2.5 | S18 |
| QA | CD 버닝 검증 (실 미디어) | 7.2.6 | S18 |
| Team B | DICOM Conformance Statement | 9.1 | S17 |
| RA | FMEA 최종본 + 잔여위험 평가 + SBOM 검증 | -- | S18 |

#### 마일스톤 M4: 통합 테스트 (19.0 MM)

| 게이트 항목 | 완료 기준 | 담당 |
|------------|----------|------|
| Tier 1+2 통합테스트 전체 통과 | 0 fail | QA |
| STRIDE 6시나리오 실행 | 보안테스트 리포트 | QA |
| 잔여위험 수용 가능 | FMEA 최종본 승인 | RA |

#### 통합 마일스톤 I4: RC 빌드 (19.0 MM)

| 게이트 항목 | 완료 기준 | 담당 |
|------------|----------|------|
| RC 빌드 생성 | Release 빌드 + 코드 서명 | Team A |
| DOC-034 릴리스 체크리스트 | 80%+ 항목 통과 | QA |
| DHF 편찬 시작 | DOC-035 초안 | RA |
| eSTAR 초안 | DOC-036 구조 확정 | RA |

---

### Sprint S20--S21 (9.5 -- 10.5 MM) -- M5 시스템 테스트

| Team | 작업 | WBS ID | Sprint |
|------|------|--------|--------|
| QA | E2E 촬영 워크플로우 테스트 | 7.3.1 | S20 |
| QA | 침투 테스트 (외부 전문가) | 7.3.3 | S20 |
| QA | 사용성 Summative (방사선사 10명+) | 7.3.4 | S21 |
| RA | V&V Summary 검토 + RTM 최종본 | -- | S20 |

#### 마일스톤 M5: 시스템 테스트 (21.0 MM)

| 게이트 항목 | 완료 기준 | 담당 |
|------------|----------|------|
| E2E 테스트 통과 | 촬영 워크플로우 전체 | QA |
| 침투 테스트 완료 | 외부 보고서 수령 | QA |
| Summative Usability 통과 | IEC 62366-1 준수 확인 | QA |
| RTM 최종본 승인 | 100% SWR->TC 매핑 | RA |
| V&V Summary Report | DOC-025 완성 | QA |
| Known Anomalies 0건 | DOC-044 확인 | QA + RA |

---

### Sprint S22--S24 (10.5 -- 12.0 MM) -- M6 릴리스

| Team | 작업 | WBS ID | Sprint |
|------|------|--------|--------|
| QA | VnV Summary Report 완성 | 9.4 | S22 |
| QA | IEC 62304 + IEC 81001-5-1 체크리스트 | 9.9--9.10 | S22 |
| RA | DHF 편찬 완성 | 9.5 | S22--S23 |
| RA | eSTAR 510(k) 패키지 | 9.6 | S23--S24 |
| RA | CE Technical Documentation | 9.7 | S23--S24 |
| RA | KFDA 기술 문서 | 9.8 | S23--S24 |

#### 마일스톤 M6: 릴리스 (24.0 MM)

| 게이트 항목 | 완료 기준 | 담당 |
|------------|----------|------|
| DHF 최종 승인 | DOC-035 4-signature | RA |
| eSTAR 패키지 완성 | FDA 제출 준비 완료 | RA |
| CE Technical Doc | MDR 준수 확인 | RA |
| SBOM 최종본 | CycloneDX 최종 검증 | RA + QA |
| 릴리스 빌드 | Authenticode 서명 + 설치 패키지 | Team A |
| 릴리스 체크리스트 | DOC-034 10항목 전체 Pass | QA |

---

## 4. CRITICAL 차단 항목 및 해소 계획

| ID | 항목 | 잔여 MM | 차단 Sprint | 해소 전략 | 담당 | 상태 |
|----|------|---------|-----------|----------|------|------|
| **C-0** | **솔루션 빌드 8에러** | 즉시 | **S03 R3** | 각 팀 P0 Task 최우선 | 4팀 | **BLOCKING** |
| **C-1** | App.xaml.cs Null Stub 6개 DI 교체 | 0.1 | **S04** | Coordinator DISPATCH | Coordinator | 미착수 |
| **C-4** | Generator RS-232 실 구현 | 0.5 | S08--S09 | 시뮬레이터 우선 | Team B | 계획 |
| **C-5** | FPD Detector SDK 통합 | 0.5 | S07--S08 | 어댑터 패턴 | Team B | 계획 |
| **H-1** | PHI AES-256-GCM 완성 | 0.15 | S04 | SQLCipher GCM 전환 | Team A | 계획 |
| **H-2** | TLS 1.3 네트워크 | 0.2 | S09 | SslStream + X.509 | Team A | 계획 |
| **H-7** | STRIDE 구현 완성 | 0.3 | S07--S08 | 보안통제 6카테고리 | Team A | 계획 |
| **H-8** | DOC-042 CMP 미완성 | 0.05 | S04 | Draft->Review->Approved | RA | 계획 |

---

## 5. 팀 간 의존성 매트릭스

```
Team A ──────> Coordinator (Common 인터페이스 변경 시 승인)
Team B ──────> Coordinator (Workflow/Detector 인터페이스 변경 시)
Design ──────> Coordinator (UI.Contracts 바인딩 의존)
Team A ──────> RA (NuGet 추가 시 SOUP/SBOM 갱신 알림)
Team B ──────> RA (안전임계 변경 시 FMEA 리뷰)
QA     ──────> RA (커버리지/보안 결과 -> 규제 문서 반영)
Coordinator -> 전체 (DI 변경, 통합 빌드 결과 공유)
RA Task 1 ──> Team A Task 2 (SBOM/SOUP blocked by 패키지 변경)
```

---

## 6. Sprint 실행 프로토콜 (DISPATCH 연계)

### 6.1 Sprint 실행 주기

```
1. Sprint 시작: 각 팀 DISPATCH.md 발행
2. 팀별 Worktree 작업: 독립 브랜치에서 구현
3. 팀별 완료: Git push + PR 생성
4. 통합 검증: Coordinator 솔루션 빌드 + 테스트
5. Sprint 종료: QA 커버리지 확인 + 마일스톤 게이트 (해당 시)
```

### 6.2 DISPATCH 발행 기준

| 조건 | DISPATCH 대상 |
|------|-------------|
| Sprint 시작 | 해당 Sprint 작업이 있는 팀 전체 |
| CRITICAL 차단 해소 | 차단된 팀 + Coordinator |
| 마일스톤 도달 | QA + RA + Coordinator |
| 통합 마일스톤 | 전체 6팀 |

### 6.3 DISPATCH 완료 기준 (Round 1 사고 교훈 적용)

| 항목 | 필수 |
|------|------|
| 자기 모듈 빌드 성공 | **HARD** |
| 자기 테스트 통과 | **HARD** |
| 전체 솔루션 빌드 결과 기록 | **HARD** |
| Git commit + push + PR | **HARD** |
| Status 섹션 정확한 결과 기록 | **HARD** |
| 허위 보고 금지 | **HARD** |

---

## 7. 리스크 관리

| 리스크 | 영향 Sprint | 확률 | 영향도 | 대응 |
|--------|-----------|------|--------|------|
| S03 빌드 에러 장기화 | S03--S04 | 낮음 | M1 지연 | P0 우선 해소, 팀 간 협조 |
| Generator RS-232 HW 미확보 | S08--S09 | 높음 | M2 지연 | 시뮬레이터 우선, HW 확보 시 교체 |
| FPD SDK 벤더 지연 | S07--S08 | 중간 | M3 지연 | 어댑터 패턴으로 SDK 없이 진행 |
| 외부 침투 테스트 일정 | S20 | 중간 | M5 지연 | 조기 예약 (S10부터 협의) |
| 사용성 테스트 참가자 | S21 | 낮음 | M5 지연 | 병원 협력 조기 확보 |
| DOC-042 CMP 미완성 | S04--S05 | 높음 | M1 지연 | RA 우선순위 상향 |

---

## 8. Sprint-마일스톤 요약 타임라인

```
S01 S02 S03 S04 S05 S06 S07 S08 S09 S10 S11 S12 S13 S14 S15 S16 S17 S18 S19 S20 S21 S22 S23 S24
 |---done---|--NOW--|                                                                            |
            | S03R3 | S04  |  S05  |       |       |       |       |       |       |       |    |
                     |      |       |       |       |       |       |       |       |       |    |
                      M1+I1  ------>  M2+I2  ------>  M3+I3  --->  M4+I4  M5     -------> M6
                      5.0MM          12.0MM          16.0MM       19.0MM  21.0MM          24.0MM
```

---

*이 문서는 WBS-001 v3.1의 Sprint 체계를 기반으로 6팀 Worktree 개발에 맞는 구현계획을 정의합니다.*
*v2.0에서 교차검증 결과(빌드 에러, 커버리지 갭, DISPATCH 정합성)를 전면 반영하였습니다.*
*Sprint 진행에 따라 실제 소진 MM과 작업 완료율을 기준으로 갱신합니다.*
