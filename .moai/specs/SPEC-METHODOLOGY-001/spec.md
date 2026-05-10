---
id: SPEC-METHODOLOGY-001
version: 1.0.0
status: draft
priority: P0
team: meta
title: HnVue 자율주행 개발 방법론 메타 SPEC — 5축 부속 SPEC 통합 헌장
created: 2026-05-10
updated: 2026-05-10
author: manager-spec
issue_number: 0
supersedes: SPEC-GOVERNANCE-001
---

## HISTORY

| 버전 | 날짜 | 변경 내용 |
|------|------|-----------|
| 1.0.0 | 2026-05-10 | 초안 작성. SPEC-GOVERNANCE-001(draft) 흡수 통합. 5축 부속 SPEC 정의 |

---

## 개요 (Overview)

HnVue 의료 영상 WPF 프로젝트(IEC 62304 Class B)의 자율주행 개발 방법론을 재정립하는 **최상위 메타 SPEC**이다.

S05~S17 사이에 발생한 8건의 구조적 사고(role-matrix.md §8 참조)와 S15~S16의 "사망 나선"(3 Sprint 동안 실질 커밋 0건) 사태를 근본 원인까지 파고들어, 사고 재발 방지 + 자율주행 신뢰성 확보를 동시에 달성하기 위한 **헌장(charter) 수준의 통합 SPEC**이다.

이 SPEC은 직접 요구사항을 정의하지 않고, **5개 부속 SPEC**을 RTM 상위 키로 가지며 각 부속 SPEC이 단일 축(axis)에 책임을 진다.

---

## 5축 부속 SPEC 정의

| 부속 SPEC ID | 축 (Axis) | 책임 영역 |
|-------------|-----------|-----------|
| SPEC-CONSTITUTION-001 | 헌법 | 7팀 역할 경계, CC 권한, FROZEN/EVOLVABLE 영역, 사망 나선 방지 헌법 조항 |
| SPEC-DISPATCH-V2-001 | 프로토콜 | DISPATCH 수명주기 v2, Phase Dependency 강제 게이트, Stall/TIMEOUT 해석 |
| SPEC-AUTODRIVE-GATE-001 | 게이트 | Evidence-Based Completion 강화, 사망 나선 감지, Self-Verification 7항목 |
| SPEC-SPEC-TRIAGE-001 | 재고 | 기존 7개 SPEC 3분류(ACTIVE/COMPLETED/DEPRECATED), RTM 갱신 절차 |
| SPEC-ROADMAP-001 | 일정 | S18+ 라운드 적용 계획, 우선순위 P0/P1/P2 라벨링 |

---

## 비전 (Vision)

> **"속도가 아니라 완결성을 추구한다.**
>
> **사망 나선을 두 번 다시 일으키지 않는다.**
>
> **CC와 7팀이 자기 역할에서 벗어나지 않는다."**

핵심 원칙(Core Tenets):

1. **Evidence-Based Completion** — 모든 COMPLETED 보고는 빌드/테스트 증거를 동반한다.
2. **Phase Dependency 명시화** — Phase 1 미완료 시 후속 Phase는 게이트 통과 불가.
3. **CC 권한 경계 재정립** — CC는 코드/빌드/테스트/직접 머지 절대 금지(CONSTITUTIONAL PROHIBITION).
4. **Stall/TIMEOUT 해석 상세화** — 사람의 판단을 대체하지 말고, 사람의 판단을 유도한다.
5. **사망 나선 감지** — 5 라운드 연속 실질 커밋 0건 시 자동 사용자 알림.

---

## 목표 (Goals)

- **G-1**: 5개 부속 SPEC의 일관성 있는 RTM 상위 키 제공
- **G-2**: SPEC-GOVERNANCE-001(draft)의 흡수 통합 — 거버넌스 8개 요구사항을 5개 부속 SPEC에 재배치
- **G-3**: S18-R1부터 신 방법론 적용 가능한 마이그레이션 경로 제공
- **G-4**: 기존 7개 SPEC의 3분류 매트릭스 확정으로 SPEC 재고 정리 (.moai/specs/ 위생 회복)
- **G-5**: 사고 재발 방지 — 8건의 사고 케이스를 부속 SPEC 요구사항에 1:1로 연결

