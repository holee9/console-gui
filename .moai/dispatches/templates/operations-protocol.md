# Commander Center — Worktree Team Operations Protocol

## 1. 작업 주기 (DISPATCH → EXECUTE → REPORT → REVIEW → MERGE)

```
┌─────────────┐    ┌──────────────┐    ┌──────────────┐    ┌───────────┐    ┌──────┐
│ 1. DISPATCH  │───>│ 2. EXECUTE   │───>│ 3. REPORT    │───>│ 4. REVIEW │───>│ 5.   │
│ 발행+커밋    │    │ 팀 자율 실행  │    │ 완료보고 제출 │    │ 커맨더 검증│    │ MERGE│
└─────────────┘    └──────────────┘    └──────────────┘    └───────────┘    └──────┘
     CC                 TEAM                 TEAM               CC            CC
```

## 2. Phase 1: DISPATCH 발행 (커맨더센터)

### 발행 전 체크리스트
- [ ] DISPATCH 내용이 SPEC 수용기준과 정합한지 확인
- [ ] 팀간 의존성(선행/후행)이 명시되어 있는지 확인
- [ ] 수용 기준이 측정 가능한지 확인

### 발행 절차 [HARD]

1. **DISPATCH 파일 작성**: `.moai/dispatches/active/S{NN}-R{N}-{team}.md`
2. **main에 커밋**: DISPATCH 파일을 반드시 main 브랜치에 커밋
3. **팀 워크트리에서 pull**: 각 팀이 `git pull origin main`으로 DISPATCH 획득
4. **팀 에이전트 실행**: DISPATCH 파일을 읽고 작업 시작

```bash
# 발행 절차 (커맨더센터)
git add .moai/dispatches/active/S{NN}-R{N}-{team}.md
git commit -m "dispatch({team}): S{NN} R{N} {작업요약}"
git push origin main

# 팀 워크트리에서
git pull origin main
```

### [HARD] DISPATCH는 반드시 main에 커밋 후 팀 실행
- 커밋되지 않은 DISPATCH는 워크트리에서 접근 불가
- 에이전트 프롬프트로 전달하는 것은 보조 수단일 뿐, 파일 기반이 원칙

## 3. Phase 2: EXECUTE (팀 워크트리)

### 팀 실행 규칙

- DISPATCH에 명시된 Task 순서대로 실행
- 각 Task 완료 시 DISPATCH.md Status 섹션 업데이트
- **빌드 증거 필수**: `dotnet build` 결과를 DISPATCH.md에 기록
- **테스트 결과 필수**: `dotnet test` 결과를 DISPATCH.md에 기록

### [HARD] 팀은 PR을 생성하지 않는다
- PR 생성은 커맨더센터 전권
- 팀은 commit + push까지만 수행
- PR 없이 브랜치에 push된 상태로 대기

```bash
# 팀 완료 절차 (팀 에이전트)
git add {변경파일들}
git commit -m "{type}({team}): {변경요약}"
git push origin team/{team}
# PR 생성하지 않음 — 커맨더센터가 검증 후 생성
```

## 3.5 Completion Action (필수 — 작업 완료 즉시 실행)

### 문제 배경
S04 R1에서 팀이 작업을 완료한 후 commit/push/보고 없이 대기하는 현상 발생.
에이전트가 "작업 끝"을 인식해도 다음 액션이 정의되지 않아 방치됨.

### [HARD] 작업 완료 후 반드시 순서대로 실행

```
모든 Task 완료 (또는 BLOCKED 발생)
    ↓
1. DISPATCH.md Status 섹션 업데이트
   - State: COMPLETED / PARTIAL / BLOCKED
   - Build Evidence: dotnet build 결과
   - Test Evidence: dotnet test 결과
   - Coverage Evidence: 모듈별 커버리지
   - New/Modified Files: 파일 목록
   - Issues: 발견 문제 (없으면 "None")
    ↓
2. git add 모든 변경 파일 + DISPATCH.md
    ↓
3. git commit -m "feat({team}): S{NN} R{N} {요약}"
    ↓
4. git push origin team/{team}
    ↓
5. 종료 (PR 생성 금지, 커맨더센터 통지 불필요)
   → 커맨더센터가 모니터링으로 감지
```

