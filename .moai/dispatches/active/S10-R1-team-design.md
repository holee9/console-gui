# DISPATCH: S10-R1 — Design

Sprint: S10 | Round: 1 | Team: Design
Updated: 2026-04-15

---

## Context

S09-R3 QA PASS. DesignSystemConverters NullReference 14건 해결. HnVue.UI 83.0% (85% 게이트 미달 — Views code-behind 0% 커버).

---

## Tasks

### Task 1: PPT 미구현 화면 구현 — MergeView (P1)

PPT slides 12-13에 해당하는 MergeView 화면 구현.

**PPT Scope Compliance [HARD]**:
- Slides 12-13: MergeView.xaml ONLY
- 다른 화면 요소 절대 포함 금지

**검증 기준**:
- [ ] PPT 1:1 비교 완료
- [ ] `dotnet build` 0 errors
- [ ] DesignTime Mock 렌더링 정상

### Task 2: UI Components 커버리지 향상 (P2)

HnVue.UI 83.0% → 85%+ 기여. Components/Converters 테스트 가능 코드 개선.

**검증 기준**:
- [ ] UI Components 테스트 커버리지 향상
- [ ] `dotnet build` 0 errors

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: MergeView (P1) | NOT_STARTED | - | PPT slides 12-13 |
| Task 2: UI 커버리지 (P2) | NOT_STARTED | - | |

---

## Self-Verification Checklist

- [ ] `dotnet build` 0 errors
- [ ] PPT scope compliance 확인
- [ ] DesignTime Mock 빌드 확인
- [ ] DISPATCH Status에 빌드 증거 기록
