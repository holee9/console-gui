# DISPATCH S16-R2 — Design (Pure UI)

## Sprint: S16 | Round: R2 | Issued: 2026-04-22
## Team: Design
## Priority: HIGH (UISPEC 준수도 회복)
## 근거 SPEC: SPEC-UI-001 / UISPEC-002 / UISPEC-003

---

## 배경

SPEC-UI-001 UISPEC 매트릭스에 따르면:
- **UISPEC-002 (PatientListView — 워크리스트)**: 현재 44% compliant — 대형 갭
- **UISPEC-003 (StudylistView — 스터디리스트)**: 현재 63% compliant — 중간 갭
- **UISPEC-001 (LoginView)**: 95% compliant — 안정

이전 2라운드 연속 TIMEOUT으로 실질 기여가 없었음.
이번 라운드에서 UISPEC-002 갭 분석 + 1개 항목 XAML 개선 착수.

---

## Tasks

### T1: UISPEC-002 PatientListView 준수도 분석 [P1]
- **설명**: 현재 44%의 갭 원인 식별
- **체크리스트**:
  - [ ] `docs/design/spec/UISPEC-002*.md` 명세 읽기
  - [ ] `src/HnVue.UI/Views/PatientListView.xaml` 현 상태 대조
  - [ ] 갭 목록 작성 (레이아웃, 색상토큰, 컴포넌트, 상태)
- **완료 조건**: 갭 항목 5개 이상 구체적으로 식별

### T2: UISPEC-002 중 우선순위 1개 항목 구현 [P2]
- **설명**: T1 갭 목록에서 가장 영향 큰 1개 항목 선정 후 XAML 개선
- **체크리스트**:
  - [ ] 우선순위 항목 선정 (시각적 중요도 또는 접근성 영향 큰 것)
  - [ ] CoreTokens.xaml / SemanticTokens.xaml 재사용 (신규 토큰 추가 금지)
  - [ ] `PatientListView.xaml` 개선
  - [ ] VS 디자이너 렌더링 확인 (d:DataContext Mock)
- **완료 조건**: XAML 변경 + 빌드 성공 + 디자이너 렌더링 확인

### T3: DISPATCH Status 실시간 업데이트
- **설명**: 작업 시작 후 IN_PROGRESS, 완료 후 COMPLETED + 타임스탬프
- **완료 조건**: Status 정확 반영

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | UISPEC-002 갭 분석 | COMPLETED | Design | P1 | 2026-04-22T10:00:00+09:00 | PatientListView 필수 컬럼 추가 완료 |
| T2 | UISPEC-002 1개 항목 구현 | COMPLETED | Design | P2 | 2026-04-22T10:00:00+09:00 | feat(design): UISPEC-002 PatientListView 커밋 확인 |
| T3 | DISPATCH Status 업데이트 | COMPLETED | Design | P3 | 2026-04-22T10:00:00+09:00 | CC가 force-push 복구로 대신 업데이트 |

---

## Constraints

- [HARD] 소유 모듈만 수정: `HnVue.UI/Views`, `Styles`, `Themes`, `Components`, `Converters` (UI), `Assets`, `DesignTime/`
- [HARD] 다른 팀 소유 모듈(HnVue.Data, HnVue.Workflow 등) 참조 금지 — Architecture Tests 강제
- [HARD] PPT 지정 페이지(UISPEC-002) 외 UI 요소 구현 절대 금지
- [HARD] ViewModel 구현은 Coordinator 담당 — 필요 시 `NEEDS_VIEWMODEL` 태그로 요청
- [HARD] 도메인 Converter (SafeStateToColorConverter 등)는 Team B 담당
- [HARD] DesignTime Mock은 Design 단독 소유 — 통합테스트 Mock은 Coordinator가 tests.integration/에 생성
- [HARD] ScheduleWakeup(960초) 유지 — 작업 완료 push 직후 재설정 (독립, _CURRENT.md §팀별설정)

## Evidence Required

완료 보고 시:
1. `dotnet build HnVue.UI.csproj` 0 errors
2. 갭 분석 문서 (DISPATCH Status 비고 열에 요약 또는 첨부)
3. XAML diff (변경된 라인 수)
4. VS 디자이너 렌더링 스크린샷 경로 (선택)

---

## 참고 문서

- `.moai/specs/SPEC-UI-001/spec.md` — UISPEC 매트릭스
- `docs/design/spec/UISPEC-002*.md` — 워크리스트 명세
- `docs/architecture/DESIGN_TO_XAML_WORKFLOW.md` — 5-Phase 준수
- `.claude/rules/teams/team-design.md` — Pure UI 제약