### [HARD] 중요: Task 단위 commit 허용, 최종 commit은 Status 포함
- 각 Task 완료 시마다 commit 가능 (중간 저장)
- **마지막 commit에는 반드시 DISPATCH.md Status 업데이트 포함**
- Status가 업데이트되지 않은 채 push하면 미완료로 간주

### [HARD] BLOCKED 시에도 보고 필수
- 선행 팀 미완료, 빌드 불가 등으로 막힌 경우
- State: BLOCKED, Blocked By: {이유} 기록 후 commit+push
- 커맨더센터가 BLOCKED 보고를 받아 의존성 해결

## 4. Phase 3: REPORT (팀 → 커맨더센터)

### 보고는 DISPATCH.md Status로 대체
Phase 3.5 Completion Action에서 DISPATCH.md에 기록한 내용이 곧 보고서.
별도 보고서 파일이나 메시지 전송 불필요. 커맨더센터가 모니터링으로 감지.

```markdown
## Status

- **State**: COMPLETED | PARTIAL | BLOCKED
- **Completed Tasks**: T0, T1, T2 (수용기준 충족 항목 나열)
- **Build Evidence**:
  - dotnet build: {에러수} 에러
  - dotnet test: {통과수} passed / {실패수} failed
  - 커버리지: {모듈} {퍼센트}%
- **New Files**: {신규 파일 목록}
- **Modified Files**: {수정 파일 목록}
- **Issues**: {발견된 문제, 없으면 "None"}
- **Blocked By**: {없으면 "None", 있으면 팀/이유}
```

### [HARD] 보고서는 DISPATCH.md Status 섹션에 기록
- 별도 보고서 파일 생성하지 않음
- DISPATCH.md 자체가 보고서
- State가 COMPLETED/PARTIAL/BLOCKED 중 하나여야 함

## 5. Phase 4: REVIEW (커맨더센터)

### 검증 체크리스트

커맨더센터는 팀의 DISPATCH.md Status를 확인 후 다음을 검증:

1. **수용 기준 충족**: DISPATCH에 명시된 각 Task의 수용 기준이 모두 충족되었는지
2. **빌드 증거**: 0 에러 확인
3. **테스트 결과**: 0 실패 확인 (flaky 제외)
4. **커버리지**: 목표치 달성 확인
5. **파일 범위**: DISPATCH 파일 소유권 범위 내 수정인지
6. **PPT 준수**: Design 팀의 경우 지정 페이지 외 구현 없는지

### 검증 결과에 따른 액션

| 결과 | 액션 |
|------|------|
| **PASS** | Phase 5(MERGE) 진행 |
| **CONDITIONAL** | 추가 가이드 DISPATCH 발행 (R{N+1}) |
| **FAIL** | 수정 지시 DISPATCH 발행 (R{N+1}) |
| **BLOCKED** | 선행 팀 완료 후 재실행 지시 |

## 6. Phase 5: MERGE (커맨더센터)

### PR 생성 절차

검증 PASS 시에만 커맨더센터가 PR 생성:

```bash
# 커맨더센터 전권
curl -X POST "{gitea_api}/repos/{repo}/pulls" \
  -H "Authorization: token {token}" \
  -H "Content-Type: application/json" \
  -d @pr-body.json
```

### [HARD] PR은 커맨더센터만 생성
- 팀의 자가 PR은 원칙적으로 거부
- 단, 기존 열린 PR이 있으면 재사용

## 7. 모니터링 규격

### 커맨더센터 모니터링 항목

