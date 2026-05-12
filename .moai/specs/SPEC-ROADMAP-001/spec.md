---
id: SPEC-ROADMAP-001
version: 1.0.0
status: approved
priority: P1
team: meta
title: 자율주행 방법론 적용 로드맵 — S18+ 라운드 우선순위 계획
created: 2026-05-10
updated: 2026-05-10
author: manager-spec
issue_number: 0
parent: SPEC-METHODOLOGY-001
---

## HISTORY

| 버전 | 날짜 | 변경 내용 |
|------|------|-----------|
| 1.0.0 | 2026-05-10 | 초안 작성. S18+ 라운드 우선순위 계획 |

---

## 개요 (Overview)

SPEC-METHODOLOGY-001 + 4개 부속 SPEC(CONSTITUTION, DISPATCH-V2, AUTODRIVE-GATE, SPEC-TRIAGE)이 발효된 후 S18+ 라운드의 적용 순서와 우선순위를 정의한다.

**중요 원칙**:
- 시간 예측(2-3일/1주/다음달) 절대 금지 — P0/P1/P2 우선순위 라벨만 사용.
- 각 라운드는 독립 산출물 + Self-Verification 통과로 완결.
- Phase Dependency(SPEC-DISPATCH-V2-001 REQ-DISPATCH-V2-003) 절대 준수.

---

## 요구사항 (EARS 형식)

### [REQ-ROADMAP-001] S18-R1 — 신 방법론 발효 첫 라운드

**유형**: Event-Driven

**WHEN** SPEC-METHODOLOGY-001 + 4개 부속 SPEC이 모두 사용자 승인을 받은 후,
**THE SYSTEM SHALL** S18-R1 라운드를 다음 우선순위로 발행해야 한다:

| Phase | 팀 | 작업 | 우선순위 |
|-------|-----|------|----------|
| Phase 1 | CC | SPEC-CONSTITUTION-001 헌법 게시 + role-matrix.md amendment 제안 | P0 |
| Phase 1 | RA | 마이그레이션 보고서 .moai/reports/methodology-migration-S18-R1.md 작성 | P0 |
| Phase 2 | CC | SPEC-SPEC-TRIAGE-001 마이그레이션 액션 A1~A6 실행 | P0 |
| Phase 2 | (전팀) | Self-Verification 7항목 체크리스트 적용 시작 | P0 |
| Phase 3 | QA | spec-triage-S18-R1.md 검증 | P1 |

**금지**: S18-R1에서 소스코드 변경 작업 금지 (방법론 정착 전용).

**근거**: 신 방법론의 첫 라운드는 도입 자체가 산출물. 코드 변경과 혼합 시 검증 어려움.

---

### [REQ-ROADMAP-002] S18-R2 — DISPATCH v2 게이트 활성화

**유형**: Event-Driven

**WHEN** S18-R1이 정상 종료(전팀 COMPLETED 또는 명시 SKIP)될 때,
**THE SYSTEM SHALL** S18-R2 라운드를 다음 우선순위로 발행해야 한다:

| Phase | 팀 | 작업 | 우선순위 |
|-------|-----|------|----------|
| Phase 1 | CC | SPEC-DISPATCH-V2-001 운영 게이트 활성화 (Phase Dependency 자동화) | P0 |
| Phase 1 | (구현 팀) | DISPATCH Status 의무 업데이트 시작 (REQ-DISPATCH-V2-004) | P0 |
| Phase 2 | (구현 팀) | 첫 실질 코드 작업 — SPEC-TEAMB-COV-001 잔여 작업 진행 | P1 |
| Phase 3 | QA | DISPATCH v2 게이트 정상 동작 검증 | P1 |

**근거**: S18-R1에서 헌법/SPEC 정착 후 S18-R2에서 게이트 활성화. 동시 적용 시 디버깅 어려움.

---

### [REQ-ROADMAP-003] S18-R3 — 자율주행 게이트 자동화

**유형**: Event-Driven

**WHEN** S18-R2가 정상 종료될 때,
**THE SYSTEM SHALL** S18-R3 라운드를 다음 우선순위로 발행해야 한다:

| Phase | 팀 | 작업 | 우선순위 |
|-------|-----|------|----------|
| Phase 1 | CC | SPEC-AUTODRIVE-GATE-001 사망 나선 감지 자동화 (REQ-AUTOGATE-005) | P0 |
| Phase 1 | (구현 팀) | Self-Verification 7항목 정착 + 빌드 증거 의무화 (REQ-AUTOGATE-001) | P0 |
| Phase 2 | (구현 팀) | SPEC-INFRA-002, SPEC-COORDINATOR-001 잔여 작업 진행 | P1 |
| Phase 3 | QA | Stall Detection + Safety-Critical 90% 게이트 운영 검증 | P0 |

**근거**: 사망 나선 감지는 가장 큰 안전망. 게이트 자동화 후 본격적인 코드 작업 진행.

---

### [REQ-ROADMAP-004] S19+ — Safety-Critical 잔여 커버리지 + UI 잔여 SPEC

**유형**: Event-Driven

**WHEN** S18-R3 종료 후 Safety-Critical 모듈 90% 미달 항목이 잔존할 때,
**THE SYSTEM SHALL** S19 이후 라운드에서 다음 우선순위로 작업을 진행해야 한다:

