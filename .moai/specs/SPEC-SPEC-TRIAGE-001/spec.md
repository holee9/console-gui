---
id: SPEC-SPEC-TRIAGE-001
version: 1.0.0
status: draft
priority: P1
team: meta
title: SPEC 재고 정리 — 3분류(ACTIVE/COMPLETED/DEPRECATED), 기존 7개 SPEC 마이그레이션
created: 2026-05-10
updated: 2026-05-10
author: manager-spec
issue_number: 0
parent: SPEC-METHODOLOGY-001
---

## HISTORY

| 버전 | 날짜 | 변경 내용 |
|------|------|-----------|
| 1.0.0 | 2026-05-10 | 초안 작성. 기존 7개 SPEC 분류 매트릭스 확정 |

---

## 개요 (Overview)

`.moai/specs/` 디렉토리의 SPEC 재고 위생을 회복한다. 3분류 기준(ACTIVE/COMPLETED/DEPRECATED)을 정의하고, 현재 7개 기존 SPEC을 분류하며 RTM 갱신 절차를 제공한다.

본 SPEC은 운영 위생(operational hygiene) 작업이며, 헌법이나 프로토콜 수준의 변경을 포함하지 않는다.

---

## 요구사항 (EARS 형식)

### [REQ-TRIAGE-001] 3분류 기준 정의

**유형**: Ubiquitous

**THE SYSTEM SHALL** 모든 `.moai/specs/SPEC-*/spec.md` 파일을 다음 3분류로 분류해야 한다:

| 분류 | 기준 | 처리 |
|------|------|------|
| ACTIVE | `status: approved` 또는 `status: draft` 또는 `status: partial` AND 최근 30일 내 활동 | 현행 유지 |
| COMPLETED | `status: implemented` AND 머지된 PR 존재 (CHANGELOG 또는 git log에서 증거) | `.moai/specs/_archive/completed/`로 이동 |
| DEPRECATED | `supersedes:` 필드에 의해 다른 SPEC이 명시 OR 60일 미활동 OR 중복 | `.moai/specs/_archive/deprecated/`로 이동 |

**근거**: SPEC 재고가 위생적으로 관리되어야 본 부속 SPEC들의 RTM이 신뢰성을 가진다.

---

### [REQ-TRIAGE-002] 기존 7개 SPEC 분류 매트릭스

**유형**: Ubiquitous

**THE SYSTEM SHALL** 본 SPEC 작성 시점(2026-05-10)에 존재하는 7개 SPEC을 다음과 같이 분류한다:

| SPEC ID | 분류 | 사유 |
|---------|------|------|
| SPEC-COORDINATOR-001 | **ACTIVE** | 현행 운영 SPEC, Coordinator 통합 작업 진행 중 |
| SPEC-INFRA-001 | **COMPLETED** | 인프라 초기 구성 완료 (분석 시 머지 PR 존재 가정 — 검증 필요) |
| SPEC-INFRA-002 | **ACTIVE** | 인프라 후속 작업 진행 중 |
| SPEC-TEAMB-COV-001 | **ACTIVE** | Team B 잔여 커버리지 진행 중 (Safety-Critical 핵심) |
| SPEC-TEAMB-FIX-001 | **ACTIVE** | Team B 수정 작업 진행 중 |
| SPEC-UI-001 | **ACTIVE (메타데이터 보강 필요)** | status/priority/team 필드 누락 의심 — 보강 필수 |
| SPEC-GOVERNANCE-001 | **DEPRECATED** | SPEC-METHODOLOGY-001로 SUPERSEDED — 8개 요구사항 5축 분산 |

**근거**: 본 SPEC 작성 시점 .moai/specs/ 디렉토리 스냅샷 기준. 분류 정확도는 마이그레이션 보고서에서 검증.

---

### [REQ-TRIAGE-003] SPEC-GOVERNANCE-001 SUPERSEDED 마킹 절차

**유형**: Event-Driven

**WHEN** SPEC-METHODOLOGY-001 status가 `approved`로 전환될 때,
**THE SYSTEM SHALL** SPEC-GOVERNANCE-001/spec.md에 다음 변경을 적용해야 한다:
1. frontmatter에 `status: deprecated` 추가
2. frontmatter에 `superseded_by: SPEC-METHODOLOGY-001` 추가
3. HISTORY 섹션 상단에 SUPERSEDED 기록 추가
4. 본문에 "이 SPEC은 SPEC-METHODOLOGY-001 + 5개 부속 SPEC으로 흡수되었습니다" 안내 박스 추가
5. 60일 후 `.moai/specs/_archive/deprecated/` 이동 (사용자 수동)

**금지**: 본 SPEC 작성 단계에서 GOVERNANCE-001 파일을 직접 편집하지 않는다 (오케스트레이터의 후속 task).

**근거**: 흡수 진행 중 GOVERNANCE-001 본문 보존. 60일 후 정식 아카이브.

---

### [REQ-TRIAGE-004] SPEC 메타데이터 보강 필수 필드

**유형**: Ubiquitous

**THE SYSTEM SHALL** 모든 ACTIVE SPEC이 다음 frontmatter 필드를 보유하도록 강제해야 한다:

