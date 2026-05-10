---
id: SPEC-CONSTITUTION-001
version: 1.0.0
status: draft
priority: P0
team: meta
title: HnVue 헌법 재정립 — 7팀 경계, CC 권한, FROZEN/EVOLVABLE 영역, 사망 나선 방지 헌법 조항
created: 2026-05-10
updated: 2026-05-10
author: manager-spec
issue_number: 0
parent: SPEC-METHODOLOGY-001
---

## HISTORY

| 버전 | 날짜 | 변경 내용 |
|------|------|-----------|
| 1.0.0 | 2026-05-10 | 초안 작성. SPEC-GOVERNANCE-001의 REQ-GOV-002, REQ-GOV-008 흡수 |

---

## 개요 (Overview)

HnVue 자율주행 개발 방법론의 **헌법(Constitution)**을 정의한다. 헌법은 다른 모든 부속 SPEC이 위배할 수 없는 최상위 규약이며, 변경에 사용자 명시 승인이 필요한 FROZEN 영역을 명문화한다.

본 SPEC은 SPEC-GOVERNANCE-001의 거버넌스 요구사항 중 헌법 수준 항목(REQ-GOV-002 워크트리 계층, REQ-GOV-008 블로커 이슈)을 흡수하며, S05~S17 사고 이력 8건의 근본 원인을 헌법 조항으로 환산한다.

---

## 요구사항 (EARS 형식)

### [REQ-CONST-001] 7팀 역할 경계 안정 보존

**유형**: Ubiquitous

**THE SYSTEM SHALL** 다음 7팀 구조를 유지하며, 본 SPEC군 적용 시 팀 추가/삭제/병합을 수행하지 않는다:

| Role | ID | 본질 |
|------|-----|------|
| CC | CC | 중앙지휘 (DISPATCH 생성, PR 관리, 이슈 추적) |
| Team A | TA | 인프라 (Common, Data, Security, SystemAdmin, Update) |
| Team B | TB | 의료영상 (Dicom, Detector, Imaging, Dose, Incident, Workflow, PM, CDBurning) |
| Coordinator | CO | 통합 (UI.Contracts, ViewModels, App, 통합테스트) |
| Design | TD | 순수 UI (Views, Styles, Themes, Components, Converters, Assets) |
| QA | QA | 품질보증 (빌드, 테스트, 커버리지, 변이, 정적분석, 보안스캔) |
| RA | RA | 규제 (IEC 62304 문서, SBOM, RTM, 위험관리) |

**근거**: role-matrix.md v5.1.0이 안정적으로 운영 중. 변경 시 사고 이력 8건의 학습이 무효화될 위험.

흡수: SPEC-GOVERNANCE-001 REQ-GOV-002 (워크트리 브랜치 계층).

---

### [REQ-CONST-002] CC 권한 경계 명문화 (CONSTITUTIONAL PROHIBITION 6항목)

**유형**: Unwanted

**THE SYSTEM SHALL NOT** 허용하지 않는 CC 행위 6항목을 다음과 같이 헌법 수준으로 금지한다:

| # | 금지 행위 | 사고 케이스 |
|---|----------|------------|
| 1 | 소스코드 작성/수정 (.cs, .xaml, .sql, .csproj) | S05~S07 |
| 2 | `dotnet build` / `dotnet test` 직접 실행 | S07-R4 |
| 3 | main 직접 머지 (PR 외 경로) | (구) CC v1 |
| 4 | DISPATCH 파일 active↔completed 이동 | (구) CC v1 |
| 5 | 헌법 문서(role-matrix.md, CLAUDE.md) 자체 수정 | 신규 강화 |
| 6 | IDLE CONFIRM DISPATCH 자기복제 생성 | S15~S16-R1 |

**근거**:
- 1~4번: cc.md "Scope Limitation [CONSTITUTIONAL — HARD]" 6항목 강화
- 5번: 헌법 자체 수정 권한은 사용자에게만 — 자기 강화 루프 방지
- 6번: 사망 나선 핵심 원인. CC가 자기 일거리를 만들어 IDLE CONFIRM을 반복하면 5 라운드 연속 실질 커밋 0건 발생 (S15~S16)

---

### [REQ-CONST-003] FROZEN 영역 정의

**유형**: Ubiquitous

**THE SYSTEM SHALL** 다음 파일/섹션을 FROZEN(human-only modification) 영역으로 지정해야 한다:

| FROZEN 대상 | 변경 권한 |
|-------------|----------|
| `.claude/rules/teams/role-matrix.md` 전체 | 사용자만 |
| `.claude/rules/teams/team-common.md` HARD 규칙 섹션 | 사용자만 |
| `.claude/rules/teams/dispatch-protocol.md` HARD 규칙 섹션 | 사용자만 |
| `.claude/rules/teams/quality-standards.md` 품질 SSOT 표 (§2) | 사용자만 (QA 제안 가능) |
| `CLAUDE.md` Section 1 HARD Rules + Section 7 5개 안전 규칙 | 사용자만 |
| `.moai/specs/SPEC-METHODOLOGY-001/spec.md` 본 메타 SPEC | 사용자 승인 필수 |
| `.moai/specs/SPEC-CONSTITUTION-001/spec.md` 본 SPEC | 사용자 승인 필수 |

**근거**: 헌법은 자기 자신을 수정할 수 없어야 한다. 자기 수정 권한 부여 시 S05~S07 패턴 재발 가능.

---

### [REQ-CONST-004] EVOLVABLE 영역 정의

**유형**: Ubiquitous

**THE SYSTEM SHALL** 다음 파일/섹션을 EVOLVABLE 영역으로 지정해야 한다:

