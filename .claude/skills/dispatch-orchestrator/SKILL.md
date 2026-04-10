---
name: dispatch-orchestrator
description: >
  Dispatch 통합 오케스트레이터. 전체 worktree 팀의 DISPATCH.md 결과를 수집,
  미완료 작업을 실행하고, 통합 빌드 검증 및 보고서를 생성한다.
  '디스패치 통합', '디스패치 보고', '디스패치 검증', 'dispatch integration',
  'dispatch report' 요청 시 사용. 재실행, 업데이트, 보완 요청 시에도 트리거.
user-invocable: true
metadata:
  version: "2.0.0"
  category: "workflow"
  status: "active"
  updated: "2026-04-10"
---

# Dispatch Orchestrator — Worktree Integration Pipeline

전체 worktree 팀의 DISPATCH 결과를 수집하고 미완료 작업을 처리하는 파이프라인 오케스트레이터.

## Architecture

**Pattern**: Fan-out/Fan-in + Distributed Verification
**Mode**: Sub-agent (각 worktree가 독립적, 팀 통신 불필요)

```
Phase 0: Context Check (재실행 판별)
Phase 1: Dispatch Status Collection (6팀 DISPATCH.md 수집 + 분류)
Phase 2: Validation (빌드 증거 검증 + PR 상태 확인)
Phase 3: Pending Task Execution (Fan-out, parallel sub-agents)
Phase 4: Integration Report (결과 취합 + 머지 순서 결정)
Phase 5: Merge Execution (사용자 승인 후 순차 머지)
```

## References

- **DISPATCH Format**: `${CLAUDE_SKILL_DIR}/dispatch-schema.md` (DISPATCH.md 필드 정의 및 검증 규칙)
- **Operational Rules**: `DEV-OPS-GUIDELINES.md` (빌드/배포 운영 규칙)

## Workflow

### Phase 0: Context Check

1. `_workspace/dispatch-integration/` 존재 확인
   - 존재 + 부분 수정 요청 -> 해당 Phase만 재실행
   - 미존재 -> 초기 실행, 디렉토리 생성

### Phase 1: Dispatch Status Collection

1. 모든 6개 worktree의 DISPATCH.md 읽기 (QA, RA, Design, Team A, Team B, Coordinator)
2. 각 팀 상태를 5단계로 분류:

| Status | Definition |
|--------|-----------|
| COMPLETED (with build evidence) | 작업 완료 + Build Evidence 필드에 빌드/테스트 결과 존재 |
| COMPLETED (no evidence) | 작업 완료 표기이나 Build Evidence 필드 누락 -> INVALID, IN_PROGRESS로 재분류 |
| IN_PROGRESS | 작업 진행 중 |
| PENDING | 아직 시작되지 않은 작업 |
| BLOCKED | 외부 의존성으로 차단된 작업 |

3. Git 상태 확인:
   - 각 worktree 브랜치가 remote에 push되었는지 확인
   - PR이 생성되었는지 확인
   - PR이 mergeable 상태인지 확인

### Phase 2: Validation

Phase 1에서 COMPLETED로 분류된 팀에 대해 검증 수행:

1. **Build Evidence 검증**
   - DISPATCH.md의 Build Evidence 필드 존재 여부 확인
   - 빌드 성공/실패 기록, 테스트 통과 수 등 확인
   - Build Evidence가 없는 COMPLETED는 INVALID 처리

2. **PR 상태 검증** (Gitea API 활용)
   - `GET /api/v1/repos/{owner}/{repo}/pulls?state=open` 로 오픈 PR 목록 조회
   - 각 팀 브랜치의 PR 존재 여부 및 mergeable 상태 확인
   - 충돌 발생 시 해당 팀을 BLOCKED으로 재분류

3. **Validation Report 생성**
   - 팀별 검증 결과 요약
   - INVALID 항목 목록
   - 후속 조치 필요 팀 식별

### Phase 3: Pending Task Execution (Fan-out)

Phase 1-2에서 미완료로 판정된 팀에 대해 병렬 sub-agent 실행:

- 각 미완료 worktree에 대해 독립적인 sub-agent를 Fan-out으로 실행
- sub-agent는 해당 worktree의 DISPATCH.md 지시에 따라 잔여 작업 수행
- 빌드 검증: `dotnet build HnVue.sln --configuration Release` 실행
- 테스트 실행: `dotnet test --configuration Release --no-build`
- run_in_background: true (병렬 실행)

### Phase 4: Integration Report

모든 Phase 2-3 완료 후:

1. 전체 DISPATCH.md 상태 재수집
2. **머지 순서 결정** (의존성 기반):
   ```
   QA/RA -> Design -> Team A -> Team B -> Coordinator
   ```
   - QA/RA: 문서/설정 변경으로 코드 충돌 최소
   - Design: UI 전용, 비즈니스 로직 무관
   - Team A: Infrastructure 기반 (Team B 의존 가능)
   - Team B: Medical Pipeline (Team A 인프라 의존)
   - Coordinator: 통합 레이어 (모든 팀 결과 의존)

3. 통합 보고서 생성: `TestReports/DISPATCH_INTEGRATION_REPORT_{date}.md`
   - 팀별 최종 상태
   - 빌드/테스트 결과 요약
   - 머지 순서 및 예상 충돌
   - 후속 작업 권고

### Phase 5: Merge Execution

**사용자 승인 필요** — Phase 4 보고서 제출 후 승인 대기.

승인 후 순차 머지 실행:

1. 머지 순서에 따라 각 팀 브랜치를 main에 머지
2. 각 머지 후:
   - 나머지 미머지 브랜치를 main 기준으로 rebase
   - 충돌 발생 시 즉시 중단 + 사용자에게 보고
3. 전체 머지 완료 후:
   - `git pull origin main` 으로 최신 상태 동기화
   - `dotnet build HnVue.sln --configuration Release` 로 최종 빌드 검증
   - clean state 확인 (`git status` clean)

## Data Flow

- Phase 1 -> Phase 2: 팀별 상태 분류 결과 (메모리)
- Phase 2 -> Phase 3: 미완료 작업 목록 + 검증 결과 (메모리)
- Phase 3 -> Phase 4: sub-agent 반환값 (빌드/테스트 결과, 작업 결과)
- Phase 4 -> Phase 5: 머지 순서 + 통합 보고서 (파일)
- Phase 5 -> Output: 최종 main 상태 검증 결과

## Build Command Fallbacks

빌드 명령어 우선순위:

1. **Primary**: `dotnet build HnVue.sln --configuration Release`
2. **Secondary** (dotnet CLI 미설치 시): `"D:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" HnVue.sln /p:Configuration=Release`

테스트 명령어:
1. **Primary**: `dotnet test --configuration Release --no-build`
2. **Secondary**: MSBuild + vstest.console.exe

## Error Handling

| Error Condition | Response |
|----------------|----------|
| Gitea API 접근 불가 | Git CLI 기반 브랜치/리모트 상태 확인으로 대체, PR 상태는 UNKNOWN 처리 |
| dotnet CLI not in PATH | MSBuild 직접 경로 사용 (Secondary 빌드 명령) |
| 머지 충돌 발생 | 즉시 머지 중단, 충돌 파일 목록 + 양쪽 변경사항 보고, 사용자 개입 요청 |
| 문서 파일 미존재 | 스킵 + 보고서에 명시 |
| DISPATCH.md 미존재 (worktree) | 해당 팀 PENDING 처리 + 보고서에 명시 |
| sub-agent 실패 | 1회 재시도, 재실패 시 해당 결과 없이 진행 (보고서에 누락 명시) |
| rebase 충돌 | rebase 중단 (`git rebase --abort`), 사용자에게 수동 해결 요청 |

## Test Scenarios

- 정상: 모든 worktree COMPLETE (with evidence), 전체 빌드 PASS -> 보고서 생성 + 머지 순서 제안
- 부분 완료: 3팀 COMPLETE, 2팀 IN_PROGRESS, 1팀 PENDING -> 미완료 팀 Fan-out 실행
- 검증 실패: COMPLETE 표기이나 Build Evidence 없음 -> INVALID 재분류 + 재실행
- 빌드 실패: Team B 빌드 실패 -> 에러 캡처 + 보고서에 FAIL 기록 + 후속 조치 권고
- 머지 충돌: Team B 머지 시 충돌 -> 즉시 중단 + 충돌 내역 보고
