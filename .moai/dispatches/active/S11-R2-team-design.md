# DISPATCH: S11-R2 — Design

> **Sprint**: S11 | **Round**: 2 | **Date**: 2026-04-17
> **Team**: Design (Pure UI)
> **Priority**: P2

---

## Context

S11-R1 SettingsView 완료 (PR #85).

다음 미구현 화면 필요.

---

## Tasks

### Task 1: AcquisitionView 세부 디자인 (P2)

**대상**: WorkflowView.xaml (Acquisition)

**범위**: PPT 슬라이드 9-11

**구현 항목**:
1. Thumbnail strip 디자인
2. Acquisition controls 배치
3. State indicators

**제약**:
- PPT 슬라이드 9-11만 구현
- ViewModel 필요 시 Coordinator에 `NEEDS_VIEWMODEL` 태그 요청
- XAML + code-behind만

### Task 2: DesignToken 업데이트 (P3)

**대상**: CoreTokens.xaml

**목표**: 일관된 디자인 토큰 유지

---

## Acceptance Criteria

- [ ] PPT 슬라이드 9-11 구현 완료
- [ ] DesignToken 준수
- [ ] 소유권 준수 (Views, Styles, Themes, Components, Converters, Assets만)

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: AcquisitionView 디자인 (P2) | NOT_STARTED | - | - |
| Task 2: DesignToken 업데이트 (P3) | NOT_STARTED | - | - |

---

## Self-Verification Checklist

- [ ] PPT 범위 준수 (슬라이드 9-11만)
- [ ] XAML 디자인 완료
- [ ] 소유권 준수 (UI만)
- [ ] DISPATCH Status 업데이트
- [ ] `/clear` 실행 완료