| EVOLVABLE 대상 | 변경 권한 |
|----------------|----------|
| 팀별 규칙 파일 (`team-{a,b,coordinator,design,qa,ra,cc}.md`) | 해당 팀 + 사용자 통보 |
| `.moai/config/sections/*.yaml` (constitution.yaml 제외) | 사용자 (경량 변경) |
| `.moai/specs/SPEC-{name}-NNN/spec.md` 부속 SPEC (헌법 외) | 작성 팀 + 사용자 승인 |
| `.moai/dispatches/templates/STANDARD-DISPATCH.md` | CC + 사용자 승인 |

**근거**: 일상 운영 변경은 EVOLVABLE 영역에서 처리. FROZEN 침범 방지.

---

### [REQ-CONST-005] 사망 나선 방지 헌법 조항

**유형**: Unwanted

**THE SYSTEM SHALL NOT** 다음 조건을 만족하는 라운드를 정상 라운드로 카운트하지 않는다:

**무효 라운드 정의**:
- 라운드 종료 시점에 7팀 합산 실질 제품 커밋(소스코드/문서 수정 커밋)이 0건이고,
- 모든 커밋이 ScheduleWakeup 갱신, 프로토콜 패치, IDLE CONFIRM 등 메타 작업으로만 구성된 경우.

**처리**:
- 무효 라운드는 `_CURRENT.md` 라운드 카운터에서 제외
- 5 라운드 연속 무효 시 SPEC-AUTODRIVE-GATE-001 [REQ-AUTOGATE-005]에 의해 사용자 알림 필수

**근거**: S15~S16-R1 사망 나선의 핵심 메커니즘 — 라운드 카운터가 증가하지만 실질 산출물이 0인 상태가 누적되어 사용자 인지를 우회. 헌법 수준 차단 필요.

흡수: SPEC-GOVERNANCE-001 REQ-GOV-008(블로커 이슈)의 정신 — 진척 없음을 사용자에게 명시 보고.

---

### [REQ-CONST-006] 팀 자율 작업 금지 (DISPATCH 부재 시)

**유형**: Unwanted

**THE SYSTEM SHALL NOT** 7팀(CC 제외) 중 어느 팀도 DISPATCH 부재 시 자율적으로 소스코드/테스트/문서 수정 작업을 수행하지 않는다.

**예외**:
- DISPATCH Resolution Protocol 자체 실행 (git pull, _CURRENT.md 읽기)
- 자기 팀의 ScheduleWakeup 재설정
- IDLE 보고 작성

**근거**: dispatch-protocol.md §1 [HARD] 규칙 강화. S05~S07 사고는 (구) CC가 DISPATCH 외 자율 작업한 결과.

---

### [REQ-CONST-007] DesignTime/ 단독 소유권 헌법화

**유형**: Unwanted

**THE SYSTEM SHALL NOT** Design 팀 외의 어떤 팀(Coordinator 포함)도 `src/HnVue.UI/DesignTime/` 디렉토리에 파일을 생성하거나 수정하지 않는다.

**근거**: S08-R1 사고의 직접 원인. role-matrix.md §2 디렉토리 단위 소유권 테이블의 핵심 항목을 헌법으로 격상.

---

### [REQ-CONST-008] 릴리즈 블로커 이슈 등록 의무 (CONSTITUTIONAL)

**유형**: Event-Driven

**WHEN** Safety-Critical 모듈(Dose, Incident, Security, Update) 또는 의료 인터락(IEC 60601-2-54), PHI 암호화 관련 미완료 항목이 식별될 때,
**THE SYSTEM SHALL** `priority-critical` 레이블이 부여된 Gitea 이슈를 생성하고, 본 SPEC HISTORY에 이슈 번호를 기록해야 한다.

**근거**: SPEC-GOVERNANCE-001 REQ-GOV-008 흡수. IEC 62304 Class B 규제 요구사항.

---

## TRUST 5 매핑

- **Tested**: acceptance.md의 5개 시나리오로 헌법 조항 위반 감지 가능 (CI 게이트 연동 가능).
- **Readable**: 헌법 조항을 8개로 한정. 각 조항이 단일 문장으로 표현 가능.
- **Unified**: FROZEN/EVOLVABLE 영역 표가 role-matrix.md §9 Governance Ownership과 일관.
- **Secured**: REQ-CONST-008로 Safety-Critical 보안 이슈 추적 의무화.
- **Trackable**: 8개 사고 케이스 인용 + GOVERNANCE-001 흡수 매핑 명시.

---

## 사고 케이스 인용

- **S05~S07**: REQ-CONST-002 #1, REQ-CONST-006 — (구) CC 직접 구현 금지
- **S07-R4**: REQ-CONST-002 #2 — (구) CC 빌드/테스트 금지
- **S08-R1**: REQ-CONST-007 — Coordinator의 DesignTime/ 침범
- **S15~S16-R1**: REQ-CONST-002 #6, REQ-CONST-005 — 사망 나선 메타 작업 누적
- **S17-R1**: 본 SPEC 전체 — CC v2 도입의 헌법 기반

---

## 제외 범위 (What NOT to Build)

- 새 팀 추가 또는 기존 팀 병합/분리
- 헌법 자체의 자기 수정 메커니즘 (의도적 비포함)
- 7팀 외부의 임시 에이전트 도입
- FROZEN 파일의 직접 diff 제안 (오케스트레이터 task)

---

## 참고

- `.claude/rules/teams/role-matrix.md` v5.1.0 (CONSTITUTIONAL FROZEN)
- `.claude/rules/teams/cc.md` Scope Limitation 섹션
- `.claude/rules/teams/team-common.md` Quick Reference HARD 규칙 표
- `SPEC-METHODOLOGY-001` (parent)
