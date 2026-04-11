# DISPATCH Current Index

> **[HARD] 에이전트 FIRST ACTION**: 이 파일을 가장 먼저 읽는다.
> 자신의 팀 행(row)에서 파일명을 확인한 뒤, 해당 파일만 읽는다.
> 상태가 `PR_OPEN` 또는 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고.

Updated: 2026-04-12 (S05-R2 시작)
DISPATCH 절대 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`

---

## 현재 팀별 DISPATCH 상태

| 팀 | 현재 DISPATCH 파일 | 상태 | 비고 |
|----|-------------------|------|------|
| **Coordinator** | — | **IDLE** | S05-R1 완료. 신규 DISPATCH 없음 — IDLE 보고 대기 |
| **QA** | `S05-R2-qa.md` | **ACTIVE** | 릴리즈 준비도 보고서 생성 (DOC-034) |
| **RA** | `S05-R2-ra.md` | **ACTIVE** | RMP v2.0 업데이트 (4-Tier + MR-072) |
| **Design** | `S05-R2-design-2.md` | **ACTIVE** | MergeView PPT Slide 12-13 구현 |
| **Team A** | — | **IDLE** | S05-R1 완료 (Data 85%+). 신규 DISPATCH 없음 — IDLE 보고 대기 |
| **Team B** | — | **IDLE** | S05-R1 완료. 신규 DISPATCH 없음 — IDLE 보고 대기 |

**→ S05 Round 2 시작. ACTIVE 팀은 자신의 DISPATCH 파일을 읽고 즉시 착수.**

---

## 중요 규칙 변경 (S05부터 적용)

```
[HARD] DISPATCH.md (루트) 와 CLAUDE.md 는 CC 전용 파일입니다.
팀 브랜치에서 이 파일들을 절대 수정하지 마세요.
→ .gitattributes에 merge=ours 규칙 추가됨 (자동 충돌 방지)

상태 업데이트: 반드시 .moai/dispatches/active/S05-R2-{team}.md 파일만 수정
```

---

## 실행 전 필수: 브랜치 최신화

```bash
# 각 팀은 작업 시작 전 반드시 실행
git checkout team/{your-team}
git pull origin main
git push origin team/{your-team}
```

---

## IDLE 상태 팀 행동 지침

```
상태가 IDLE인 경우:
1. 아래 IDLE 보고를 Commander Center에 전달
2. 추가 작업을 임의로 시작하지 않는다

IDLE 보고 형식:
  State: IDLE
  Reason: _CURRENT.md 상태가 IDLE — 신규 DISPATCH 없음
  Last completed: [마지막 완료 작업 요약]
  Awaiting: New DISPATCH from Commander Center
```

---

## DISPATCH 라운드 이력

| 날짜 | 라운드 | 파일 | 상태 |
|------|--------|------|------|
| 2026-04-08 | Phase 0 | DISPATCH-*-2026-04-08.md | `completed/` 아카이브 |
| 2026-04-09 | S03 QA Coverage | DISPATCH-*-2026-04-09.md | `completed/` 아카이브 |
| 2026-04-11 | S04 R1+R2 | S04-R{1,2}-*.md | `completed/` 아카이브 (PR #77-82 머지 완료) |
| 2026-04-12 | S05 R1 | S05-R1-*.md | `completed/` 아카이브 (전팀 완료) |
| 2026-04-12 | **S05 R2** | **S05-R2-*.md** | **ACTIVE — 현재** |

---

## Commander Center 전용 — 신규 DISPATCH 발행 절차

```
1. 기존 ACTIVE 파일 상태를 MERGED 또는 SUPERSEDED로 변경
2. completed/ 폴더로 이동
3. 신규 DISPATCH 파일 생성 (S{N}-R{N}-{team}.md)
4. 이 표의 해당 팀 행 업데이트
5. git add .moai/dispatches/ && git commit && git push origin main
```