| 항목 | 방법 | 주기 |
|------|------|------|
| 신규 커밋 | `git log {branch} --oneline -1` | 5분 |
| **Uncommitted 소스 변경** | `git diff --name-only HEAD` (워크트리) | **5분** |
| **Uncommitted DISPATCH.md** | `git diff HEAD -- DISPATCH.md` (워크트리) | **5분** |
| **신규 보고서 파일** | `git status --porcelain \| grep "^??" \| grep -i report` (워크트리) | **5분** |
| DISPATCH 상태 | DISPATCH.md Status 섹션 확인 (uncommitted 포함) | 5분 |
| 빌드/테스트 | `dotnet build` + `dotnet test` | 15분 |
| PR 상태 | Gitea API 조회 | 10분 |

### [HARD] 모니터링은 commit뿐 아니라 uncommitted 변화도 검사

S04 R1 교훈: 팀이 작업 완료 후 DISPATCH.md 업데이트 + 보고서 파일 생성까지 했으나
**commit하지 않아** git log 기반 모니터링에서 감지 불가.

모니터링 체크 시 반드시 다음을 병렬 확인:
1. `git log {branch} --oneline -1` — 신규 커밋
2. `git diff --name-only HEAD` — uncommitted 소스/문서 변경
3. `git status --porcelain | grep "^??"` — 신규 파일(보고서 등)
4. DISPATCH.md Status 섹션 — uncommitted 상태 포함 읽기

### 이상 감지 기준

| 상황 | 판정 | 액션 |
|------|------|------|
| DISPATCH.md에 COMPLETED/PARTIAL/BLOCKED + uncommitted | **DONE_UNCOMMITTED** | 팀에 commit+push 지시 |
| 보고서 파일 존재 + uncommitted | **REPORT_UNCOMMITTED** | 팀에 commit+push 지시 |
| 소스 변경 존재 + uncommitted | **WORK_IN_PROGRESS** | 정상, 계속 모니터링 |
| 20분+ 무변화 (commit+uncommitted 모두) | **STALLED** | 팀 상태 확인, 재실행 검토 |
| DISPATCH State 미업데이트 | **ORPHANED** | 에이전트 세션 종료 가능성, 재실행 |
| 수용 기준 미충족 COMPLETED | **FALSE_REPORT** | 추가 검증, 수정 지시 |

## 8. DISPATCH 템플릿 (개정)

모든 DISPATCH는 다음 구조를 따른다:

```markdown
# DISPATCH: {Team} — S{NN} Round {N}

| 항목 | 내용 |
|------|------|
| 발행일 | YYYY-MM-DD |
| 발행자 | Main (MoAI Commander Center) |
| 대상 | {Team} |
| 브랜치 | team/{team} |
| 유형 | S{NN} Round {N} — {작업요약} |
| 선행조건 | {없음 또는 Team X 완료} |
| SPEC 참조 | SPEC-XXX |
| Gitea API | {url} |

## 실행 방법
1. git pull origin main (DISPATCH 획득)
2. 본 문서 읽고 Task 순서대로 실행
3. 완료 후 Status 섹션 업데이트
4. commit + push (PR 생성하지 않음)

## 컨텍스트
{작업 배경 설명}

## 파일 소유권
{수정 가능한 파일 범위}

## Task {N} ({우선순위}): {작업명}
### 사전 확인
{실행 전 체크 명령}
### 구현/수정 내용
{상세 작업 내용}
### 수용 기준
- [ ] {측정 가능한 기준 1}
- [ ] {측정 가능한 기준 2}

## 빌드 검증
{dotnet build + dotnet test 명령}

## Git 완료 프로토콜
git add → git commit → git push (PR 생성 금지)

## Status (작업 후 업데이트)
- **State**: NOT_STARTED
- **Completed Tasks**: --
- **Build Evidence**: --
- **Issues**: --
```

---

Version: 1.0.0
Classification: WORKFLOW PROTOCOL
Last Updated: 2026-04-11
