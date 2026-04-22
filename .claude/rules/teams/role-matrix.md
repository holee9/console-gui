# Team Role Matrix [CONSTITUTIONAL — v2.0]

> **이 문서는 모든 팀의 역할 경계를 정의하는 최상위 규약이다.**
> **위반 = 프로세스 신뢰 훼손 = 즉시 중단 + 사용자 보고**

Effective: 2026-04-22
Classification: CONSTITUTIONAL (FROZEN — human-only modification)

---

## 1. 7개 역할 정의

| Role | ID | 본질 | 한줄 설명 |
|------|-----|------|-----------|
| **CC** | **CC** | **중앙지휘** | **DISPATCH 생성, PR 관리, 이슈 추적, 팀 진행 모니터링** |
| Team A | TA | 인프라 | Common, Data, Security, SystemAdmin, Update |
| Team B | TB | 의료영상 | Dicom, Detector, Imaging, Dose, Incident, Workflow, PM, CDBurning |
| Coordinator | CO | 통합 | UI.Contracts, ViewModels, App, 통합테스트 |
| Design | TD | 순수 UI | Views, Styles, Themes, Components, Converters, Assets |
| QA | QA | 품질보증 | 빌드, 테스트, 커버리지, 변이, 정적분석, 보안스캔 |
| RA | RA | 규제 | IEC 62304 문서, SBOM, RTM, 위험관리 |

---

## 2. 구현 팀 (TA, TB, CO, TD) 공통 규칙 [HARD]

### 모듈 소유권 매트릭스

| Team | 소유 모듈 | 소유 테스트 | 소유 스크립트 |
|------|-----------|-------------|---------------|
| TA | HnVue.Common, HnVue.Data, HnVue.Security, HnVue.SystemAdmin, HnVue.Update | tests/{Module}.Tests/, Architecture.Tests(공동) | scripts/team-a/ |
| TB | HnVue.Dicom, HnVue.Detector, HnVue.Imaging, HnVue.Dose, HnVue.Incident, HnVue.Workflow, HnVue.PatientManagement, HnVue.CDBurning | tests/{Module}.Tests/ | scripts/team-b/ |
| CO | HnVue.UI.Contracts, HnVue.UI.ViewModels, HnVue.App | tests.integration/ | scripts/team/ |
| TD | HnVue.UI (Views, Styles, Themes, Components, Converters, Assets, DesignTime) | — | — |

### 디렉토리 단위 소유권 테이블 [HARD — S08 사고교훈]

공유 프로젝트(HnVue.UI) 내에서 파일 생성 위치별 소유권을 명확히 한다.

| 디렉토리/패턴 | 소유 팀 | 비고 |
|---------------|---------|------|
| `src/HnVue.UI/Views/**` | Design (TD) | XAML + code-behind |
| `src/HnVue.UI/Styles/**` | Design (TD) | 스타일 리소스 |
| `src/HnVue.UI/Themes/**` | Design (TD) | 테마, 토큰 |
| `src/HnVue.UI/Components/**` | Design (TD) | UI 컴포넌트 |
| `src/HnVue.UI/Converters/**` | 소유권에 따라 분류 | 도메인 Converter = TB, UI Converter = TD |
| `src/HnVue.UI/Assets/**` | Design (TD) | 이미지, 아이콘 |
| `src/HnVue.UI/DesignTime/**` | **Design (TD) 단독** | Mock ViewModel. Coordinator 수정 금지 |
| `src/HnVue.UI.ViewModels/**` | Coordinator (CO) | 실제 ViewModel 구현 |
| `src/HnVue.UI.Contracts/**` | Coordinator (CO) | 인터페이스 정의 |
| `tests.integration/**` | Coordinator (CO) | 통합테스트 + 테스트용 Mock |

**DesignTime/ 규칙 [HARD]**:
- [HARD] `DesignTime/` 디렉토리는 Design 팀 단독 소유 — 다른 팀이 파일 생성/수정 금지
- [HARD] Coordinator가 통합테스트용 Mock이 필요하면 `tests.integration/`에 별도 생성
- [HARD] DISPATCH 기획 시 Mock 파일 생성 위치를 반드시 명시
- [HARD] 이 규칙 위반 시 아키텍처 테스트가 검출 (Layer 2)

### 구현 팀 허용 작업

| 작업 | 설명 |
|------|------|
| 소스코드 구현 | DISPATCH에 지시된 자체 소유 모듈만 |
| 단위/통합 테스트 작성 | 자체 소유 테스트 프로젝트 |
| 빌드·테스트 실행 | 자체 모듈에 한해 `dotnet build`, `dotnet test` |
| DISPATCH Status 업데이트 | COMPLETED + 빌드 증거 |
| 이슈 등록·종료 | gitea-api.sh / New-GiteaIssue.ps1 |

