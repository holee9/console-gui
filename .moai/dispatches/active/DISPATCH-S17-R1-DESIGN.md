# DISPATCH S17-R1 — Design (Pure UI)

## Sprint: S17 | Round: R1 | Issued: 2026-04-22
## Team: Design
## Priority: P3-Medium (UISPEC 준수도 향상)
## 근거 SPEC/문서: SPEC-UI-001 / UISPEC-002 / UISPEC-003

---

## 배경

S16-R2에서 PatientListView 필수 컬럼 추가 완료. 그러나 UISPEC-002(워크리스트)는 여전히 낮은 준수도.
UISPEC-003(스터디리스트)도 63% compliant로 갭이 존재.
이 라운드에서 UISPEC-002 남은 갭 항목 + UISPEC-003 분석 착수.

---

## Tasks

### T1: UISPEC-002 PatientListView 남은 갭 항목 구현 [P1]
- **설명**: S16-R2에서 식별된 갭 중 미구현 항목 추가 구현
- **체크리스트**:
  - [ ] S16-R2 T1 갭 분석 결과 재확인
  - [ ] `docs/design/spec/UISPEC-002*.md` 명세에서 미구현 항목 식별
  - [ ] 우선순위 2~3개 항목 선정 (시각적/접근성 영향 큰 것)
  - [ ] CoreTokens.xaml / SemanticTokens.xaml 재사용 (신규 토큰 추가 금지)
  - [ ] `PatientListView.xaml` 개선
  - [ ] VS 디자이너 렌더링 확인 (d:DataContext Mock)
- **완료 조건**: 2~3개 갭 항목 XAML 개선 + 빌드 성공

### T2: UISPEC-003 StudylistView 갭 분석 [P2]
- **설명**: StudylistView 63% compliant의 갭 원인 분석
- **체크리스트**:
  - [ ] `docs/design/spec/UISPEC-003*.md` 명세 읽기
  - [ ] `src/HnVue.UI/Views/StudylistView.xaml` 현 상태 대조
  - [ ] 갭 목록 작성 (레이아웃, 색상토큰, 컴포넌트, 상태)
  - [ ] S18 우선순위 정리
- **완료 조건**: 갭 항목 5개 이상 구체적으로 식별

### T3: DISPATCH Status 실시간 업데이트
- **설명**: 작업 시작 후 IN_PROGRESS, 완료 후 COMPLETED + 타임스탬프
- **완료 조건**: Status 정확 반영

---

## Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 타임스탬프 | 비고 |
|---------|------|------|--------|----------|-----------|------|
| T1 | UISPEC-002 남은 갭 구현 | COMPLETED | Design | P1 | 2026-04-22T19:05:00+09:00 | 우측 패널 제목 추가 ✅ |
| T2 | UISPEC-003 갭 분석 | COMPLETED | Design | P2 | 2026-04-22T19:05:00+09:00 | 갭 분석 완료 ✅ |
| T3 | DISPATCH Status 업데이트 | COMPLETED | Design | P3 | 2026-04-22T19:10:00+09:00 | 완료 보고 |

---

## Constraints

- [HARD] 소유 모듈만 수정: `HnVue.UI/Views`, `Styles`, `Themes`, `Components`, `Converters` (UI), `Assets`, `DesignTime/`
- [HARD] 다른 팀 소유 모듈(HnVue.Data, HnVue.Workflow 등) 참조 금지 — Architecture Tests 강제
- [HARD] PPT 지정 페이지(UISPEC-002, UISPEC-003) 외 UI 요소 구현 절대 금지
- [HARD] ViewModel 구현은 Coordinator 담당 — 필요 시 `NEEDS_VIEWMODEL` 태그로 요청
- [HARD] DesignTime Mock은 Design 단독 소유
- [HARD] ScheduleWakeup(960초) 유지 — 작업 완료 push 직후 재설정 (독립, _CURRENT.md)

## Evidence Required

완료 보고 시:
1. `dotnet build HnVue.UI.csproj` 0 errors
2. XAML diff (변경된 라인 수)
3. 갭 분석 결과 요약

---

## 참고 문서

- `.moai/specs/SPEC-UI-001/spec.md` — UISPEC 매트릭스
- `docs/design/spec/UISPEC-002*.md` — 워크리스트 명세
- `docs/design/spec/UISPEC-003*.md` — 스터디리스트 명세
- `.claude/rules/teams/team-design.md` — Pure UI 제약