## 비목표 (Non-Goals)

- **NG-1**: 소스코드 변경 — 이번 SPEC군은 거버넌스/방법론 한정. 모듈 구현은 다루지 않는다.
- **NG-2**: 7팀 워크트리 구조 변경 — 현행 7팀(CC+TA+TB+CO+TD+QA+RA) 구조 유지.
- **NG-3**: FROZEN 파일의 직접 diff 제안 — 본 SPEC군은 변경 명세만 정의. 실제 적용은 오케스트레이터의 후속 작업.
- **NG-4**: Safety-Critical 모듈 잔여 커버리지 보강 — SPEC-TEAMB-COV-001 등 별도 ACTIVE SPEC이 처리.
- **NG-5**: SPEC-GOVERNANCE-001의 직접 SUPERSEDED 마킹 편집 — 본 SPEC의 `supersedes:` 프론트매터로만 선언. 실제 GOVERNANCE-001 파일 편집은 오케스트레이터 task.

---

## 요구사항 (EARS 형식)

### [REQ-METHODOLOGY-001] 5개 부속 SPEC RTM 상위 키 보유

**유형**: Ubiquitous

**THE SYSTEM SHALL** SPEC-METHODOLOGY-001을 RTM 상위 키로 하여 다음 5개 부속 SPEC을 자식으로 가져야 한다:
- SPEC-CONSTITUTION-001
- SPEC-DISPATCH-V2-001
- SPEC-AUTODRIVE-GATE-001
- SPEC-SPEC-TRIAGE-001
- SPEC-ROADMAP-001

**근거**: 5축 분리로 단일 책임 원칙(SRP) 적용. 각 부속 SPEC이 독립적으로 진화 가능.

---

### [REQ-METHODOLOGY-002] SPEC-GOVERNANCE-001 SUPERSEDED 처리

**유형**: State-Driven

**WHILE** SPEC-METHODOLOGY-001의 status가 `approved` 또는 `implemented`인 동안,
**THE SYSTEM SHALL** SPEC-GOVERNANCE-001을 SUPERSEDED 상태로 처리하고, GOVERNANCE-001의 8개 요구사항(REQ-GOV-001~008)을 다음 매핑에 따라 5개 부속 SPEC에 흡수해야 한다:

| GOVERNANCE-001 요구사항 | 흡수 대상 부속 SPEC | 비고 |
|------------------------|---------------------|------|
| REQ-GOV-001 (커밋 의무) | SPEC-AUTODRIVE-GATE-001 | Evidence-Based Completion 강화 |
| REQ-GOV-002 (워크트리 계층) | SPEC-CONSTITUTION-001 | 7팀 구조 명문화 (현행 유지) |
| REQ-GOV-003 (Coordinator 통합) | SPEC-DISPATCH-V2-001 | Phase Dependency 게이트 |
| REQ-GOV-004 (Issue-First) | SPEC-DISPATCH-V2-001 | DISPATCH 수명주기 |
| REQ-GOV-005 (한글 인코딩) | SPEC-DISPATCH-V2-001 | gitea-api.sh / PowerShell 권장 |
| REQ-GOV-006 (QA 보고서) | SPEC-AUTODRIVE-GATE-001 | Evidence 항목 |
| REQ-GOV-007 (수명주기 이행) | SPEC-DISPATCH-V2-001 | DISPATCH active/completed 이동 |
| REQ-GOV-008 (블로커 이슈) | SPEC-CONSTITUTION-001 | 헌법 조항 |

**근거**: GOVERNANCE-001은 단일 SPEC에 8개 이질적 요구사항을 묶어 응집도가 낮음. 5축 분리로 응집도 향상.

---

### [REQ-METHODOLOGY-003] 부속 SPEC DEPRECATED 시 RTM 자동 갱신

**유형**: Event-Driven

**WHEN** 부속 SPEC 5개 중 하나가 DEPRECATED 상태로 전환될 때,
**THE SYSTEM SHALL** SPEC-METHODOLOGY-001의 RTM 매핑 표를 갱신하고, 대체 SPEC ID를 `supersedes:` 또는 본 SPEC HISTORY에 명시해야 한다.

