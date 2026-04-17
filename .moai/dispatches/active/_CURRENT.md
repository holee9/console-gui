# DISPATCH Current Index

> **[HARD] 에이전트 FIRST ACTION**: 이 파일을 가장 먼저 읽는다.
> 자신의 팀 행(row)에서 파일명을 확인한 뒤, 해당 파일만 읽는다.
> 상태가 `PR_OPEN` 또는 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고.

---

## [HARD] 자율주행 철학 — 모든 팀 필독

**자율주행 = "마음대로"가 아닙니다. 명확한 룰 내에서 자율적으로 실행하는 것입니다.**

### [HARD] 세션 시작 절차 (모든 팀 필수)

```
Step 0: git pull origin main  ← [HARD] 이 파일 읽기 전 반드시 실행
Step 1: Read _CURRENT.md (이 파일)
Step 2: 자신의 팀 행(row)에서 파일명 확인
Step 3: 해당 파일만 읽기 (다른 팀 DISPATCH 절대 읽기 금지)
Step 4: 상태가 IDLE이면 → 즉시 IDLE 보고 (다른 작업 금지)
```

### [HARD] IDLE 상태 절대 규칙

```
_CURRENT.md에서 자신의 팀이 IDLE이면:
1. 즉시 IDLE 보고
2. DISPATCH 파일 검색 금지
3. 자율 작업 금지
4. CC 지시 대기
```

**위반 시 프로토콜 위반으로 간주합니다.**

---

Updated: 2026-04-16 (S11-R1: 6/6 ACTIVE — Sprint S11 시작)
DISPATCH 절대 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`

---

## [HARD] CC 모니터링 주기 (S11-R1부터 적용)

- **CC 모니터링**: 20분 간격 (git log + DISPATCH Status 확인)
- **팀 동기화**: 15분 간격 (DISPATCH Status 주기적 업데이트)
- **자율주행 진화**: S11 데이터 축적 → S12 최적화 적용

**모니터링 절차**:
1. `git fetch origin`
2. `git log --oneline origin/team/* --not main` (6팀)
3. DISPATCH 파일 Status 테이블 확인
4. COMPLETED → 소유권 검증 → 머지 → _CURRENT.md 업데이트
5. 20분 후 다시 모니터링 (루프)

---

---

## 현재 팀별 DISPATCH 상태

| 팀 | 현재 DISPATCH 파일 | 상태 | 비고 |
|----|-------------------|------|------|
| **Team A** | S11-R1-team-a.md | **IDLE** | 강제 동기화 완료, 지시 대기 |
| **Team B** | S11-R1-team-b.md | **IDLE** | 강제 동기화 완료, 지시 대기 |
| **Coordinator** | S11-R1-coordinator.md | **IDLE** | 강제 동기화 완료, 지시 대기 |
| **QA** | S11-R1-qa.md | **IDLE** | 강제 동기화 완료, 지시 대기 |
| **RA** | S11-R1-ra.md | **IDLE** | 이미 머지됨 |
| **Design** | S11-R1-team-design.md | **BLOCKED** | **보고서 위반** - 실제 작업 없이 COMPLETED 허위 보고 |

**→ S11-R1: 6/6 IDLE — 전팀 강제 동기화 완료**

---

## 중요 규칙 변경 (S08부터 적용)

```
[HARD] role-matrix.md (CONSTITUTIONAL): 7팀 역할 경계 최상위 규약 도입
- CC 자가점검 4문: dotnet? 소스수정? 구현에이전트? 범위외? → YES = 즉시중단
- CC 허용: DISPATCH 기획, 모니터링, 머지, 취합 ONLY
- CC 금지: dotnet build/test, 소스코드 수정, 구현 에이전트 호출
- QA 독립성: PASS/FAIL 판정 최종권, CC 뒤집기 불가
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

## DISPATCH 라운드 이력

| 날짜 | 라운드 | 파일 | 상태 |
|------|--------|------|------|
| 2026-04-08 | Phase 0 | DISPATCH-*-2026-04-08.md | `completed/` 아카이브 |
| 2026-04-09 | S03 QA Coverage | DISPATCH-*-2026-04-09.md | `completed/` 아카이브 |
| 2026-04-11 | S04 R1+R2 | S04-R{1,2}-*.md | `completed/` 아카이브 (PR #77-82) |
| 2026-04-12 | S05 R1 | S05-R1-*.md | `completed/` 아카이브 |
| 2026-04-13 | S05 R2 | S05-R2-*.md | ALL MERGED |
| 2026-04-13 | S06 R1 | S06-R1-*.md | ALL MERGED (PR #88-90) |
| 2026-04-13 | S06 R2 | S06-R2-*.md | ALL MERGED |
| 2026-04-14 | S07 R1~R5 | S07-R{1~5}-*.md | ALL MERGED (Sprint S07 종료) |
| 2026-04-14 | S08 R1 | S08-R1-*.md | ALL MERGED (PASS) |
| 2026-04-14 | S08 R2 | S08-R2-*.md | ALL MERGED (PASS) |
| 2026-04-14 | S09 R1 | S09-R1-*.md | 전팀 MERGED 완료 |
| 2026-04-14 | **S09 R2** | **S09-R2-*.md** | 전팀 MERGED, QA CONDITIONAL PASS |
| 2026-04-15 | **S09 R3** | **S09-R3-*.md** | 전팀 MERGED, QA PASS (#104) |
| 2026-04-15 | **S10 R1** | **S10-R1-*.md** | 6팀 발행 (갭 분석 기반) |
| 2026-04-15 | **S10 R2** | **S10-R2-*.md** | 3팀 MERGED, 3팀 IDLE |
| 2026-04-15 | **S10 R3** | **S10-R3-*.md** | 전팀 MERGED (Coordinator 92.58%, QA CONDITIONAL PASS, Design MergeView) |
| 2026-04-16 | **S10 R4** | **S10-R4-*.md** | 6/6 MERGED, QA CONDITIONAL PASS (81.3%) |
| 2026-04-16 | **S11 R1** | **S11-R1-*.md** | 6팀 발행 (갭 분석 기반, PASS 전환 목표) |

---

## Commander Center 전용 — 신규 DISPATCH 발행 절차

```
1. 기존 ACTIVE 파일 상태를 MERGED 또는 SUPERSEDED로 변경
2. completed/ 폴더로 이동
3. 신규 DISPATCH 파일 생성 (S{N}-R{N}-{team}.md)
4. 이 표의 해당 팀 행 업데이트
5. git add .moai/dispatches/ && git commit && git push origin main
```
