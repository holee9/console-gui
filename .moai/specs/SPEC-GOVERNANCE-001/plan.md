# SPEC-GOVERNANCE-001 구현 계획

## 기술 스택
- Git CLI (설정 변경)
- PowerShell / Bash (파일 이동, 커밋 자동화)
- C# / .NET 9 (GlobalSuppressions.cs)
- Markdown (규칙 문서 업데이트)

---

## 태스크 분해 (실행 순서)

### Phase 1: 즉각 교정 (당일 실행, 소실 방지 최우선)

#### P1-T1: 한글 인코딩 설정 수정 [15분]
- `git config core.quotepath false` 적용 (로컬 저장소)
- `.gitattributes` 생성:
  ```
  * text=auto
  *.cs   text eol=crlf encoding=utf-8
  *.md   text eol=lf   encoding=utf-8
  *.yaml text eol=lf   encoding=utf-8
  *.xml  text eol=crlf encoding=utf-8
  *.xaml text eol=crlf encoding=utf-8
  ```

#### P1-T2: 팀 브랜치 미커밋 작업 commit [30분]
순서 (의존성 기준):
1. `team/qa` — CI 설정, 스크립트 (다른 팀에 영향)
2. `team/team-b` — GlobalSuppressions.cs 8개 + 소스 수정 (19 dirty)
3. `team/team-a` — NuGet 업그레이드 + 마이그레이션 (53 dirty)
4. `team/coordinator` — App.xaml.cs, ViewModel 수정 (19 dirty)
5. `team/ra` — 규제 문서 업데이트
6. `team/team-design` — 이미 +2 커밋 완료

커밋 메시지 형식 (language.yaml: git_commit_messages=ko):
```
feat(team-b): 04-08 dispatch 완료 — GlobalSuppressions + SCS0005 억제 적용
```

#### P1-T3: Team A GlobalSuppressions.cs 추가 [20분]
Team B 패턴을 Team A 5개 모듈에 적용:
- `src/HnVue.Common/GlobalSuppressions.cs`
- `src/HnVue.Security/GlobalSuppressions.cs`
- `src/HnVue.SystemAdmin/GlobalSuppressions.cs`
- `src/HnVue.Data/GlobalSuppressions.cs`
- `src/HnVue.Update/GlobalSuppressions.cs`

억제 대상: SA1101, SA1309, SA1200, SA1600 (Team B 패턴 참조)

#### P1-T4: Dispatch 수명주기 정리 [10분]
- `.moai/dispatches/active/`의 04-08 파일 5개 → `completed/`로 이동
- `.moai/dispatches/active/`에 04-09 파일 5개 유지 (PENDING 상태)

#### P1-T5: QA 교차 리뷰 보고서 파일 생성 [15분]
- 이전 에이전트 콘솔 출력 내용을 기반으로
- `TestReports/QA_CROSS_REVIEW_REPORT_2026-04-09.md` 생성

---

### Phase 2: 거버넌스 규칙 문서화 [당일]

#### P2-T1: dispatch-system.md 커밋 의무 규칙 추가
추가할 내용:
```markdown
## Commit Discipline (HARD)

- [HARD] Before updating dispatch status to COMPLETE, the team agent MUST have created at least 1 git commit on the team branch
- [HARD] Commit message MUST reference the dispatch: `feat(TEAM): DISPATCH-{TEAM}-{DATE} — {task summary}`
- [HARD] "COMPLETE" without a verifiable commit = VIOLATION (report to main as COMMIT_MISSING)
```

#### P2-T2: worktree-integration.md 브랜치 계층 명시
추가할 섹션:
```markdown
## Branch Hierarchy (Commander Center)

main (사용자-MoAI 커맨더센터) ← 직접 구현 금지
  └─ team/coordinator (통합 게이트)
       ├─ team/team-a (Infrastructure & Foundation)
       └─ team/team-b (Medical Imaging Pipeline)
  └─ team/qa (독립 품질 게이트)
  └─ team/ra (독립 규제 팀)
  └─ team/team-design (독립 UI 팀)

[HARD] team-a, team-b는 coordinator 리뷰 없이 main에 직접 머지 금지
[HARD] main은 사용자 지시 외 직접 구현 작업 금지
```

#### P2-T3: 이슈-선행 규칙 dispatch-system.md에 추가
```markdown
## Issue-First Policy (HARD)

- [HARD] Main이 dispatch를 발행하기 전에 해당 작업에 대한 Gitea 이슈를 생성해야 한다
- [HARD] dispatch 문서 헤더에 `Issue: #{number}` 필드 추가 필수
- 이슈 레이블: 팀별 레이블 + `dispatch` 레이블
```

---

### Phase 3: 릴리즈 블로커 이슈 생성 [Gitea]

#### P3-T1: R-001 — PatientEntity 암호화
```
제목: [SECURITY][릴리즈 블로커] PatientEntity Name/DoB/CreatedBy AES-256-GCM 암호화 미구현
레이블: security, priority-critical, release-blocker
SWR: SWR-CS-080
```

#### P3-T2: R-002 — Null 레포지토리 6개
```
제목: [INFRA] App.xaml.cs Null Repository 스텁 6개 — EF Core 실구현 전환 계획 수립
레이블: team-a, priority-high
대상: IDoseRepository, IWorklistRepository, IIncidentRepository, IUpdateRepository, ISystemSettingsRepository, IStudyRepository
```

---

## 위험 요소

| 위험 | 확률 | 영향 | 대응 |
|------|------|------|------|
| Team A 커밋 시 53개 파일 빌드 오류 | 중 | 높음 | 커밋 전 `dotnet build` 실행 후 커밋 |
| GlobalSuppressions 추가 후 기존 경고 노출 | 낮 | 중간 | 빌드 경고 0건 확인 후 커밋 |
| .gitattributes line ending 충돌 | 낮 | 중간 | `git add --renormalize .` 후 변경사항 검토 |
| core.quotepath 변경 후 기존 스크립트 영향 | 낮 | 낮음 | 표시만 변경, 실제 파일명 불변 |

---

## MX 태그 계획

- `dispatch-system.md` 업데이트: `@MX:NOTE` — 커밋 의무 규칙 섹션
- `worktree-integration.md` 업데이트: `@MX:ANCHOR` — 브랜치 계층 구조
- `GlobalSuppressions.cs` 신규: `@MX:NOTE` — 억제 정당성

---

## 완료 기준

- [ ] `core.quotepath false` 설정 완료
- [ ] `.gitattributes` 생성 및 커밋
- [ ] 5개 팀 브랜치 모두 신규 커밋 존재
- [ ] Team A GlobalSuppressions.cs 5개 파일 추가
- [ ] 04-08 dispatch 5개 → completed/ 이동
- [ ] dispatch-system.md 커밋 의무 규칙 추가
- [ ] worktree-integration.md 브랜치 계층 명시
- [ ] R-001, R-002 Gitea 이슈 생성
- [ ] QA 교차 리뷰 보고서 파일 생성
- [ ] 전체 빌드 PASS 확인
