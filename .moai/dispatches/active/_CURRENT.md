# DISPATCH Current Index

> **[HARD] 에이전트 FIRST ACTION**: 이 파일을 가장 먼저 읽는다.
> 자신의 팀 행(row)에서 파일명을 확인한 뒤, 해당 파일만 읽는다.
> 상태가 `PR_OPEN` 또는 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고.

Updated: 2026-04-11
DISPATCH 절대 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`

---

## 현재 팀별 DISPATCH 상태

| 팀 | 현재 DISPATCH 파일 | 상태 | 비고 |
|----|-------------------|------|------|
| **Coordinator** | `S04-R1-coordinator.md` | **PR_OPEN (#77)** | 작업완료, PR 검토 대기 |
| **QA** | `S04-R1-qa.md` | **PR_OPEN (#78)** | 작업완료, PR 검토 대기 |
| **RA** | `S04-R1-ra.md` | **PR_OPEN (#79)** | 작업완료, PR 검토 대기 |
| **Design** | `S04-R1-design.md` | **PR_OPEN (#80)** | 작업완료, PR 검토 대기 |
| **Team A** | `S04-R1-team-a.md` | **PR_OPEN (#81)** | 작업완료, PR 검토 대기 |
| **Team B** | `S04-R1-team-b.md` | **PR_OPEN (#82)** | 작업완료, PR 검토 대기 |

**→ 모든 팀 현재 IDLE. 신규 DISPATCH 없음. PR 병합 후 다음 라운드 발행 예정.**

---

## IDLE 상태 팀 행동 지침

```
상태가 PR_OPEN 또는 IDLE인 경우:

1. 아래 IDLE 보고를 Commander Center에 전달
2. 추가 작업을 임의로 시작하지 않는다
3. 이전 날짜 DISPATCH 파일(DISPATCH-*-2026-04-XX.md)을 찾아 실행하지 않는다

IDLE 보고 형식:
  State: IDLE
  Reason: _CURRENT.md 상태가 PR_OPEN — 신규 DISPATCH 없음
  PR: [PR 번호]
  Last completed: [마지막 완료 DISPATCH 요약]
  Awaiting: New DISPATCH from Commander Center
```

---

## DISPATCH 라운드 이력

| 날짜 | 라운드 | 파일 | 상태 |
|------|--------|------|------|
| 2026-04-08 | Phase 0 | DISPATCH-*-2026-04-08.md | `completed/` 아카이브 |
| 2026-04-09 | Phase 1 QA Coverage | DISPATCH-*-2026-04-09.md | SUPERSEDED → `completed/` |
| 2026-04-11 | S04 R1+R2 | S04-R1-*.md | PR_OPEN (#77~82) — 현재 |

---

## Commander Center 전용 — 신규 DISPATCH 발행 절차

```
1. 기존 PR_OPEN 파일 상태를 MERGED 또는 SUPERSEDED로 변경
2. 신규 DISPATCH 파일 생성 (파일명: S{N}-R{N}-{team}.md)
3. 이 표의 해당 팀 행 업데이트 (파일명 + 상태)
4. 모든 워크트리에 _CURRENT.md 전파 (cp 또는 git push)
```
