# Team Common Rules [INDEX]

팀 운영에 공통 적용되는 규칙의 **인덱스 파일**. 실제 규칙은 초점 파일에 정의.

CC v2 도입: 독립 worktree + PR-only + Gitea 이슈 추적.

---

## 규칙 파일 구조

| 파일 | 담당 내용 |
|------|----------|
| [`role-matrix.md`](./role-matrix.md) | **[CONSTITUTIONAL — FROZEN]** 7개 역할 정의 (CC 포함), 소유권 매트릭스, 사고 이력 |
| [`dispatch-protocol.md`](./dispatch-protocol.md) | DISPATCH 해석, Status 업데이트, Phase 종속성, 이슈 추적 |
| [`quality-standards.md`](./quality-standards.md) | **[SSOT]** 품질 지표 (빌드, 테스트, 커버리지, Safety-Critical) |
| [`session-lifecycle.md`](./session-lifecycle.md) | ScheduleWakeup, /clear, Stall Detection 세션 관리 |
| [`cc.md`](./cc.md) | CC(Command Center) 오케스트레이션 규칙 |
| `{team}.md` (team-a, team-b, coordinator, team-design, qa, ra) | 팀별 소유 모듈 + 팀 특화 규칙 |

---

## 필수 읽기 순서 (에이전트용)

```
1. role-matrix.md §2~§4       — 자기 팀 역할 확인
2. dispatch-protocol.md §1    — FIRST ACTION 준수
3. session-lifecycle.md §2    — ScheduleWakeup 재설정
4. {team}.md                  — 소유 모듈 확인
5. quality-standards.md §3    — 완료 전 Self-Verification
```

---

## Project Philosophy [Constitutional]

상세: `quality-standards.md` §1.

> **"Speed is not the goal. Quality and completeness are."**

- Completeness first: 3 tasks at 100% > 10 tasks at 80%
- Self-verification required: prove "0 errors" — assume 금지
- No false reports: unverified COMPLETED = 프로토콜 위반
- Scope compliance: only do what DISPATCH instructs
- Evidence-based: all completion claims include build logs + test results

---

## 주요 HARD 규칙 요약 (Quick Reference)

| 규칙 | 출처 파일 |
|------|----------|
| DISPATCH 시작 전 `git pull` 필수 | `dispatch-protocol.md` §1 |
| Status 업데이트 시 타임스탬프 필수 | `dispatch-protocol.md` §2 |
| DISPATCH 파일 이동은 사용자 직접 관리 | `dispatch-protocol.md` §3 |
| Phase 종속성: Phase 1 실패 시 후속 Phase IDLE 대기 | `dispatch-protocol.md` §6 |
| ScheduleWakeup 최소 300초, 하드코딩 금지 | `session-lifecycle.md` §2 |
| Safety-Critical 90% 커버리지 | `quality-standards.md` §2 |
| QA 판정은 최종, 사용자 승인 없이 번복 불가 | `quality-standards.md` §5 |
| DISPATCH 명명: `DISPATCH-S{NN}-R{M}-{TEAM}.md` | `STANDARD-DISPATCH.md` |
| DISPATCH Status는 team 브랜치에 push | `dispatch-protocol.md` §4 |
| CC는 team 브랜치에서 DISPATCH Status 읽기 | `cc.md` Operating Cycle |
| DISPATCH 수정 구분 (Status=팀, 나머지=CC) | `dispatch-protocol.md` §3 |

---

Version: 3.2.0 (DISPATCH Status push 명확화, CC 모니터링 프로토콜 반영)
Effective: 2026-04-22
Classification: Governance
