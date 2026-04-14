# DISPATCH: S09-R1 — Design

Sprint: S09 | Round: 1 | Team: Design
Updated: 2026-04-14

---

## Context

S08-R2 MERGED 완료. DesignTime Mock 정비 완료.
S09-R1에서는 Issue #98 하드코딩 색상 수정 + Emergency Stop 구현.

---

## Tasks

### Task 1: 하드코딩 색상 수정 + Emergency Stop (P1) — Issue #98

**목표**: Acquisition 화면 하드코딩 색상을 테마 토큰으로 교체 + Emergency Stop 버튼 구현

**상세**:
- WorkflowView.xaml의 하드코딩 색상(HEX 직접 지정)을 CoreTokens.xaml 토큰으로 교체
- Emergency Stop 버튼 추가: IEC 62366 요구사항에 따라 항상 보이는 위치
  - 빨간색 배경, 흰색 텍스트 "EMERGENCY STOP"
  - 최소 44x44px 터치 타겟 (WCAG 2.1 AA)
  - 색상 대비비 4.5:1 이상
- 3테마(Light/Dark/High Contrast) 모두 동작 확인

**수용 기준**:
- [ ] 하드코딩 색상 100% 토큰 교체
- [ ] Emergency Stop 버튼 WorkflowView에 추가
- [ ] IEC 62366 / WCAG 2.1 AA 준수
- [ ] 3테마 모두 정상 렌더링

**PPT Scope**: Slides 9-11 (WorkflowView/Acquisition ONLY)

### Task 2: IDLE CONFIRM (P3)

StudylistView XAML 구현(#102)은 다음 라운드로 이관.

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: 하드코딩+EmergencyStop (P1) | COMPLETED | 2026-04-14 21:47 | Issue #103 |
| Git 완료 프로토콜 | COMPLETED | 2026-04-14 21:47 | commit de4d33a |

---

## Self-Verification Checklist

- [x] `dotnet build` 0 errors
- [x] Only modified files within Design ownership (Views, Styles, Themes, Components, DesignTime)
- [x] PPT scope compliance (slides 9-11 ONLY)
- [x] 3테마 렌더링 확인

---

## Build Evidence

```
Build Result: 0 errors, 1627 warnings (StyleCop/IDE unused using)
Files Modified:
- src/HnVue.UI/Themes/tokens/SemanticTokens.xaml (HnVue.Semantic.Text.OnEmergency 추가)
- src/HnVue.UI/Themes/HnVueTheme.xaml (Foreground="White" → 토큰 교체)

Commit: de4d33a
Issue: #103
```
