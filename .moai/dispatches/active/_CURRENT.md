| **Design** | - | **IDLE** | S11-R2 완료 ✅ |# DISPATCH Current Index
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |> **[HARD] 에이전트 FIRST ACTION**: 이 파일을 가장 먼저 읽는다.
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |> 자신의 팀 행(row)에서 파일명을 확인한 뒤, 해당 파일만 읽는다.
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |> 상태가 `PR_OPEN` 또는 `IDLE`이면 → 새 DISPATCH 없음 → Commander Center에 IDLE 보고.
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |---
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |## [HARD] 자율주행 철학 — 모든 팀 필독
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |**자율주행 = "마음대로"가 아닙니다. 명확한 룰 내에서 자율적으로 실행하는 것입니다.**
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |### [HARD] 세션 시작 절차 (모든 팀 필수)
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |```
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |Step 0: git pull origin main  ← [HARD] 이 파일 읽기 전 반드시 실행
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |Step 1: Read _CURRENT.md (이 파일)
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |Step 2: 자신의 팀 행(row)에서 파일명 확인
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |Step 3: 해당 파일만 읽기 (다른 팀 DISPATCH 절대 읽기 금지)
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |Step 4: 상태가 IDLE이면 → 즉시 IDLE 보고 (다른 작업 금지)
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |```
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |### [HARD] IDLE 상태 절대 규칙
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |```
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |_CURRENT.md에서 자신의 팀이 IDLE이면:
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |1. 즉시 IDLE 보고
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |2. DISPATCH 파일 검색 금지
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |3. 자율 작업 금지
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |4. CC 지시 대기
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |```
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |**위반 시 프로토콜 위반으로 간주합니다.**
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |---
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |Updated: 2026-04-17 (S11-R2: 6/6 ACTIVE — S11-R2 시작)
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |DISPATCH 절대 경로: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |---
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |## [HARD] CC 모니터링 주기 (S11-R1부터 적용)
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |- **CC 모니터링**: 20분 간격 (git log + DISPATCH Status 확인)
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |- **팀 동기화**: 15분 간격 (DISPATCH Status 주기적 업데이트)
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |- **자율주행 진화**: S11 데이터 축적 → S12 최적화 적용
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |**모니터링 절차**:
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |1. `git fetch origin`
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |2. `git log --oneline origin/team/* --not main` (6팀)
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |3. DISPATCH 파일 Status 테이블 확인
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |4. COMPLETED → 소유권 검증 → 머지 → _CURRENT.md 업데이트
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |5. 20분 후 다시 모니터링 (루프)
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |---
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |---
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |## 현재 팀별 DISPATCH 상태
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 팀 | 현재 DISPATCH 파일 | 상태 | 비고 |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ ||----|-------------------|------|------|
| **Team A** | - | **MERGED** | EfUpdateRepository 커버리지 개선 ✅ |
| **Team B** | - | **MERGED** | Dicom C-STORE 에러 처리 개선 ✅ |
| **Coordinator** | - | **MERGED** | ISettingsViewModel + DI 등록 검증 ✅ |
| **Design** | - | **MERGED** | AcquisitionView 디자인 (슬라이드 9-11) ✅ |
| **QA** | - | **MERGED** | 전체 테스트 실행 + 커버리지 리포트 ✅ |
| **RA** | - | **MERGED** | CHANGELOG + SBOM v3.1 업데이트 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |**→ S11-R2: 6/6 MERGED ✅**
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |---
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |## 중요 규칙 변경 (S08부터 적용)
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |```
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |[HARD] role-matrix.md (CONSTITUTIONAL): 7팀 역할 경계 최상위 규약 도입
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |- CC 자가점검 4문: dotnet? 소스수정? 구현에이전트? 범위외? → YES = 즉시중단
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |- CC 허용: DISPATCH 기획, 모니터링, 머지, 취합 ONLY
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |- CC 금지: dotnet build/test, 소스코드 수정, 구현 에이전트 호출
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |- QA 독립성: PASS/FAIL 판정 최종권, CC 뒤집기 불가
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |```
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |---
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |## 실행 전 필수: 브랜치 최신화
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |```bash
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |# 각 팀은 작업 시작 전 반드시 실행
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |git checkout team/{your-team}
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |git pull origin main
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |git push origin team/{your-team}
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |```
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |---
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |## DISPATCH 라운드 이력
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 날짜 | 라운드 | 파일 | 상태 |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ ||------|--------|------|------|
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-08 | Phase 0 | DISPATCH-*-2026-04-08.md | `completed/` 아카이브 |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-09 | S03 QA Coverage | DISPATCH-*-2026-04-09.md | `completed/` 아카이브 |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-11 | S04 R1+R2 | S04-R{1,2}-*.md | `completed/` 아카이브 (PR #77-82) |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-12 | S05 R1 | S05-R1-*.md | `completed/` 아카이브 |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-13 | S05 R2 | S05-R2-*.md | ALL MERGED |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-13 | S06 R1 | S06-R1-*.md | ALL MERGED (PR #88-90) |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-13 | S06 R2 | S06-R2-*.md | ALL MERGED |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-14 | S07 R1~R5 | S07-R{1~5}-*.md | ALL MERGED (Sprint S07 종료) |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-14 | S08 R1 | S08-R1-*.md | ALL MERGED (PASS) |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-14 | S08 R2 | S08-R2-*.md | ALL MERGED (PASS) |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-14 | S09 R1 | S09-R1-*.md | 전팀 MERGED 완료 |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-14 | **S09 R2** | **S09-R2-*.md** | 전팀 MERGED, QA CONDITIONAL PASS |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-15 | **S09 R3** | **S09-R3-*.md** | 전팀 MERGED, QA PASS (#104) |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-15 | **S10 R1** | **S10-R1-*.md** | 6팀 발행 (갭 분석 기반) |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-15 | **S10 R2** | **S10-R2-*.md** | 3팀 MERGED, 3팀 IDLE |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-15 | **S10 R3** | **S10-R3-*.md** | 전팀 MERGED (Coordinator 92.58%, QA CONDITIONAL PASS, Design MergeView) |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-16 | **S10 R4** | **S10-R4-*.md** | 6/6 MERGED, QA CONDITIONAL PASS (81.3%) |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-16 | **S11 R1** | **S11-R1-*.md** | 6팀 발행 (갭 분석 기반, PASS 전환 목표) |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ || 2026-04-17 | **S11 R2** | **S11-R2-*.md** | 6팀 발행 (갭 분석 기반 실질 작업 할당) |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |---
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |## Commander Center 전용 — 신규 DISPATCH 발행 절차
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |```
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |1. 기존 ACTIVE 파일 상태를 MERGED 또는 SUPERSEDED로 변경
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |2. completed/ 폴더로 이동
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |3. 신규 DISPATCH 파일 생성 (S{N}-R{N}-{team}.md)
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |4. 이 표의 해당 팀 행 업데이트
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |5. git add .moai/dispatches/ && git commit && git push origin main
| **Design** | - | **IDLE** | S11-R2 완료 ✅ |```
