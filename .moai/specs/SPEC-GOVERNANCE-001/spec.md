---
id: SPEC-GOVERNANCE-001
version: 1.0.0
status: draft
created: 2026-04-09
updated: 2026-04-09
author: MoAI Orchestrator (UltraThink)
priority: P1-Critical
issue_number: 0
title: 팀 개발 거버넌스 강제화 — 워크트리 규율·이슈 추적·한글 인코딩
---

## HISTORY

| 버전 | 날짜 | 변경 내용 |
|------|------|-----------|
| 1.0.0 | 2026-04-09 | 독립 감사 결과 기반 초안 작성 (UltraThink) |

---

## 개요

독립 코드베이스 감사(2026-04-09) 결과, 팀 개발 프로세스에서 3가지 구조적 위반이 반복적으로 발생하고 있음이 확인되었습니다:

1. **워크트리 규율 위반**: 모든 dispatch 작업이 git commit 없이 "COMPLETE" 보고됨
2. **이슈 추적 미준수**: Gitea 이슈 생성 없이 dispatch 작업 직접 실행
3. **한글 인코딩 위반**: `core.quotepath=true`(기본값)로 인해 한국어 파일명이 터미널에서 8진수로 표시

또한 worktree 브랜치 계층(main → coordinator → team-a/team-b)이 명문화되지 않아 반복적으로 우회되고 있습니다.

이 SPEC은 규칙 명문화 + 즉각 교정 + 장기 거버넌스 강화를 통합 처리합니다.

---

## 위반 현황 (감사 기준)

### 즉각 조치 필요

| ID | 팀 | 심각도 | 내용 |
|----|-----|--------|------|
| V-001 | 전체 | **CRITICAL** | 5개 팀 dispatch 작업이 미커밋 상태 (소실 위험) |
| V-002 | Team A | **HIGH** | SA* GlobalSuppressions 5개 모듈 미적용 |
| V-003 | QA | **MEDIUM** | QA 교차 리뷰 보고서 파일 미저장 (콘솔 출력만) |
| V-004 | 전체 | **MEDIUM** | 완료된 04-08 dispatch가 active/ 폴더에 잔류 |
| V-005 | 전체 | **MEDIUM** | git `core.quotepath=true`로 한글 파일명 8진수 표시 |

### 장기 미결 리스크

| ID | 심각도 | 내용 |
|----|--------|------|
| R-001 | **HIGH** | PatientEntity.cs:8 — AES-256-GCM 암호화 TODO (SWR-CS-080) |
| R-002 | **MEDIUM** | App.xaml.cs — 6개 Null 레포지토리 스텁 (실구현 미완) |
| R-003 | **HIGH** | 전체 커버리지 75.6% (80% interim gate 미달) |

---

## 요구사항 (EARS 형식)

### [REQ-GOV-001] Dispatch 완료 시 커밋 의무

**유형**: Event-Driven

**WHEN** 팀 워크트리 에이전트가 dispatch 태스크를 완료 처리할 때,  
**THE SYSTEM SHALL** 해당 팀 브랜치에 최소 1개의 git commit을 생성한 후에만 dispatch 상태를 COMPLETE로 변경해야 한다.

**근거**: V-001 — 5개 팀 전체 작업이 미커밋 상태로 "COMPLETE" 보고됨.

---

### [REQ-GOV-002] 워크트리 브랜치 계층 구조

**유형**: State-Driven

**WHILE** 팀 기반 개발 모드가 활성화되어 있는 동안,  
**THE SYSTEM SHALL** 다음 계층 구조를 강제해야 한다:
- `main` ← 사용자-MoAI 커맨더센터 (직접 구현 금지)
- `team/coordinator` ← team-a/team-b 통합 게이트
- `team/team-a`, `team/team-b` ← 워커 팀 (coordinator 경유 없이 main 직접 머지 금지)
- `team/qa`, `team/ra`, `team/team-design` ← 독립 전문 팀

**근거**: 사용자 정의 커맨더센터 원칙 + 반복적 계층 우회.

---

### [REQ-GOV-003] Coordinator 통합 게이트

**유형**: Event-Driven