**근거**: 부속 SPEC의 진화/폐기 시 메타 SPEC이 stale 상태를 방지.

---

### [REQ-METHODOLOGY-004] 5축 부속 SPEC 간 순환 참조 금지

**유형**: Unwanted

**THE SYSTEM SHALL NOT** 5개 부속 SPEC 간 순환 참조 관계(circular dependency)를 형성하지 않는다.

허용되는 참조 그래프(단방향):

```
METHODOLOGY-001 (meta)
    ├─→ CONSTITUTION-001  (헌법 — 다른 부속 SPEC이 참조)
    ├─→ DISPATCH-V2-001   (CONSTITUTION-001 참조 가능)
    ├─→ AUTODRIVE-GATE-001 (CONSTITUTION-001, DISPATCH-V2-001 참조 가능)
    ├─→ SPEC-TRIAGE-001   (CONSTITUTION-001 참조 가능)
    └─→ ROADMAP-001       (모든 부속 SPEC 참조 가능 — 일정 통합)
```

**근거**: 헌법 → 프로토콜 → 게이트 → 재고 → 일정 순서의 단방향 의존이 가독성과 진화 안정성을 보장.

---

### [REQ-METHODOLOGY-005] S18-R1 적용 시점에 마이그레이션 보고서 생성

**유형**: Event-Driven

**WHEN** SPEC-METHODOLOGY-001의 status가 `approved`로 전환될 때,
**THE SYSTEM SHALL** `.moai/reports/methodology-migration-S18-R1.md` 파일을 생성하여 다음을 기록해야 한다:
- GOVERNANCE-001 → 5축 부속 SPEC 흡수 매핑 결과
- 7개 기존 SPEC의 ACTIVE/COMPLETED/DEPRECATED 분류 결과(SPEC-TRIAGE-001 출력)
- 마이그레이션 액션 리스트 (사용자 수행 필요 항목)

**근거**: 신 방법론 발효 시점의 명시적 기록은 사후 감사 및 회귀 방지에 필수.

---

### [REQ-METHODOLOGY-006] 부속 SPEC의 status 일관성

**유형**: Ubiquitous

**THE SYSTEM SHALL** 5개 부속 SPEC의 status 전이를 다음 규칙에 따라 일관되게 유지해야 한다:
- `draft` → `approved`: 사용자 승인 후
- `approved` → `implemented`: 부속 SPEC 자체 acceptance 통과 후
- `implemented` → `deprecated`: 후속 SPEC이 `supersedes` 명시 후

**근거**: status 일관성 결여는 SPEC-TRIAGE-001의 분류 알고리즘을 무력화시킴.

---

## 성공 지표 (Success Metrics)

| 지표 | 측정 방법 | 목표값 |
|------|----------|-------|
| 5축 부속 SPEC 작성 완료 | 6개 spec.md + acceptance.md 파일 존재 | 100% |
| RTM 매핑 정확성 | METHODOLOGY-001 RTM 표와 부속 SPEC `parent:` 일치 | 100% |
| GOVERNANCE-001 흡수 커버리지 | REQ-GOV-001~008 매핑 누락 0건 | 0건 |
| 사고 케이스 연결 | role-matrix.md §8 사고 8건이 부속 SPEC 요구사항에 인용 | 8건 100% |
| 비목표 위반 | 소스코드 변경 0건 | 0건 |

---

## RTM 매핑 표 (Master)

| Source REQ | Target SPEC / REQ | Phase |
|-----------|-------------------|-------|
| REQ-METHODOLOGY-001 | METHODOLOGY-001 본 SPEC | meta |
| REQ-METHODOLOGY-002 | GOVERNANCE-001 흡수 | migration |
| REQ-METHODOLOGY-003 | RTM 갱신 자동화 | governance |
| REQ-METHODOLOGY-004 | 부속 SPEC 의존 그래프 | structural |
| REQ-METHODOLOGY-005 | 마이그레이션 보고서 | rollout |
| REQ-METHODOLOGY-006 | status 일관성 | lifecycle |
| REQ-CONST-001~008 | SPEC-CONSTITUTION-001 | charter |
| REQ-DISPATCH-V2-001~012 | SPEC-DISPATCH-V2-001 | protocol |
| REQ-AUTOGATE-001~009 | SPEC-AUTODRIVE-GATE-001 | gate |
| REQ-TRIAGE-001~005 | SPEC-SPEC-TRIAGE-001 | inventory |
| REQ-ROADMAP-001~006 | SPEC-ROADMAP-001 | schedule |

