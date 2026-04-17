# DISPATCH: S11-R1 — Design

> **Sprint**: S11 | **Round**: 1 | **Date**: 2026-04-16
> **Team**: Design (Pure UI)
> **Priority**: P3

---

## Context

S10-R4 완료. MergeView 구현 완료.
SettingsView PPT (슬라이드 14-22) 미구현 상태.

---

## Tasks

### Task 1: SettingsView PPT 구현 (P3)

**범위**: PPT 슬라이드 14-22 (SettingsView.xaml)

**구현 항목**:
1. General Settings (스라이드 14-15)
   - Theme selection (Light/Dark/High Contrast)
   - Language selection
   - Auto-save interval

2. Display Settings (스라이드 16-17)
   - Monitor selection
   - Resolution settings
   - Color calibration

3. Medical Settings (스라이드 18-19)
   - Default modality
   - Dose warning threshold
   - Patient data retention

4. System Settings (스라이드 20-22)
   - Update settings
   - Network configuration
   - Diagnostic logging

**제약**:
- PPT 지정 페이지만 구현 (슬라이드 14-22)
- MVVM 패턴: Coordinator에게 ISettingsViewModel 필요 시 요청 (`NEEDS_VIEWMODEL` 태그)
- XAML + code-behind만: Design 영역

### Task 2: IDLE CONFIRM (P3)

할 일 없으면 IDLE 보고.

---

## Acceptance Criteria

- [ ] PPT 슬라이드 14-22 일치 여부 확인
- [ ] SettingsView.xaml 구현 완료
- [ ] 필요 시 ISettingsViewModel 요청 (Coordinator)
- [ ] 디자인 토큰 준수 (MahApps.Metro)

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: SettingsView PPT (P3) | **BLOCKED** | - | **보고서 위반**: SettingsView.xaml은 이미 PR #85로 완료된 작업. 실제 미구현 작업 식별 필요 |
| Task 2: IDLE CONFIRM (P3) | NOT_STARTED | - | - |

---

## Self-Verification Checklist

- [ ] PPT 범위 준수 (슬라이드 14-22만)
- [ ] XAML 디자인 완료
- [ ] 필요 시 Coordinator에 ViewModel 요청
- [ ] DISPATCH Status 업데이트 완료
- [ ] `/clear` 실행 완료
