# DISPATCH: Design Team — S05 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-12 |
| **발행자** | Commander Center |
| **대상** | Design Team |
| **브랜치** | team/team-design |
| **유형** | S05 Round 2 — AddPatientProcedureView PPT Slide 8 구현 |
| **우선순위** | P1 |
| **SPEC 참조** | SPEC-UI-001 / UISPEC-004 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S05-R2-design.md)만 Status 업데이트.

---

## 컨텍스트

S05 R1에서 PatientListView (PPT Slide 2-4) 준수율 44%→70% 달성 완료.
이번 라운드는 **AddPatientProcedureView (PPT Slide 8)**를 구현하는 것이 목표.

PPT 범위 경계 (재확인):
- Slides 1: LoginView ✅ 완료
- Slides 2-4: PatientListView ✅ 완료 (70%)
- Slides 5-7: StudylistView ✅ 완료
- **Slides 8: AddPatientProcedureView ← 이번 DISPATCH**
- Slides 9-11: WorkflowView — 향후
- Slides 12-13: MergeView — 향후
- Slides 14-22: SettingsView — 향후

---

## 사전 확인

```bash
git checkout team/team-design
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
# 현재 상태 확인
git log --oneline -3 -- src/HnVue.UI/Views/AddPatientProcedureView.xaml
```

---

## Task 1 (P1): AddPatientProcedureView.xaml PPT Slide 8 구현

### 범위 (PPT Slide 8만)

Design Team 규칙:
- XAML 레이아웃, 스타일, 바인딩만 수정
- ViewModel 변경 필요 시: Status에 `NEEDS_VIEWMODEL:` 기재 후 Coordinator 위임

### 구현 대상 (PPT Slide 8 기준)

1. **환자 정보 입력 폼**: 이름, 생년월일, 성별, 환자 ID 필드
2. **검사 프로시저 선택**: 드롭다운 또는 목록 기반 선택
3. **확인/취소 버튼**: MahApps 스타일 일관성

### 금지 사항

- Slide 8 이외의 화면 요소 추가 금지
- Thumbnail strip, ImageViewer 요소 추가 금지 (Issue #59 재발 방지)
- ViewModel 직접 수정 금지 (NEEDS_VIEWMODEL 기재 후 Coordinator 위임)

### 검증

```bash
# 빌드 확인 (XAML 구문 오류 검사)
dotnet build src/HnVue.UI/ 2>&1 | tail -5
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI/Views/AddPatientProcedureView.xaml
git add src/HnVue.UI/Views/AddPatientProcedureView.xaml.cs  # 필요 시
# DISPATCH.md, CLAUDE.md 절대 추가 금지
git commit -m "feat(design): UISPEC-004 AddPatientProcedureView PPT Slide 8 구현"
git push origin team/team-design
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: AddPatientProcedureView 구현 | NOT_STARTED | -- | PPT Slide 8 기준 |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
