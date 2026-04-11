# DISPATCH Protocol v2.0

운영 중 발견된 문제를 반영한 개선된 DISPATCH 관리 규칙.

Updated: 2026-04-11 (v1.0 문제점 수정)

---

## 핵심 규칙 [HARD]

### 규칙 1: 팀당 단일 활성 DISPATCH

- [HARD] 각 팀에는 언제나 정확히 하나의 활성 DISPATCH만 존재한다
- [HARD] 새 DISPATCH 발행 시, 이전 PENDING/NOT_STARTED 파일은 즉시 SUPERSEDED 처리
- [HARD] SUPERSEDED 파일은 즉시 `active/` → `completed/`로 이동

### 규칙 2: DISPATCH 읽기 순서 (에이전트용)

에이전트가 자신의 DISPATCH를 찾을 때:
1. `active/_CURRENT.md` 를 먼저 읽는다 (팀별 현재 파일 인덱스)
2. 해당 파일을 읽는다
3. `active/` 디렉토리의 다른 파일은 무시한다
4. **경로는 Main 프로젝트 기준 절대경로를 사용한다** (worktree 상대경로 금지)

### 규칙 3: DISPATCH 완료 커밋은 Main 기준

- [HARD] DISPATCH Status 업데이트는 worktree 커밋이 아닌, **main 브랜치**에 반영되어야 한다
- [HARD] 팀이 작업 완료 시 → `team/{팀}` 브랜치 PR → main 병합 후 → DISPATCH 파일 completed/ 이동
- PR 병합 후 Commander Center가 DISPATCH 상태를 completed/로 정리한다

### 규칙 4: 상태 머신

```
NOT_STARTED → IN_PROGRESS → COMPLETED → (moved to completed/)
PENDING     → IN_PROGRESS → COMPLETED → (moved to completed/)
              ↓
           SUPERSEDED (새 DISPATCH 발행 시) → (moved to completed/)
```

유효한 상태값: `NOT_STARTED`, `PENDING`, `IN_PROGRESS`, `COMPLETED`, `SUPERSEDED`

---

## DISPATCH 발행 절차 (Commander Center용)

새 라운드 DISPATCH 발행 시:

```bash
# 1. 이전 DISPATCH 아카이브
git mv .moai/dispatches/active/DISPATCH-{TEAM}-{OLD_DATE}.md \
       .moai/dispatches/completed/

# 2. 이전 파일 상태 SUPERSEDED로 업데이트
# (Edit 도구로 Status 필드 수정)

# 3. 새 DISPATCH 파일 생성
# (파일명 형식: S{스프린트}-R{라운드}-{팀}.md)

# 4. _CURRENT.md 인덱스 업데이트

# 5. git commit -m "chore(dispatch): S04-R1 발행, 이전 dispatch 아카이브"
```

---

## 문제 진단 체크리스트

팀이 "작업이 없다" 또는 "이전 작업만 보고"할 때:

| 증상 | 원인 | 해결 |
|------|------|------|
| 오래된 날짜 DISPATCH 보고 | active/에 구 DISPATCH 잔존 | git mv → completed/ |
| "작업 없음" 보고 | _CURRENT.md 미존재 또는 오래됨 | _CURRENT.md 업데이트 |
| worktree가 새 DISPATCH 못 읽음 | worktree 브랜치가 구 커밋 | main 브랜치 절대경로로 DISPATCH 읽기 |
| 두 DISPATCH 동시 보고 | active/에 복수 파일 공존 | SUPERSEDED 처리 후 이동 |

---

## v1.0 대비 변경 사항

| 항목 | v1.0 (문제) | v2.0 (개선) |
|------|-------------|-------------|
| 활성 DISPATCH 식별 | 없음 (디렉토리 전체) | _CURRENT.md 인덱스 |
| 이전 DISPATCH 처리 | 방치 (active/에 잔존) | SUPERSEDED → completed/ 즉시 이동 |
| 완료 상태 반영 | worktree에만 존재 | main PR 병합 후 반영 |
| 에이전트 DISPATCH 경로 | worktree 상대경로 | Main 절대경로 |
| 상태값 | PENDING/COMPLETED | NOT_STARTED/PENDING/IN_PROGRESS/COMPLETED/SUPERSEDED |