| 우선순위 | 작업 | 책임 SPEC |
|----------|------|-----------|
| P0 | Safety-Critical 모듈(Dose, Incident, Security, Update) 90% 커버리지 달성 | SPEC-TEAMB-COV-001 |
| P0 | Team B 잔여 수정 작업 | SPEC-TEAMB-FIX-001 |
| P1 | UI 잔여 SPEC 진행 | SPEC-UI-001 |
| P1 | 인프라 후속 작업 | SPEC-INFRA-002 |
| P2 | Coordinator 통합 마무리 | SPEC-COORDINATOR-001 |
| P2 | 새 SPEC 발행 (필요 시) | 별도 SPEC-PLAN |

**금지**: 시간 예측("S19를 5월 마지막 주에 시작") 사용 금지 — 우선순위 라벨만 사용.

**근거**: SPEC-METHODOLOGY-001 NG-4. Safety-Critical 잔여 커버리지는 별도 SPEC이 책임.

---

### [REQ-ROADMAP-005] 라운드 종료 시 회귀 평가

**유형**: Event-Driven

**WHEN** 각 라운드가 종료될 때,
**THE SYSTEM SHALL** 다음 회귀 평가를 수행하고 결과를 `.moai/reports/round-{SNN-RM}-retro.md`에 기록해야 한다:
- 본 라운드 실질 커밋 수치
- Self-Verification 7항목 통과율
- DISPATCH Status 업데이트 시점 정합성
- 사망 나선 카운터 변화
- TIMEOUT 발생 횟수 및 사유

**근거**: 각 라운드의 학습이 다음 라운드의 입력. 회귀 평가 누락 시 사망 나선 재발 위험.

---

### [REQ-ROADMAP-006] 사용자 결정 게이트 명시

**유형**: Event-Driven

**WHEN** 다음 상황 중 하나가 발생할 때,
**THE SYSTEM SHALL** CC가 다음 라운드 발행 전에 사용자 명시 승인을 받아야 한다:
- 사망 나선 5 라운드 임계 도달 (REQ-AUTOGATE-005)
- 연속 3회 TIMEOUT 발생 (REQ-DISPATCH-V2-006)
- 헌법 amendment 필요 (FROZEN 영역 변경 제안)
- Safety-Critical 모듈 새 BLOCKED 발생

**근거**: 자율주행은 사람의 판단을 대체하지 않는다. 안전망 임계에서 반드시 사용자 결정.

---

## 라운드 종속 그래프

```
S18-R1 (방법론 발효 — 코드 변경 없음)
    ↓ 정상 종료
S18-R2 (DISPATCH v2 게이트 활성화 + 코드 작업 시작)
    ↓ 정상 종료
S18-R3 (자율주행 게이트 자동화 + 본격 코드 작업)
    ↓ 정상 종료
S19+ (Safety-Critical 잔여 커버리지 + UI 잔여 SPEC)
    ↓ 사망 나선/TIMEOUT 임계 시
사용자 결정 게이트 (REQ-ROADMAP-006)
```

---

## TRUST 5 매핑

- **Tested**: acceptance.md에서 라운드 종속 그래프 + 우선순위 라벨 사용 검증.
- **Readable**: 라운드별 표 형식. P0/P1/P2 라벨로 우선순위 명확.
- **Unified**: 모든 라운드가 Phase 1→2→3 동일 구조. SPEC-DISPATCH-V2-001 게이트와 일관.
- **Secured**: REQ-ROADMAP-006 사용자 결정 게이트로 안전망 강화.
- **Trackable**: 라운드별 회귀 보고서(REQ-ROADMAP-005) 의무화.

---

## 사고 케이스 인용

- **S15~S16-R1 사망 나선**: REQ-ROADMAP-001 (S18-R1 코드 변경 금지) + REQ-ROADMAP-005 (회귀 평가) — 방법론 정착 전 코드 작업 차단.
- **S14-R2 main 동기화 누락**: REQ-ROADMAP-002 (S18-R2에서 DISPATCH v2 활성화 후 코드 작업) — 게이트 미활성 상태에서 코드 작업 차단.

---

## SPEC 참조

- SPEC-METHODOLOGY-001 (parent) — 5축 SPEC 발효 시점 정의
- SPEC-CONSTITUTION-001 — S18-R1 작업 대상
- SPEC-DISPATCH-V2-001 — S18-R2 활성화 대상
- SPEC-AUTODRIVE-GATE-001 — S18-R3 활성화 대상
- SPEC-SPEC-TRIAGE-001 — S18-R1 마이그레이션 대상
- SPEC-TEAMB-COV-001 — S19+ 우선순위 대상

---

## 제외 범위 (What NOT to Build)

- 시간 예측 기반 일정 ("X일 내 완료")
- 자동 라운드 발행 시스템 (CC + 사용자 수동)
- S20+ 상세 계획 (S18-S19 진행 후 별도 SPEC)
- 새 SPEC 자동 생성 도구
- Sprint 단위 보고서 자동화 (수동 작성)

---

## 참고

- `SPEC-METHODOLOGY-001` (parent)
- `SPEC-CONSTITUTION-001`
- `SPEC-DISPATCH-V2-001`
- `SPEC-AUTODRIVE-GATE-001`
- `SPEC-SPEC-TRIAGE-001`
- `.claude/rules/teams/quality-standards.md` (P0/P1/P2 라벨링 권장)