**WHEN** Team A 또는 Team B가 dispatch를 완료하고 main 머지를 요청할 때,  
**THE SYSTEM SHALL** Coordinator 팀의 명시적 리뷰 및 승인을 거쳐야 한다.

**인터페이스 변경 시 추가**: UI.Contracts 변경은 Coordinator만 수정 가능하며, 변경 시 interface-contract 레이블 이슈를 생성해야 한다.

---

### [REQ-GOV-004] 이슈 선행 생성 (Issue-First)

**유형**: Event-Driven

**WHEN** Main이 새 dispatch를 팀에 발행할 때,  
**THE SYSTEM SHALL** 대응하는 Gitea 이슈를 생성하고 dispatch 문서에 이슈 번호를 기록한 후에 dispatch를 배포해야 한다.

**근거**: 현재 dispatch 문서에 이슈 참조 없음. 작업 추적 불가.

---

### [REQ-GOV-005] 한글 인코딩 표시 정합성

**유형**: Feature

**THE SYSTEM SHALL** git 저장소가 다음 설정을 유지해야 한다:
- `git config core.quotepath false` (한글 파일명 8진수 표시 방지)
- `.gitattributes`에 `* text=auto` 및 `*.cs text eol=crlf encoding=utf-8`
- 모든 소스 파일은 BOM 없는 UTF-8로 저장

**근거**: V-005 — `core.quotepath` 기본값으로 한글 파일명이 `\353\263\200\352\262\275`로 표시됨.

---

### [REQ-GOV-006] QA 보고서 파일 저장 의무

**유형**: Event-Driven

**WHEN** QA 에이전트가 분석·교차 리뷰·커버리지 측정을 완료할 때,  
**THE SYSTEM SHALL** `TestReports/{REPORT-TYPE}_{YYYY-MM-DD}.md` 형식으로 보고서 파일을 저장해야 한다.

**근거**: V-003 — QA 교차 리뷰 결과가 콘솔 출력만으로 존재하며 파일에 저장되지 않음.

---

### [REQ-GOV-007] Dispatch 수명주기 완전 이행

**유형**: Event-Driven

**WHEN** dispatch 상태가 COMPLETE로 전환되고 팀 브랜치 커밋이 확인될 때,  
**THE SYSTEM SHALL** `.moai/dispatches/active/`의 파일을 `.moai/dispatches/completed/`로 이동해야 한다.

**근거**: V-004 — 04-08 완료 dispatch 5개가 active/ 폴더에 잔류.

---

### [REQ-GOV-008] 릴리즈 블로커 이슈 등록 의무

**유형**: Feature

**THE SYSTEM SHALL** 릴리즈 블로커로 분류된 미완료 항목에 대해 `priority-critical` 레이블이 붙은 Gitea 이슈를 생성해야 한다:
- R-001: PatientEntity AES-256-GCM 암호화 미구현
- R-002: 6개 Null 레포지토리 실구현 전환 계획

---

## 제외 범위 (What NOT to Build)

- 실제 커버리지 향상 구현 (04-09 dispatches에서 별도 처리)
- PatientEntity 암호화 실구현 (별도 SPEC으로 분리)
- Null 레포지토리 EF Core 전환 (Phase 2 작업)
- Coordinator dispatch 내용 변경 (04-09 dispatch 이미 발행됨)

---

## 영향 범위

### [MODIFY] 규칙 문서
- `.claude/rules/moai/workflow/dispatch-system.md` — 커밋 의무 규칙 추가
- `.claude/rules/moai/workflow/worktree-integration.md` — 브랜치 계층 명시

### [NEW] 설정 파일
- `.gitattributes` — UTF-8 인코딩 설정 추가
- `git config core.quotepath false` 적용

### [NEW] 즉각 교정 작업
- 5개 팀 브랜치 미커밋 작업 commit
- Team A 5개 모듈 GlobalSuppressions.cs 추가
- 04-08 dispatch active/ → completed/ 이동
- R-001, R-002 Gitea 이슈 생성

### [NEW] QA 보고서
- `TestReports/QA_CROSS_REVIEW_REPORT_2026-04-09.md` 재생성·저장
