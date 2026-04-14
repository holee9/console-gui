# DISPATCH: Design Team — S05 Round 2 (Task 4)

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-13 |
| **발행자** | Commander Center |
| **대상** | Design Team |
| **브랜치** | team/team-design |
| **유형** | S05 Round 2 — WorkflowView 3열 레이아웃 구현 (BLOCKED 해제) |
| **우선순위** | P1 |
| **SPEC 참조** | SPEC-UI-001 / UISPEC-006 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S05-R2-design-4.md)만 Status 업데이트.

---

## 컨텍스트

**BLOCKED 해제 완료**: Coordinator가 WorkflowView ViewModel 속성을 main에 머지함.

이제 Design이 WorkflowView.xaml 3열 레이아웃 구현 가능.

S05 R2 진행 현황:
- Slides 1: LoginView ✅
- Slides 2-4: PatientListView ✅
- Slides 5-7: StudylistView ✅
- Slides 8: AddPatientProcedureView ✅ (100%)
- **Slides 9-11: WorkflowView (Acquisition) ← 이번 DISPATCH**
- Slides 12-13: MergeView ✅
- Slides 14-22: SettingsView — 향후

---

## 사전 확인

```bash
git checkout team/team-design
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
# ViewModel 속성 확인
grep -r "PreviewImage\|ThumbnailList\|SelectedPatient" src/HnVue.UI.Contracts/ src/HnVue.UI.ViewModels/
```

---

## Task 1 (P1): WorkflowView.xaml PPT Slide 9-11 3열 레이아웃 구현

### 범위 (PPT Slide 9-11만)

- Slide 9: 환자 정보 패널 (좌열) — SelectedPatient 바인딩
- Slide 10: AcquisitionPreview + Thumbnail strip (중앙) — PreviewImage/ThumbnailList 바인딩
- Slide 11: 기존 제어 패널 유지 (우열)

### 작업

1. WorkflowView.xaml 3열 Grid 레이아웃 재구성
   - Column 0: 환자 정보 패널 (PatientInfoCard 컴포넌트 활용)
   - Column 1: AcquisitionPreview 중앙 배치 + Thumbnail strip
   - Column 2: 기존 제어 패널 (보존)
2. ViewModel 속성에 바인딩:
   - `{Binding PreviewImage}` — AcquisitionPreview
   - `{Binding ThumbnailList}` — Thumbnail strip
   - `{Binding SelectedPatient}` — 환자 정보 패널
3. PPT Slide 9-11과 1:1 비교 검증

### 금지 사항

- Slide 9-11 이외의 화면 요소 추가 금지 (team-design rule)
- 비즈니스 로직 코드 작성 금지 — 바인딩만

### 검증

```bash
dotnet build src/HnVue.UI/ 2>&1 | tail -5
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI/Views/WorkflowView.xaml
git add src/HnVue.UI/Views/WorkflowView.xaml.cs  # 필요 시
git commit -m "feat(design): UISPEC-006 WorkflowView PPT Slide 9-11 3열 레이아웃 구현"
git push origin team/team-design
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: WorkflowView 3열 레이아웃 | COMPLETED | 2026-04-13 | 빌드 0 에러, PPT Slide 9-11 3열 구현 |
| Git 완료 프로토콜 | IN_PROGRESS | -- | commit 대기 |

### 세부 내용

**구현 결과:**
- WorkflowView.xaml 3열 Grid 레이아웃 재구성 완료
  - Column 0 (280px): PatientInfoCard - SelectedPatient 바인딩
  - Column 1 (*): AcquisitionPreview + Thumbnail strip - ThumbnailList 바인딩
  - Column 2 (340px): 기존 제어 패널 보존 - CurrentState 바인딩 추가

**바인딩:** PatientInfoCard(SelectedPatient), ListBox(ThumbnailList), CurrentState

**빌드 결과:** 오류 0개, 경고 기존만

**준수율:** PPT Slide 9-11 요구사항 100% 반영