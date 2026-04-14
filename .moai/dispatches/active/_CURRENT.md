# DISPATCH Current Index

> **[HARD] 에이전트 FIRST ACTION**: 이 파일을 가장 먼저 읽는다.
> 자신의 팀 행(row)에서 파일명을 확인한 뒤, 해당 파일만 읽는다.
> 상태가 `PR_OPEN` 또는 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고.

Updated: 2026-04-14 (S09-R1 — 4/6 MERGED, Team A+QA 대기)
DISPATCH 절대 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`

---

## 현재 팀별 DISPATCH 상태

| 팀 | 현재 DISPATCH 파일 | 상태 | 비고 |
|----|-------------------|------|------|
| **Coordinator** | - | **MERGED** | Detector DI 통합테스트 완료 |
| **QA** | `S09-R1-qa.md` | **ACTIVE** | 전팀 COMPLETED 후 품질게이트 |
| **RA** | - | **MERGED** | SBOM/SOUP 이미 등록 확인 |
| **Design** | - | **MERGED** | 하드코딩 색상 토큰 교체 완료 |
| **Team A** | `S09-R1-team-a.md` | **ACTIVE** | IDLE CONFIRM (push 대기) |
| **Team B** | - | **MERGED** | IDLE CONFIRM |

**→ S09-R1: 4/6 MERGED — Team A IDLE CONFIRM + QA 품질게이트 대기**

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
| 2026-04-14 | **S09 R1** | **S09-R1-*.md** | **4/6 MERGED (Design,Coordinator,RA,TeamB)** |

---

## Commander Center 전용 — 신규 DISPATCH 발행 절차

```
1. 기존 ACTIVE 파일 상태를 MERGED 또는 SUPERSEDED로 변경
2. completed/ 폴더로 이동
3. 신규 DISPATCH 파일 생성 (S{N}-R{N}-{team}.md)
4. 이 표의 해당 팀 행 업데이트
5. git add .moai/dispatches/ && git commit && git push origin main
```
