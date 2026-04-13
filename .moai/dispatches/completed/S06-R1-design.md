# DISPATCH: Team Design — S06 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-13 |
| **발행자** | Commander Center |
| **대상** | Team Design |
| **브랜치** | team/team-design |
| **유형** | S06 R1 — SettingsView PPT 정합성 개선 |
| **우선순위** | P1-High |
| **SPEC 참조** | SPEC-UI-001 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S06-R1-design.md)만 Status 업데이트.

---

## 컨텍스트

SPEC-UI-001 기반 UI 리디자인 진행 중. LoginView, PatientListView, StudylistView, WorkflowView
3열 레이아웃은 완료됨. PPT Slide 14-22 SettingsView의 PPT 정합성 개선이 다음 우선순위.

현재 SettingsView.xaml은 563줄로 기존 구현 상태. PPT 디자인 스펙과 정합성 확인 필요.

---

## 사전 확인

```bash
git checkout team/team-design
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
```

---

## Task 1 (P1): SettingsView PPT 정합성 개선 (Slide 14-22)

### 범위

1. PPT Slide 14-22에 명시된 SettingsView 디자인 스펙과 현재 구현 비교
2. PPT 명세에 맞게 레이아웃, 컴포넌트, 색상, 타이포그래피 조정
3. MahApps.Metro 테마 준수 (Light/Dark/High Contrast)
4. 접근성 요구사항 준수 (WCAG 2.1 AA)

### PPT Scope [HARD]

- **Slides 14-22: SettingsView.xaml ONLY**
- 다른 화면 수정 금지

### 대상 파일

- `src/HnVue.UI/Views/SettingsView.xaml` — 메인 수정 대상
- `src/HnVue.UI/Themes/` — 필요시 토큰 업데이트

### 검증

```bash
dotnet build src/HnVue.UI/ --configuration Release 2>&1 | tail -5
# PPT 정합성 1:1 비교 결과를 DISPATCH Status에 기록
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI/Views/SettingsView.xaml src/HnVue.UI/Themes/
git commit -m "feat(design): SPEC-UI-001 SettingsView PPT Slide 14-22 정합성 개선"
git push origin team/team-design
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: SettingsView PPT 정합성 (P1) | **COMPLETED** | 2026-04-13 09:15 | Build 0 errors, 524 tests passed. Issue #85. DesignTime+접근성+10탭 폼 구현 |
| Git 완료 프로토콜 | **COMPLETED** | 2026-04-13 09:16 | commit 0ce907f, pushed to team/team-design |

### Build Evidence

```
HnVue.UI Build: 0 errors (Release)
HnVue.UI.Tests: 524 passed, 0 failed, 0 skipped
Full Solution: errors in HnVue.Data.Tests (other team code) - Design modules clean
```

### PPT 1:1 Comparison

| PPT Slide | Element | Status |
|-----------|---------|--------|
| 14 (System) | Access Notice + Priority + Language + Auto Logout | Match |
| 15 (Account) | ID + Password + Role ComboBox + DataGrid | Match |
| 16 (Detector) | Type + Connection + IP + Calibration | Match |
| 17 (Generator) | Model + Port + Baud Rate | Match |
| 18 (Network) | PACS + Worklist + Print merged | Match |
| 19 (Display) | Theme + Window Mode + Overlays | Match |
| 20 (Option) | Auto-receive + Auto-print + W/L default | Match |
| 21 (Database) | Path + Backup + Restore | Match |
| 22 (DicomSet) | AE Title + Station + Transfer Syntax | Match |
| 22 (RIS Code) | Matching + Un-Matched DataGrids | Match |

### Files Modified (Scope: SettingsView ONLY)

- `src/HnVue.UI/Views/SettingsView.xaml` (modified)
- `src/HnVue.UI/DesignTime/DesignTimeSettingsViewModel.cs` (new)