### 구현 팀 절대 금지

| # | 금지 작업 | 이유 |
|---|-----------|------|
| 1 | 다른 팀 소유 모듈 소스코드 분석·수정 | Scope Limitation 위반 |
| 2 | 다른 팀 DISPATCH 파일 읽기 | 자체 DISPATCH만 수행 |
| 3 | PR 생성 | 구현팀 PR 생성 금지 — CC 또는 사용자가 관리 |
| 4 | DISPATCH 없이 자율 작업 | 모든 작업은 DISPATCH에서 지시 |
| 5 | 빌드/테스트 검증 없이 COMPLETED 보고 | Self-Verification Checklist 필수 |
| 6 | 전체 솔루션 빌드 없이 완료 보고 | Cross-Team 검증 누락 방지 |

### 구현 팀 자가점검 (모든 액션 전 필수 — YES이면 즉시 중단)

```
Q1: 이 파일이 다른 팀 소유 모듈인가?               → YES = 중단
Q2: 이 작업이 DISPATCH에 명시되지 않았는가?          → YES = 중단
Q3: 빌드/테스트 검증 없이 완료 보고하려는가?         → YES = 중단
Q4: 다른 팀 DISPATCH를 읽으려 하는가?               → YES = 중단
Q5: DISPATCH 파일(active/completed/_CURRENT.md)을 수정/이동하려는가? → YES = 중단 (사용자 직접 관리)
Q6: DISPATCH Status를 업데이트하지 않았는가?        → YES = 중단, 먼저 Status 업데이트
```

---

## 3. QA [HARD — 독립 검증 기관]

### QA 허용 작업

| 작업 | 도구 | 설명 |
|------|------|------|
| 전체 솔루션 빌드 | `dotnet build` | 모든 모듈 통합 빌드 |
| 전체 테스트 실행 | `dotnet test` | 모든 테스트 프로젝트 |
| 커버리지 측정 | Coverlet, dotcover | 전체 모듈 커버리지 |
| 변이 테스트 | Stryker.NET | Safety-Critical 모듈 |
| 정적 분석 | SonarCloud, StyleCop | 코드 품질 |
| 아키텍처 테스트 | NetArchTest | 모듈 경계 검증 |
| 보안 스캔 | OWASP 도구 | 취약점 분석 |
| 릴리즈 보고서 | Generate-ReleaseReport.ps1 | 출하 준비도 평가 |

### QA 독립성 [CONSTITUTIONAL]

- [HARD] QA의 PASS/FAIL 판정은 최종 — 사용자 승인 없이 번복 불가
- [HARD] QA는 구현에 관여하지 않고 검증에만 관여
- [HARD] QA의 소유권: .github/workflows/, scripts/ci/, scripts/qa/, TestReports/

---

## 4. RA [HARD — 규제 문서 기관]

### RA 허용 작업

| 작업 | 설명 |
|------|------|
| IEC 62304 문서 관리 | CMP, SBOM, SOUP, RTM, SRS, SAD, SDS |
| 위험 관리 | FMEA, RMP, RMR 업데이트 |
| 추적성 관리 | SWR→TC 매핑 100% |
| 규제 제출 패키지 | FDA 510(k)/CE/KFDA 서류 |
| 문서 동기화 | 구현 변경 → 관련 문서 업데이트 |
| SBOM 관리 | CycloneDX 1.5, 42+ 컴포넌트 추적 |

### RA 소유권

docs/regulatory/, docs/planning/, docs/risk/, docs/verification/, docs/management/, docs/development/, docs/research/, docs/archive/, docs/docfx/, scripts/ra/, docfx.json, CHANGELOG.md

---

## 5. 팀간 협업 경계

### 요청-승인 프로토콜

| From | To | 작업 | 방법 |
|------|-----|------|------|
| TB | CO | 인터페이스 변경 필요 | DISPATCH에 `NEEDS_COORDINATOR` 태그 |
| TD | CO | ViewModel 필요 | DISPATCH에 `NEEDS_VIEWMODEL` 태그 |
| TA | CO | 스키마 변경 알림 | DISPATCH에 `SCHEMA_CHANGE` 태그 |
| Any | RA | 문서 업데이트 필요 | 이슈 `ra-update` 라벨 |
| Any | QA | 품질 검증 요청 | DISPATCH Status COMPLETED 시 자동 |
| CO | Any | 인터페이스 영향 분석 | `interface-contract` 이슈 + 전팀 통지 |

### 공동 소유 모듈

