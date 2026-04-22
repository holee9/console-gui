# Team Common Rules [INDEX]

팀 운영에 공통 적용되는 규칙의 **인덱스 파일**. 실제 규칙은 초점 파일에 정의.

2026-04-22 재정비: 564줄 단일 파일을 5개 초점 파일로 분해.

---

## 규칙 파일 구조

| 파일 | 담당 내용 |
|------|----------|
| [`role-matrix.md`](./role-matrix.md) | **[CONSTITUTIONAL — FROZEN]** 7개 역할 정의, 소유권 매트릭스, 사고 이력 |
| [`dispatch-protocol.md`](./dispatch-protocol.md) | DISPATCH 해석, Status 업데이트, 파일 관리, 이슈 추적 |
| [`cc-operating-protocol.md`](./cc-operating-protocol.md) | CC 운영 절차 (모니터링, 머지, 자율진행, TIMEOUT, 시간분석) |
| [`quality-standards.md`](./quality-standards.md) | **[SSOT]** 품질 지표 (빌드, 테스트, 커버리지, Safety-Critical) |
| [`session-lifecycle.md`](./session-lifecycle.md) | ScheduleWakeup, /clear, Stall Detection 세션 관리 |
| `{team}.md` (team-a, team-b, coordinator, team-design, qa, ra) | 팀별 소유 모듈 + 팀 특화 규칙 |

---

## 필수 읽기 순서 (에이전트용)

### 팀 에이전트 세션 시작 시

```
1. role-matrix.md §2~§5       — 자기 팀 역할 확인
2. dispatch-protocol.md §1    — FIRST ACTION 준수
3. session-lifecycle.md §2    — ScheduleWakeup 재설정
4. {team}.md                  — 소유 모듈 확인
5. quality-standards.md §3    — 완료 전 Self-Verification
```

### CC 세션 시작 시

```
1. role-matrix.md §2          — CC Mantra + 허용/금지 작업
2. cc-operating-protocol.md   — 운영 절차 전체
3. dispatch-protocol.md §3    — DISPATCH File Management
4. quality-standards.md       — 품질 게이트 기준 (판정은 QA)
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
| DISPATCH 파일 이동은 CC 전유 | `dispatch-protocol.md` §3 |
| CC dotnet/코드수정/에이전트호출 금지 | `role-matrix.md` §2 |
| CC 정지 조건 5가지 외 자율 실행 | `cc-operating-protocol.md` §5 |
| ScheduleWakeup 최소 300초, 하드코딩 금지 | `session-lifecycle.md` §2 |
| Safety-Critical 90% 커버리지 | `quality-standards.md` §2 |
| QA 판정은 최종, CC 번복 불가 | `quality-standards.md` §5 |
| DISPATCH 템플릿 `STANDARD-DISPATCH.md` 준수 | `.moai/dispatches/templates/` |
| 근거 SPEC 없는 DISPATCH 발행 불가 | `cc-operating-protocol.md` §4 |
| 2라운드 연속 IDLE CONFIRM 경고 | `cc-operating-protocol.md` §9 |

---

## 변경 이력

| 버전 | 날짜 | 변경 |
|------|------|------|
| 1.0 | 2026-04-01~14 | 최초 작성, 단일 파일 |
| 1.5 | 2026-04-15~21 | S07~S15 사고 대응 HARD 규칙 누적 (564줄 비대화) |
| **2.0** | **2026-04-22** | **5개 초점 파일로 분해, 본 파일은 인덱스로 축소** |

---

## 재정비 근거

`.moai/plans/SYSTEM-REFORM-2026-04-22.md` 참조.

기존 564줄 단일 파일의 구조적 결함:
- SSOT 위배 (Quality Standards 중복)
- Incident-driven patch 축적 (모순 규칙 발생)
- 거버넌스 소유권 공백
- 탐색 비효율

재정비 후:
- 각 초점 파일 ≤ 250줄
- SSOT 단일화 (quality-standards.md)
- 명확한 cross-reference
- 사고 교훈은 role-matrix.md §9에 통합 보관

---

Version: 2.0.0 (INDEX)
Effective: 2026-04-22
Classification: Governance
