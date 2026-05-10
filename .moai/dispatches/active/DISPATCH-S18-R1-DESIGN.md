# DISPATCH S18-R1 — Design (Pure UI)

## Sprint: S18 | Round: R1 | Issued: 2026-05-10
## Team: Design
## Priority: P2-High
## 근거 SPEC/문서: SPEC-UI-001 + SPEC-SPEC-TRIAGE-001 (액션 A1) + SPEC-METHODOLOGY-001

---

## 배경

S17-R1에서 PatientListView 갭 + UISPEC-002 갭 구현은 MERGED.
이번 라운드는 SPEC-UI-001 메타데이터 보강(SPEC-TRIAGE-001 액션 A1) + UISPEC-003 분석.

---

## Tasks

### T1: SPEC-UI-001 메타데이터 보강 [P1] (TRIAGE A1)
- 설명: SPEC-UI-001/spec.md 프론트매터 누락 필드 보강
- 체크리스트:
  - [ ] status, priority, team 필드 추가
  - [ ] title 명시
  - [ ] HISTORY 섹션 추가
- 완료 조건: 모든 필수 frontmatter 필드 채움

### T2: UISPEC-003 분석 [P2] (S17 carryover)
- 설명: UISPEC-003 (다음 화면) 분석 보고서 작성
- 체크리스트:
  - [ ] PPT 슬라이드 식별
  - [ ] 컴포넌트 매핑
  - [ ] 갭 분석
- 완료 조건: `.moai/specs/SPEC-UI-001/uispec-003-analysis.md` 작성

### T3: 신 Self-Verification 7항목 적용 [P2, 신규]
- 실질 커밋 SHA 기재 + UI 모듈 빌드 확인

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | SPEC-UI-001 메타데이터 보강 | NOT_STARTED | TD | P1 | - | TRIAGE A1 |
| T2 | UISPEC-003 분석 | NOT_STARTED | TD | P2 | - | S17 carryover |
| T3 | Self-Verification 7항목 | NOT_STARTED | TD | P2 | - | 신규 |

---

## Constraints

- [HARD] 소유 모듈만: HnVue.UI/Views, Styles, Themes, Components, Converters, Assets, DesignTime
- [HARD] DesignTime/ 단독 소유 — 다른 팀은 침범 금지
- [HARD] PPT 범위 엄수 — 명시 페이지만 구현
- [HARD] 빌드 검증 없이 COMPLETED 금지 (HnVue.UI 빌드)
- [HARD] ScheduleWakeup(960초) — 독립 Phase

## Evidence Required

1. `dotnet build src/HnVue.UI/HnVue.UI.csproj` 결과
2. SPEC-UI-001/spec.md frontmatter diff
3. UISPEC-003 분석 보고서 경로
4. 실질 커밋 SHA

---

## 참고 문서

- `.moai/specs/SPEC-UI-001/`, `SPEC-SPEC-TRIAGE-001/`
- `.claude/rules/teams/team-design.md`