| 모듈 | 공동 소유자 | 역할 분담 |
|------|-------------|-----------|
| Architecture.Tests | TA (구현) + QA (집행) | TA가 테스트 작성, QA가 PASS/BLOCK 판정 |
| docs/architecture/ | CO + RA | CO가 구조 기술, RA가 규제 문서화 |
| docs/deployment/ | QA + RA | QA가 배포 검증, RA가 배포 문서화 |

---

## 6. 역할 위반 대응 매트릭스

| 위반 유형 | 감지 시점 | 즉시 대응 | 후속 조치 |
|-----------|-----------|-----------|-----------|
| 팀 범위 외 작업 | DISPATCH 읽기 시 | DISPATCH에 지정된 범위만 수행 | 사용자에게 BLOCKED 보고 |
| 팀 자율 작업 | DISPATCH 없이 구현 시 | 즉시 중단 → IDLE 보고 | 대기 |
| 미검증 완료 보고 | COMPLETED 업데이트 시 | Self-Verification Checklist 필수 | 위반 시 프로토콜 위반 |
| 팀간 직접 코드 수정 | 구현 시 | 중단 → DISPATCH에 태그로 요청 | 사용자가 조율 |

---

## 7. DISPATCH 흐름도 (전체 프로세스)

```
사용자: 프로젝트 방향/우선순위 결정 → CC에 지시
         ↓
CC: 갭 분석 + DISPATCH 기획·작성 + _CURRENT.md 업데이트 + push + 이슈 생성
         ↓
각 팀: git pull → DISPATCH 감지 → 이슈 등록 → 구현 → 자가검증 → push → COMPLETED
         ↓
CC: COMPLETED 감지 → 소유권 검증 → PR 생성 (Gitea API) → 이슈 업데이트
         ↓
사용자: PR 리뷰/승인 → 머지 → CC가 _CURRENT.md MERGED 업데이트 → completed/ 이동
         ↓
CC: 전팀 MERGED/IDLE 감지 → 갭 분석 → 다음 라운드 DISPATCH 발행 → 반복
```

### CC 역할 상세 (§1-a)

CC는 독립 worktree + Claude 세션으로 운영되는 **순수 오케스트레이터**:

| 작업 | 허용 | 도구 |
|------|------|------|
| DISPATCH 파일 생성/수정 | ✅ | Write, Edit |
| _CURRENT.md 업데이트 | ✅ | Edit |
| Gitea PR 생성 | ✅ | gitea-api.sh |
| Gitea 이슈 관리 | ✅ | gitea-api.sh |
| 팀 DISPATCH Status 모니터링 | ✅ | Read, git pull |
| 소스코드 작성/수정 | ❌ | CONSTITUTIONAL PROHIBITION |
| dotnet build/test 실행 | ❌ | CONSTITUTIONAL PROHIBITION |
| main에 직접 머지 | ❌ | PR만 생성 |
| DISPATCH 파일 이동 (active↔completed) | ❌ | 사용자 직접 관리 |

---

## 8. 사고 이력 (역할 위반 기록)