---

## TRUST 5 매핑

- **Tested**: acceptance.md의 Given/When/Then 시나리오로 5축 부속 SPEC 일관성 검증.
- **Readable**: 5축 분리로 각 SPEC이 단일 책임. 본 메타 SPEC은 인덱스 역할만 수행.
- **Unified**: 모든 부속 SPEC이 동일한 frontmatter 스키마(`parent: SPEC-METHODOLOGY-001`) 사용.
- **Secured**: 본 SPEC군은 거버넌스 한정 — 보안 위협 수준은 변경 안내(README/CHANGELOG) 누락 정도.
- **Trackable**: HISTORY 표 + RTM 매핑 + role-matrix.md §8 사고 케이스 인용으로 추적 가능.

---

## 마이그레이션 계획 (Current → S18-R1)

| 단계 | 액션 | 책임 | 산출물 |
|------|------|------|--------|
| M1 | 5축 부속 SPEC 작성 | manager-spec | 5개 spec.md + acceptance.md |
| M2 | 사용자 승인 (AskUserQuestion) | 오케스트레이터 | status: draft → approved |
| M3 | GOVERNANCE-001 SUPERSEDED 마킹 | 오케스트레이터 | GOVERNANCE-001/spec.md 헤더 갱신 |
| M4 | FROZEN 파일 diff 후보 도출 | 오케스트레이터 | role-matrix.md/CLAUDE.md 변경 제안서 |
| M5 | 사용자 FROZEN 승인 | 사용자 | 헌법 변경 적용 |
| M6 | S18-R1 적용 시작 | CC + 7팀 | 첫 라운드 DISPATCH 발행 |

---

## 사고 케이스 인용 (Cross-References)

부속 SPEC 요구사항이 인용해야 할 8건:

| Sprint | 사고 | 흡수 대상 부속 SPEC |
|--------|------|---------------------|
| S05~S07 | (구) CC 직접 구현 | SPEC-CONSTITUTION-001 |
| S07-R4 | (구) CC 직접 빌드/테스트 | SPEC-CONSTITUTION-001 |
| S08-R1 | CO의 DesignTime/ 침범 | SPEC-CONSTITUTION-001 |
| S09-R3 | CO+TD 교차 침범 + QA Stall | SPEC-DISPATCH-V2-001 |
| S14-R2 | Phase 2 main 동기화 누락 | SPEC-DISPATCH-V2-001 |
| S15-R2 | Design 무한 대기 → TIMEOUT | SPEC-DISPATCH-V2-001 |
| S15~S16-R1 | 사망 나선(실질 커밋 0건) | SPEC-AUTODRIVE-GATE-001 |
| S17-R1 | CC v2 도입 | SPEC-CONSTITUTION-001 |

---

## 제외 범위 (What NOT to Build)

- 소스코드 변경 (NG-1)
- 7팀 워크트리 구조 변경 (NG-2)
- FROZEN 파일 직접 diff (NG-3)
- 잔여 커버리지 작업 (NG-4)
- GOVERNANCE-001 파일 편집 (NG-5)
- 새 도구/CLI/스크립트 작성

---

## 참고 (References)

- `.claude/rules/teams/role-matrix.md` §8 — 사고 이력 (CONSTITUTIONAL FROZEN)
- `.claude/rules/teams/team-common.md` — 규칙 인덱스
- `.claude/rules/teams/dispatch-protocol.md` — DISPATCH v2 입력
- `.claude/rules/teams/quality-standards.md` — 품질 SSOT
- `.claude/rules/teams/cc.md` — CC v2 오케스트레이션
- `.moai/specs/SPEC-GOVERNANCE-001/spec.md` — 흡수 대상 (SUPERSEDED 예정)
