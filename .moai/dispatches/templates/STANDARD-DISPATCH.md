# DISPATCH {SPRINT}-{ROUND} — {Team Name}

## Sprint: S{NN} | Round: R{M} | Issued: {YYYY-MM-DD}
## Team: {Team A | Team B | Coordinator | Design | QA | RA}
## Priority: {P1-Critical | P2-High | P3-Medium | P4-Low}
## 근거 SPEC/문서: **{SPEC-XXX-NNN 또는 docs/ 경로 — [HARD] 생략 불가}**

---

## 배경 (Background)

{이 라운드의 작업이 필요한 이유. 선행 SPEC/사고/갭 분석 결과 등을 1~3문장으로 기술.
근거 SPEC이 없는 경우 DISPATCH 발행 불가 — 이 필드는 삭제 불가.}

---

## Tasks

### T1: {Title} [P{N}]
- **설명**: {specific instruction}
- **체크리스트**:
  - [ ] {verifiable action 1}
  - [ ] {verifiable action 2}
  - [ ] {verifiable action 3}
- **완료 조건**: {objective, measurable success criterion}

### T2: {Title} [P{N}]
- **설명**: ...
- **체크리스트**:
  - [ ] ...
- **완료 조건**: ...

### T_n: DISPATCH Status 실시간 업데이트 (표준)
- **설명**: 작업 시작 시 IN_PROGRESS, 완료 시 COMPLETED, 차단 시 BLOCKED — 타임스탬프 필수
- **완료 조건**: Status 테이블에 모든 Task 타임스탬프가 정확히 반영

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | {title} | NOT_STARTED | {Team} | P{N} | - | - |
| T2 | {title} | NOT_STARTED | {Team} | P{N} | - | - |

상태 전환 규칙: `dispatch-protocol.md` §2 참조.
타임스탬프 포맷: `YYYY-MM-DDTHH:MM:SS+09:00` (KST, ISO-8601).

---

## Constraints [HARD]

- 소유 모듈만 수정: {팀별 모듈 명시 — role-matrix.md §3 참조}
- DISPATCH 없는 자율 작업 금지
- 빌드/테스트 검증 없이 COMPLETED 보고 금지 (quality-standards.md §3 Self-Verification)
- ScheduleWakeup({300초 이상}) 유지 (session-lifecycle.md)

---

## Evidence Required

완료 보고 시 DISPATCH Status 비고 열에 아래 3개 증거 필수 (quality-standards.md §4):

1. `dotnet build HnVue.sln -c Release` 결과 (errors/warnings 개수)
2. `dotnet test` 결과 (PASS/FAIL/SKIP 수치 — 자기 소유 테스트 프로젝트만)
3. `git diff --name-only main..HEAD` 결과 (소유권 확인용)

Safety-Critical 모듈(Dose, Incident, Security, Update) 수정 시 추가:
- Stryker Mutation Score before/after
- Characterization test 결과

---

## 참고 문서

- **근거 SPEC**: `.moai/specs/{SPEC-ID}/` (spec.md, plan.md, acceptance.md, tasks.md)
- **팀 규칙**: `.claude/rules/teams/{team}.md`
- **공통 프로토콜**: `.claude/rules/teams/dispatch-protocol.md`, `cc-operating-protocol.md`, `quality-standards.md`, `session-lifecycle.md`
- **역할 경계**: `.claude/rules/teams/role-matrix.md` (CONSTITUTIONAL)

---

## 템플릿 사용 규칙 [HARD — 2026-04-22 시행]

- [HARD] 이 템플릿은 **실질 개발 업무 DISPATCH**에 사용. IDLE CONFIRM은 `IDLE-CONFIRM-DISPATCH.md` 참조
- [HARD] `근거 SPEC/문서` 필드가 비어 있으면 DISPATCH 발행 불가
- [HARD] `Evidence Required` 필드 삭제 금지
- [HARD] `Status 테이블` 타임스탬프 열 삭제 금지
- [HARD] DISPATCH 발행 시 `STANDARD-DISPATCH.md`의 모든 섹션 준수

---

Version: 1.0.0 (구버전 `dispatch-template.md` 대체)
Effective: 2026-04-22
