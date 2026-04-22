# System Reform — 2026-04-22

## 1. 배경

직전 턴에서 사용자의 요청을 오독하여 S16-R2 팀 업무 DISPATCH를 재발행했음.
사용자의 본래 의도는 **업무 할당이 아니라 업무를 수행하는 시스템 자체의 재정비**.
이 계획서는 시스템 구조적 결함을 정의하고 해소 방안을 기록한다.

---

## 2. 구조적 결함 (Structural Pathologies)

### 결함 1. SSOT(Single Source of Truth) 위배
- "Quality Standards SSOT"를 team-common.md가 주장하나, `moai-constitution.md`, `qa.md`, DISPATCH 본문에도 분산
- CC 역할 경계가 role-matrix.md(261줄) + team-common.md(564줄) 두 곳에 중복 정의

### 결함 2. Incident-Driven Patch 축적
- team-common.md는 S07, S09, S14, S15 등 사고마다 HARD 규칙을 **append-only로 누적**
- 통폐합 없이 564줄로 비대화
- 모순 사례: `/clear 금지 → 철회 → 유지` (3회 오락가락)

### 결함 3. DISPATCH 템플릿 표준 부재
- `.moai/dispatches/templates/dispatch-template.md` 구버전 (PowerShell 기반 "지시서대로 작업해" 워크플로우)
- 실제 DISPATCH는 hand-written — 근거 SPEC 필드, Exit Criteria, Evidence Required 강제 없음
- IDLE CONFIRM 전용 DISPATCH가 자기복제되어 프로세스 사망 나선 유발

### 결함 4. 거버넌스 소유권 공백
- `.claude/rules/`, `.moai/config/`, `.moai/dispatches/`는 role-matrix.md에 **소유권 명시 없음**
- 결과: 시스템 파일 변경의 책임 불명확 → CC가 무제한 땜질

### 결함 5. 메모리 단편화
- `~/.claude/projects/.../memory/`에 48개 feedback 파일
- CC 규칙 관련만 10개 중복:
  `cc_no_build`, `cc_no_dotnet_monitoring`, `cc_no_implementation`, `cc_no_spawn`,
  `cc_no_ask_execute`, `cc_no_ask_next_round`, `cc_proactive_fix`, `cc_process_proactive`,
  `commander_center`, `operations_protocol`
- MEMORY.md 200줄 제한 근접 → 규칙 탐색 비효율

---

## 3. 재정비 축 (Reform Axes)

| 축 | 현재 | 목표 |
|----|------|------|
| A. 규칙 구조 | team-common.md 564줄 단일 파일 | 4개 초점 파일 + 인덱스 (각 ≤250줄) |
| B. 템플릿 강제 | hand-written DISPATCH | STANDARD-DISPATCH.md + 근거 SPEC 의무 |
| C. 거버넌스 | 소유권 공백 | role-matrix.md에 governance scope 추가 |
| D. 메모리 통합 | 48 feedback 파일 | CC 규칙 10개 → 1개 통합 |
| E. 사고 교훈 | 각 규칙에 혼재 | incident-archive.md로 분리 |

---

## 4. 조치 목록 (이번 재정비 사이클)

### 4.1 즉시 조치
- [x] CronCreate `b716ee35` 취소 (조기 설정)
- [x] S16-R2 DISPATCH 6건 → HOLD 표시 (`_CURRENT.md`에 HOLD 공지)

### 4.2 규칙 구조 재조직
team-common.md를 4개 파일로 분해:

| 파일 | 담당 내용 | 기존 섹션 |
|------|----------|----------|
| `dispatch-protocol.md` | DISPATCH Resolution, Status Update, File Management, Issue Tracking | §1, §9~§11, §16 |
| `cc-operating-protocol.md` | CC Role(요약), CC Monitoring, Merge, Auto-Progression, TIMEOUT, Time Analysis | §2, §12~§15 |
| `quality-standards.md` | Quality Standards (SSOT), Self-Verification, Project Philosophy | §4~§6 |
| `session-lifecycle.md` | ScheduleWakeup, /clear, Stall Detection, 폴링 | §3, §11 (Session 부분) |

team-common.md는 ≤80줄 인덱스 파일로 축소.

### 4.3 DISPATCH 템플릿 표준화
`STANDARD-DISPATCH.md` 신규 작성:
- 필수 필드: Sprint/Round, Team, 근거 SPEC/문서, Priority, Tasks, Status 테이블(타임스탬프 포함), Constraints, Evidence Required
- `IDLE-CONFIRM-DISPATCH.md` 별도 템플릿: 2회 연속 발행 시 경고, 3회 연속 시 프로토콜 위반 기록
- 근거 SPEC 없는 DISPATCH는 **발행 불가** 규칙 추가

### 4.4 role-matrix.md 거버넌스 확장
- 신규 섹션: "11. Governance Ownership"
- `.claude/rules/`, `.moai/config/`, `.moai/dispatches/`, `.moai/plans/` 소유권 명시
- 원칙: CC + Coordinator 공동 거버넌스 (구조 변경은 사용자 승인 필요)

### 4.5 메모리 통합
- 10개 CC 규칙 memory → `feedback_cc_operating_rules.md` 단일 파일로 통합
- 중복 제거 후 archive/로 이동
- MEMORY.md 인덱스 정리 (목표: ≤25 active 항목)

### 4.6 사고 교훈 분리
- `incident-archive.md` 신규 작성
- S05~S15 사고 사례를 규칙에서 분리하여 참조 전용 문서로 보관
- 규칙 파일은 "결론 규칙"만 보유, 사고 상세는 archive 참조

---

## 5. 성공 기준 (Exit Criteria)

- [ ] team-common.md ≤ 80줄 (인덱스)
- [ ] 4개 초점 파일 각각 ≤ 250줄
- [ ] CLAUDE.md 크기 변화 없음 (재정비는 .claude/rules/ 내부 작업)
- [ ] STANDARD-DISPATCH.md 템플릿 확정 + 기존 DISPATCH 구버전과 대조 검증
- [ ] role-matrix.md 섹션 11 (Governance) 추가
- [ ] feedback_cc_operating_rules.md 생성 + 10개 원본 archive 이동
- [ ] 모든 변경 단일 커밋 + push

---

## 6. 재정비 후 후속 작업

1. S16-R2 HOLD 해제 판단: 재정비 완료 후 DISPATCH 내용을 STANDARD 형식으로 재발행 여부 결정
2. CronCreate 재설정: CC 자동 모니터링 복귀 (단, ACTIVE 팀 존재 시에만)
3. 사고 방지 검증: 다음 3라운드 동안 DISPATCH 내용이 STANDARD 준수하는지 확인
4. 메트릭 관찰: "실질 커밋 vs 프로토콜 커밋" 비율 측정

---

Version: 1.0.0
Author: MoAI Commander Center (ultrathink system reform)
Created: 2026-04-22
Scope: `.claude/rules/teams/*`, `.moai/dispatches/templates/*`, MEMORY.md
Classification: Governance Reform
