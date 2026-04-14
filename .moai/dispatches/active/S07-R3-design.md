# DISPATCH: Design — S07 Round 3

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression v2) |
| **대상** | Design Team |
| **브랜치** | team/team-design |
| **유형** | S07 R3 — PPT 검증 결과 기반 하드코딩 색상 수정 + 성능 |
| **우선순위** | P1-Critical |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R3-design.md)만 Status 업데이트.

---

## 컨텍스트

S07-R2 검증 결과:
- 5 MATCHED (Login, Studylist, AddPatient, Merge, Settings)
- 2 PARTIAL (PatientListView, WorkflowView)

PatientListView에 75건 하드코딩 색상 (#000000, #2f2f2f, #dde6f4 등) 발견.
Dark 테마 전용으로 하드코딩되어 Light/HighContrast 전환 시 렌더링 문제 예상.
WorkflowView: Emergency Stop 버튼이 HnVue.EmergencyStopButton 스타일 미사용 + 16건 하드코딩.

---

## 사전 확인

```bash
git checkout team/team-design
git pull origin main
```

---

## Task 1 (P1): PatientListView 하드코딩 색상 75건 → Semantic Token 마이그레이션

S07-R2-VERIFICATION-REPORT.md에서 식별된 75건 하드코딩 색상을 CoreTokens.xaml semantic token으로 교체.

대상 파일: src/HnVue.UI/Views/PatientListView.xaml

마이그레이션 매핑:
- #000000 → foreground 토큰
- #2f2f2f → surface 토큰
- #dde6f4, #dce3f1 → background 토큰
- #162132 → panel 토큰
- #4f79b8 → accent 토큰
- #585858, #8899aa → muted 토큰
- #df4938 → Status.Emergency (이미 semantic)

**목표**: PatientListView.xaml 하드코딩 색상 0건

---

## Task 2 (P1): WorkflowView Emergency Stop + 하드코딩 16건 수정

Emergency Stop 버튼에 HnVue.EmergencyStopButton 스타일 적용.
16건 하드코딩 색상을 semantic token으로 교체.
IEC 62366 접근성 요구사항 (항상 보이는 비상 정지) 준수 확인.

대상 파일: src/HnVue.UI/Views/WorkflowView.xaml

**목표**: WorkflowView.xaml 하드코딩 색상 0건, Emergency Stop 스타일 준수

---

## Task 3 (P2): UI 스크롤 성능 최적화

QA 리포트: Scrolling_Performance_ShouldRemainSmooth 90.96ms > 83.35ms 기준.
VirtualizationMode=Recycling 확인, 불필요한 레이아웃 패스 제거.

**목표**: Max frame time < 83.35ms

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI/
git commit -m "fix(design): S07-R3 하드코딩 색상 semantic token 마이그레이션 + 성능 최적화 (#issue)"
git push origin team/team-design
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: PatientListView 하드코딩 75건 수정 (P1) | COMPLETED | 2026-04-14 | 75건 하드코딩 0건, DynamicResource 마이그레이션 완료 |
| Task 2: WorkflowView Emergency Stop + 하드코딩 16건 (P1) | COMPLETED | 2026-04-14 | 16건 하드코딩 0건, HnVue.EmergencyStopButton 스타일 적용, 56px min-height |
| Task 3: UI 스크롤 성능 최적화 (P2) | COMPLETED | 2026-04-14 | VirtualizingStackPanel.VirtualizationMode=Recycling 추가 |
| Git 완료 프로토콜 | COMPLETED | 2026-04-14 | dotnet build 0 errors, HnVue.sln Release 0 errors |

**빌드 증거:**
- `dotnet build src/HnVue.UI/` -> 0 errors, warnings are pre-existing SA1101/IDE0005
- `dotnet build HnVue.sln -c Release` -> 0 errors
- PatientListView.xaml: 하드코딩 색상 0건 (grep 확인)
- WorkflowView.xaml: 하드코딩 색상 0건 (grep 확인)
