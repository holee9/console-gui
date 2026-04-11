# DISPATCH Current Index

> **목적**: 각 팀의 현재 활성 DISPATCH를 명확히 식별. 에이전트는 반드시 이 파일을 먼저 읽고 자신의 DISPATCH 파일을 확인한다.
>
> **규칙**: 팀당 항상 정확히 하나의 활성 DISPATCH만 존재한다.

Updated: 2026-04-11

## 현재 활성 DISPATCH (S04 Round 1)

| 팀 | 파일 | 우선순위 | 상태 | 핵심 작업 |
|----|------|----------|------|-----------|
| **Coordinator** | `S04-R1-coordinator.md` | P0 | NOT_STARTED | NullRepository 6개 → EF Core 교체 (런타임 블로커) |
| **QA** | `S04-R1-qa.md` | P0 | NOT_STARTED | Views/Migrations 제외 정책 공식화 + S04 Gate Report |
| **Team A** | `S04-R1-team-a.md` | P0 | NOT_STARTED | PHI AES-256-GCM 암호화 구현 (IEC 62304 위반 해소) |
| **Team B** | `S04-R1-team-b.md` | P1 | NOT_STARTED | Dicom 49.6%→80%, Update 75%→85% |
| **Design** | `S04-R1-design.md` | P0 | NOT_STARTED | UI.QA 13개 테스트 실패 수정 + StudylistView 리디자인 |
| **RA** | `S04-R1-ra.md` | P1 | NOT_STARTED | DOC-042 CMP Draft→Approved |

## 중요 블로킹 의존성

```
S04 Gate 진입 (CONDITIONAL → APPROVED)
  ├── Coordinator P0: NullRepository 6개 교체 완료
  │   └── Team B/Dose/Update 검증이 이에 의존
  ├── QA P0: coverage.runsettings 제외 정책 공식화
  └── Design P0: UI.QA 13개 테스트 실패 수정

Team A P0 완료 후
  └── RA P2: RTM SWR-CS-080 TC 매핑 실행 가능
```

## DISPATCH 읽기 규칙 (에이전트용)

1. 이 파일(`_CURRENT.md`)에서 자신의 팀을 찾는다
2. 해당 파일을 읽고 작업을 시작한다
3. 이전 날짜의 DISPATCH 파일은 모두 `completed/`에 있으며 무시한다
4. DISPATCH 파일 경로는 **항상 Main 프로젝트 디렉토리 기준**: `D:/workspace-gitea/Console-GUI/.moai/dispatches/active/`

## 이력

| 날짜 | 라운드 | 파일 | 상태 |
|------|--------|------|------|
| 2026-04-08 | Phase 0 | DISPATCH-*-2026-04-08.md | completed/ |
| 2026-04-09 | Phase 1 QA Coverage | DISPATCH-*-2026-04-09.md | SUPERSEDED → completed/ |
| 2026-04-11 | S04 Round 1 | S04-R1-*.md | **현재 활성** |
