# SPEC-GOVERNANCE-001 (Compact)

## 요구사항

- REQ-GOV-001: WHEN dispatch 완료 → SHALL 팀 브랜치 git commit 존재 후 COMPLETE 표시
- REQ-GOV-002: WHILE 팀 개발 활성화 → SHALL main(커맨더센터)·coordinator(통합게이트)·team-a/b(워커) 계층 강제
- REQ-GOV-003: WHEN team-a/b 완료·main 머지 요청 → SHALL coordinator 명시적 리뷰·승인 필수
- REQ-GOV-004: WHEN 새 dispatch 발행 → SHALL Gitea 이슈 생성 + 이슈번호 dispatch 기록 先행
- REQ-GOV-005: THE SYSTEM SHALL core.quotepath=false + .gitattributes UTF-8 설정 유지
- REQ-GOV-006: WHEN QA 보고서 생성 → SHALL TestReports/{TYPE}_{DATE}.md 파일 저장
- REQ-GOV-007: WHEN dispatch COMPLETE + 커밋 확인 → SHALL active/ → completed/ 이동
- REQ-GOV-008: SHALL 릴리즈 블로커(R-001 암호화, R-002 Null 레포) priority-critical 이슈 등록

## 수용 기준 요약

1. 5개 팀 브랜치 모두 신규 커밋 존재 (`git log main..team/* --oneline` ≥ 1)
2. `git config --get core.quotepath` = `false`
3. Team A GlobalSuppressions.cs 5개 모듈 존재
4. 04-08 dispatch 5개 → completed/ 이동 완료
5. dispatch-system.md에 `[HARD]` 커밋 의무 규칙 존재
6. worktree-integration.md에 브랜치 계층 다이어그램 존재
7. R-001, R-002 Gitea 이슈 생성 완료
8. 전체 빌드 0 errors

## 제외 범위

- PatientEntity 실제 암호화 구현 (별도 SPEC)
- Null 레포지토리 EF Core 전환 (Phase 2)
- 커버리지 향상 구현 (04-09 dispatches)
- Coordinator dispatch 내용 변경

## 영향 파일

- `.claude/rules/moai/workflow/dispatch-system.md` [MODIFY]
- `.claude/rules/moai/workflow/worktree-integration.md` [MODIFY]
- `.gitattributes` [NEW]
- `src/HnVue.Common/GlobalSuppressions.cs` [NEW]
- `src/HnVue.Security/GlobalSuppressions.cs` [NEW]
- `src/HnVue.SystemAdmin/GlobalSuppressions.cs` [NEW]
- `src/HnVue.Data/GlobalSuppressions.cs` [NEW]
- `src/HnVue.Update/GlobalSuppressions.cs` [NEW]
- `.moai/dispatches/completed/DISPATCH-*-2026-04-08.md` [NEW, 이동]
- `TestReports/QA_CROSS_REVIEW_REPORT_2026-04-09.md` [NEW]