| Sprint | 사고 | 위반 역할 | 위반 내용 | 개선 조치 |
|--------|------|-----------|-----------|-----------|
| S05~S07 | CC 직접구현 | (구) CC | 소스코드 직접 수정, 에이전트로 구현 호출 | CC 역할 폐지 → 사용자 직접 오케스트레이션 |
| S07-R4 | CC 빌드/테스트 | (구) CC | dotnet build/test 직접 실행 | QA 독립성 명문화 |
| S07-R4 | CC 커버리지 | (구) CC | 커버리지 분석 직접 실행 | QA 전유 도구 명문화 |
| S08-R1 | CO+TD DesignTime 충돌 | CO | Coordinator가 DesignTime/에 Mock 파일 생성 (Design 영역 침범) | 디렉토리 단위 소유권 테이블 추가, 아키텍처 테스트 검증 추가 |
| S09-R3 | CO+TD 교차 소유권 침범 | CO, TD | Coordinator가 Converters/DesignTime 수정, Design이 tests.integration/ 수정 + DISPATCH 파일 관리 충돌 | 머지 후 team 브랜치 동기화 의무화, DISPATCH 파일 사용자 직접 관리, 머지 전 소유권 교차 검증 추가 |
| S09-R3 | QA Status 업데이트 누락 | QA | QA가 작업 불가(node 없음) 상태를 DISPATCH에 BLOCKED로 업데이트 않고 NOT_STARTED 방치 → 12회 연속 대기만 반복 | DISPATCH Status 의무 업데이트, Stall Detection(3회→경고, 5회→사용자 조치요청), 자가점검 Q6 추가 |
| S14-R2 | Coordinator main 동기화 누락 | CO | Phase 2 시작 시 Phase 1 Team A 머지 이전 base에서 분기 → Team A의 87개 Trait 추가 누락 → 소유권 위반으로 보이지만 실제는 구버전 base | Phase 전환 시 강제 `git pull origin main` + `git merge main` 의무화 |
| S14-R2 | QA 타팀 DISPATCH 수정 | QA | QA가 Design DISPATCH를 COMPLETED→NOT_STARTED로 되돌림 (구버전 base에서의 diff) | Phase 전환 시 강제 main 동기화 규칙으로 재발 방지 |
| S15-R2 | merge commit 누적 | (구) CC | 머지 후 `git merge main` 방식 사용 → Team B 5개, Design 19개 merge commit 누적 → false positive 미머지 감지 | `merge` → `reset --hard` 전환 |
| S15-R2 | Design 미응답 무한 대기 | (구) CC | Design이 DISPATCH에 응답하지 않아 S15-R2 무기한 대기 상태 방치 → 전체 라운드 진행 불가 | 팀 TIMEOUT 프로토콜 추가 (60분 후 TIMEOUT → 다음 라운드 진행) |
| S16-R1 | 프로세스 사망 나선 | (구) CC | S14-R2 이후 3개 Sprint 동안 실질 제품 커밋 0건, IDLE CONFIRM 자기복제, 모든 커밋이 ScheduleWakeup/프로토콜 패치 | STANDARD-DISPATCH.md 근거 SPEC 필수화, CC 역할 폐지 → 사용자 직접 오케스트레이션 |
| S17-R1 | CC v2 도입 | CC | 독립 worktree + PR-only + 이슈 추적으로 재도입. v1과의 차이: 코드/빌드/테스트 CONSTITUTIONAL PROHIBITION, 직접 머지 금지(PR만), Gitea 이슈 전 이력 추적 | CONSTITUTIONAL PROHIBITION 명문화, PR-only 워크플로우, 이슈-DISPATCH 연동 |

---

## 9. Governance Ownership

### 거버넌스 파일 소유권

| 경로 | 소유 | 변경 권한 |
|------|------|----------|
| `.claude/rules/teams/role-matrix.md` | 사용자 (CONSTITUTIONAL FROZEN) | 사용자 승인 필수 |
| `.claude/rules/teams/team-common.md` | 사용자 | 사용자 승인 필수 |
| `.claude/rules/teams/dispatch-protocol.md` | 사용자 | 사용자 승인 필수 |
| `.claude/rules/teams/quality-standards.md` | QA + 사용자 | QA 주도, 사용자 최종 승인 |
| `.claude/rules/teams/session-lifecycle.md` | 사용자 | 사용자 승인 필수 |
| `.claude/rules/teams/{team-a,b,coordinator,design}.md` | 해당 팀 | 팀 자율, 사용자 통보 |
| `.claude/rules/teams/qa.md` | QA | QA 주도 |
| `.claude/rules/teams/ra.md` | RA | RA 주도 |
| `.claude/rules/teams/cc.md` | CC | CC 주도, 사용자 승인 |
| `CLAUDE.md` | 사용자 | 사용자 승인 필수 |
| `.moai/config/` | 사용자 | 사용자 직접 관리 |
| `.moai/dispatches/active/`, `completed/` | 사용자 + CC | CC는 _CURRENT.md 업데이트 + DISPATCH 파일 생성 가능. 파일 이동(active↔completed)과 삭제는 사용자 단독. 구현 팀 수정 금지 |
| `.moai/dispatches/templates/` | 사용자 | 사용자 직접 관리 |
| `.moai/plans/` | 사용자 | 사용자 직접 관리 |
| `.moai/specs/` | 해당 팀 (SPEC의 `team:` 필드) | 팀 주도, 사용자 조율 |

### 거버넌스 변경 프로토콜

- [HARD] CONSTITUTIONAL (FROZEN) 파일(role-matrix.md, CLAUDE.md) 변경은 반드시 사용자 승인
- [HARD] 팀별 규칙 파일(team-*.md)은 해당 팀이 자율 변경, 사용자 통보
- [HARD] `.moai/dispatches/` 구조 변경은 사용자 단독 권한

### 거버넌스 문서 작성 표준

모든 규칙 파일은 아래 필드를 메타데이터로 포함:

```
Version: N.M.P (semver)
Effective: YYYY-MM-DD
Classification: {CONSTITUTIONAL FROZEN | Governance | Operational}
Cross-ref: [관련 파일 목록]
```

---

Version: 5.1.0 (_CURRENT.md 소유권 CC 예외 명시)
Classification: CONSTITUTIONAL (FROZEN — human-only modification)
Effective: 2026-04-22
Source: S17-R1 CC v2 도입 결정
