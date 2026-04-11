# DISPATCH: Design Team — S05 Round 2 (Task 3)

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-12 |
| **발행자** | Commander Center |
| **대상** | Design Team |
| **브랜치** | team/team-design |
| **유형** | S05 Round 2 추가 — WorkflowView PPT Slide 9-11 검증 |
| **우선순위** | P1 |
| **SPEC 참조** | SPEC-UI-001 / UISPEC-006 |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] DISPATCH.md와 CLAUDE.md는 수정 금지. 이 파일(.moai/dispatches/active/S05-R2-design-3.md)만 Status 업데이트.

---

## 컨텍스트

S05 R2 진행 현황:
- Slides 1: LoginView ✅
- Slides 2-4: PatientListView ✅ (70%)
- Slides 5-7: StudylistView ✅
- Slides 8: AddPatientProcedureView ✅ (100%)
- **Slides 9-11: WorkflowView (Acquisition) ← 이번 DISPATCH**
- Slides 12-13: MergeView ✅ (90%)
- Slides 14-22: SettingsView — 향후

WorkflowView는 Acquisition 화면으로 **Thumbnail strip 포함이 허용**된 유일한 화면.

---

## 사전 확인

```bash
git checkout team/team-design
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
git log --oneline -5 -- src/HnVue.UI/Views/WorkflowView.xaml
```

---

## ⚠️ 대기 조건 [BLOCKED — Coordinator 완료 후 착수]

> Coordinator가 WorkflowView ViewModel 속성(PreviewImage/ThumbnailList/SelectedPatient/WorkflowState)을
> push → CC가 main 머지 → `git pull origin main` 확인 후 착수.
> Coordinator DISPATCH: `S05-R2-coordinator.md` Task 1 참조.

---

## Task 1 (P1): WorkflowView.xaml PPT Slide 9-11 3열 레이아웃 구현

### 범위 (PPT Slide 9-11만)

- Slide 9: 환자 정보 패널 (좌열) — SelectedPatient 바인딩
- Slide 10: AcquisitionPreview + Thumbnail strip (중앙) — PreviewImage/ThumbnailList 바인딩
- Slide 11: 기존 제어 패널 유지 (우열)

### 작업

1. WorkflowView.xaml 3열 Grid 레이아웃 재구성
2. 환자 정보 패널 추가 (PatientInfoCard 컴포넌트 활용)
3. AcquisitionPreview 중앙 배치 + Thumbnail strip 추가
4. Coordinator가 추가한 ViewModel 속성에 바인딩

### 금지 사항

- Slide 9-11 이외의 화면 요소 추가 금지

### 검증

```bash
dotnet build src/HnVue.UI/ 2>&1 | tail -5
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI/Views/WorkflowView.xaml
git add src/HnVue.UI/Views/WorkflowView.xaml.cs  # 필요 시
git commit -m "feat(design): UISPEC-006 WorkflowView PPT Slide 9-11 검증 및 개선"
git push origin team/team-design
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: WorkflowView 검증 | PARTIAL | 2026-04-12 | 재구현 필요 - NEEDS_VIEWMODEL 의뢰 |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |

### 세부 내용

**검증 결과:**
- 현재 WorkflowView.xaml (112줄): 좌측 제어 패널만 구현됨
- PPT Slide 9-11 요구사항: 3열 레이아웃 필요
  - Slide 9: 환자 정보 패널 + 워크플로우 상태
  - Slide 10: 이미지 획득 영역 (AcquisitionPreview)
  - Slide 11: Thumbnail strip

**구현 격차:**
- ❌ 환자 정보 패널 (우측)
- ❌ 중앙 Preview 영역 (AcquisitionPreview 컴포넌트)
- ❌ Thumbnail strip
- ✅ 좌측 제어 패널 (기존 구현 유지)

**NEEDS_VIEWMODEL 요청:**
WorkflowViewModel에 다음 속성 추가 필요:
1. **PreviewImage** 속성 (ImageSource) - AcquisitionPreview 바인딩
2. **ThumbnailList** 속성 (ObservableCollection<StudyThumbnail>)
3. **SelectedPatient** 속성 (PatientInfo) - 환자 정보 패널 바인딩
4. **WorkflowState** 속성 (enum) - IDLE/READY/EXPOSING 등

**Coordinator 의뢰 내용:**
- WorkflowView.xaml 재구성: 3열 Grid 레이아웃
  - 좌: 환자 정보 패널 (PatientInfoCard)
  - 중: AcquisitionPreview + Thumbnail strip
  - 우: 기존 제어 패널 (보존)
- WorkflowViewModel 속성 추가
- XAML data binding 설정

**빌드 결과:**
- XAML 컴파일: 성공 (기존 상태 유지)
- 오류: 0개
- 경고: StyleCop 경고만 (기존)

**준수율:**
- 현재: ~30% (제어패널만)
- 목표: PPT Slide 9-11 전체 구현 (~90%)
