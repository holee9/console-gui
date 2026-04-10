# SPEC-GOVERNANCE-001 인수 기준

## 시나리오 1: Dispatch 커밋 의무 검증

**Given** team/team-b 워크트리에 미커밋 변경사항이 존재하고  
**When** P1-T2 (팀 브랜치 커밋) 태스크가 완료되면  
**Then** `git log main..team/team-b --oneline` 결과가 1개 이상의 커밋을 반환해야 한다  
**And** 커밋 메시지가 `feat(team-b): 04-08 dispatch 완료` 형식을 포함해야 한다  
**And** `git -C .worktrees/team-b status --short` 결과가 0줄이어야 한다

---

## 시나리오 2: 한글 인코딩 표시 정합성

**Given** 프로젝트에 한글 파일명이 포함되어 있고  
**When** `git status` 또는 `git log --name-only` 명령을 실행하면  
**Then** 파일명이 `\353\263\200\352\262\275` 형태의 8진수가 아닌 `변경` 등 한글로 표시되어야 한다  
**And** `git config --get core.quotepath` 결과가 `false`여야 한다

---

## 시나리오 3: Team A SA* 억제 적용

**Given** Team A 5개 모듈에 GlobalSuppressions.cs가 추가되고  
**When** `dotnet build HnVue.sln --configuration Release` 실행 시  
**Then** SA* 코드 경고가 Team A 모듈에서 0건이어야 한다 (또는 억제 적용 확인)  
**And** 빌드가 0 errors로 완료되어야 한다

---

## 시나리오 4: Dispatch 수명주기 완전 이행

**Given** `.moai/dispatches/active/`에 04-08 dispatch 파일 5개가 존재하고  
**When** P1-T4 (dispatch 정리) 태스크가 완료되면  
**Then** `.moai/dispatches/completed/`에 04-08 파일 5개가 존재해야 한다  
**And** `.moai/dispatches/active/`에는 04-09 파일 5개만 남아야 한다

---

## 시나리오 5: 규칙 문서 업데이트

**Given** dispatch-system.md와 worktree-integration.md가 업데이트되고  
**When** 문서를 읽으면  
**Then** dispatch-system.md에 `[HARD]` 커밋 의무 규칙이 존재해야 한다  
**And** worktree-integration.md에 브랜치 계층 다이어그램이 존재해야 한다  
**And** dispatch-system.md에 `Issue-First Policy` 섹션이 존재해야 한다

---

## 시나리오 6: 릴리즈 블로커 이슈 등록

**Given** R-001 (PatientEntity 암호화)과 R-002 (Null 레포지토리)가 미해결이고  
**When** Phase 3 이슈 생성이 완료되면  
**Then** Gitea에 `priority-critical` + `security` 레이블의 R-001 이슈가 존재해야 한다  
**And** Gitea에 `priority-high` + `team-a` 레이블의 R-002 이슈가 존재해야 한다

---

## 경계 조건

- **Team A 커밋 중 빌드 실패 시**: 빌드 오류 수정 후 재커밋 (작업 진행)
- **.gitattributes 적용 후 line ending 변경 파일 다수 발생 시**: `git add --renormalize .` 실행하되 바이너리 파일(.pptx) 제외
- **Gitea API 오류 시**: 이슈 수동 생성 후 번호를 SPEC에 기록

---

## 게이트 기준

| 게이트 | 기준 | 측정 방법 |
|--------|------|-----------|
| 커밋 완전성 | 5개 팀 브랜치 모두 신규 커밋 | `git log main..team/* --oneline` |
| 빌드 안정성 | 전체 솔루션 0 errors | `dotnet build --configuration Release` |
| 인코딩 정합성 | git 출력에서 한글 정상 표시 | `git config --get core.quotepath = false` |
| 문서 완전성 | 3개 규칙 문서 업데이트 완료 | 파일 존재 + 내용 확인 |
| 이슈 추적 | R-001, R-002 이슈 생성 | Gitea API 또는 gh CLI |