| 필드 | 필수성 | 예시 |
|------|--------|------|
| id | 필수 | SPEC-UI-001 |
| version | 필수 | 1.0.0 |
| status | 필수 | draft / approved / partial / implemented / deprecated |
| priority | 필수 | P0 / P1 / P2 |
| team | 필수 | team-design / team-a / team-b / coordinator / qa / ra / meta |
| title | 필수 | 한글 또는 영어 제목 |
| created | 필수 | YYYY-MM-DD |
| updated | 필수 | YYYY-MM-DD |
| parent | 부속 SPEC만 | SPEC-METHODOLOGY-001 |
| supersedes | 흡수 SPEC만 | SPEC-XXX-NNN |

**보강 대상**: SPEC-UI-001은 본 SPEC에 따라 메타데이터 보강 필요.

**근거**: SPEC 메타데이터 일관성은 RTM 자동화 + 검색 + 분류 알고리즘의 전제 조건.

---

### [REQ-TRIAGE-005] RTM 갱신 절차

**유형**: Event-Driven

**WHEN** 본 SPEC의 분류 결과가 확정될 때,
**THE SYSTEM SHALL** 다음 RTM 갱신 작업을 수행해야 한다:
1. SPEC-METHODOLOGY-001의 RTM 매핑 표에 5축 부속 SPEC 5개 등록
2. ACTIVE SPEC 5개(COORDINATOR-001, INFRA-002, TEAMB-COV-001, TEAMB-FIX-001, UI-001)의 RTM 키를 유지
3. COMPLETED SPEC 1개(INFRA-001)는 `_archive/completed/`로 이동 후 RTM에서 "완료" 표기
4. DEPRECATED SPEC 1개(GOVERNANCE-001)는 SUPERSEDED 마킹만 적용 (60일 후 아카이브)
5. 결과를 `.moai/reports/spec-triage-S18-R1.md`에 기록

**근거**: RTM 갱신은 SPEC 재고 정리의 산출물. 누락 시 다음 라운드에서 stale 데이터로 의사결정 위험.

---

## 마이그레이션 액션 리스트

다음 액션은 사용자/오케스트레이터가 본 SPEC 승인 후 실행:

| # | 액션 | 책임 | 우선순위 |
|---|------|------|----------|
| A1 | SPEC-UI-001 frontmatter 보강 (status/priority/team) | Design 팀 | P0 |
| A2 | SPEC-INFRA-001 머지 PR 증거 확인 후 COMPLETED 확정 | 사용자 | P1 |
| A3 | SPEC-GOVERNANCE-001 SUPERSEDED 마킹 적용 | 오케스트레이터 | P0 |
| A4 | `.moai/specs/_archive/{completed,deprecated}/` 디렉토리 생성 | 사용자 | P1 |
| A5 | spec-triage-S18-R1.md 보고서 생성 | manager-docs | P1 |
| A6 | SPEC-METHODOLOGY-001 RTM 표에 5축 등록 | 본 SPEC 발효 시 자동 | P0 |

---

## TRUST 5 매핑

- **Tested**: acceptance.md에서 7개 SPEC 분류 정확도 + 메타데이터 보강 자동 검증.
- **Readable**: 3분류 기준 표 + 7개 SPEC 매트릭스로 한눈에 파악 가능.
- **Unified**: SPEC frontmatter 필드 표준화 (REQ-TRIAGE-004) — 모든 SPEC 동일 스키마.
- **Secured**: 본 SPEC군은 운영 위생. 보안 관련성 낮음.
- **Trackable**: 마이그레이션 액션 리스트로 진행 추적 가능.

---

## 사고 케이스 인용

- **(구) GOVERNANCE-001 잔존**: 본 SPEC REQ-TRIAGE-003으로 정식 SUPERSEDED 처리. 흡수 누락 방지.
- **S15~S16-R1 사망 나선**: 본 SPEC REQ-TRIAGE-002 — 무활동 SPEC 60일 임계로 stale 누적 차단.

---

## SPEC 참조

- SPEC-METHODOLOGY-001 (parent) — RTM 갱신의 최종 대상
- SPEC-CONSTITUTION-001 — FROZEN 영역 정의 (ACTIVE SPEC 자체 수정 권한 명시)

---

## 제외 범위 (What NOT to Build)

- 자동 SPEC 분류 도구/스크립트 (수작업 + 사용자 검증)
- SPEC 본문 자동 편집 도구 (오케스트레이터의 후속 task)
- 60일 자동 아카이브 자동화 (사용자 수동)
- 새 SPEC 생성 (본 SPEC은 재고 정리 한정)

---

## 참고

- `.moai/specs/SPEC-COORDINATOR-001/spec.md`
- `.moai/specs/SPEC-INFRA-001/spec.md`
- `.moai/specs/SPEC-INFRA-002/spec.md`
- `.moai/specs/SPEC-TEAMB-COV-001/spec.md`
- `.moai/specs/SPEC-TEAMB-FIX-001/spec.md`
- `.moai/specs/SPEC-UI-001/spec.md`
- `.moai/specs/SPEC-GOVERNANCE-001/spec.md`
- `SPEC-METHODOLOGY-001` (parent)
