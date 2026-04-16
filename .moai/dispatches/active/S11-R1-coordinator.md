# DISPATCH: S11-R1 — Coordinator

> **Sprint**: S11 | **Round**: 1 | **Date**: 2026-04-16
> **Team**: Coordinator (Integration)
> **Priority**: P2

---

## Context

S10-R4 QA CONDITIONAL PASS. HnVue.UI 전체 82.3% (목표 85%).
ToastItem 완료 (68.9% → 85%+), 다른 Components/Converters 낮음:
- 일반 Converters: 70-80% 범위
- Medical 컴포넌트: AcquisitionPreview 77% (Team B 영향, but Coordinator C# 코드)

---

## Tasks

### Task 1: UI Components/Converters 커버리지 개선 (P2)

**목표**: HnVue.UI 82.3% → 85%+ (2.7% gap)

**타겟 선정 (Coordinator 소유 C# 코드)**:
1. **Converters** (일반 UI, 도메인 제외):
   - BoolToVisibilityConverter: 반전 로직 테스트
   - InverseBoolToVisibilityConverter: null/값 범위
   - NullToVisibilityConverter: null/false/true 분기

2. **UI Services** (Toast 완료, 다른 서비스):
   - ThemeRollbackService: 롤백 실패 시나리오
   - ModalDialogService: show/hide 상태 관리

**제외** (Team B 소유):
- AcquisitionPreview, PatientInfoCard, StudyThumbnail (Medical 도메인)
- AgeFromBirthDateConverter, SafeStateToColorConverter (도메인 Converter)

**접근법**:
- MVVM ViewModel 테스트 패턴 (CommunityToolkit.Mvvm)
- Mock IDialogService, IThemeService

### Task 2: IDLE CONFIRM (P3)

할 일 없으면 IDLE 보고.

---

## Acceptance Criteria

- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` 전체 통과
- [ ] HnVue.UI 커버리지 85%+ (82.3% → 85%)
- [ ] 소유권 범위 내 파일만 수정 (Converters/Services)

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: UI 커버리지 (P2) | NOT_STARTED | - | 2.7% gap 해소 |
| Task 2: IDLE CONFIRM (P3) | NOT_STARTED | - | - |

---

## Self-Verification Checklist

- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` PASS (ViewModels 748+)
- [ ] 커버리지 85%+ 달성
- [ ] DISPATCH Status 업데이트 완료
- [ ] `/clear` 실행 완료
