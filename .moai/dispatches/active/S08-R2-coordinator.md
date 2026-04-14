# DISPATCH: Coordinator — S08 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center |
| **대상** | Coordinator |
| **브랜치** | team/coordinator |
| **유형** | S08 R2 — DI 등록 누락 보완 + 통합테스트 |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S08-R2-coordinator.md)만 Status 업데이트.

---

## 컨텍스트

S08-R1 StudylistView 구현 완료(QA PASS). S08-R2에서 누락된 DI 등록 보완.

---

## 사전 확인

```bash
git checkout team/coordinator
git pull origin main
```

---

## Task 1 (P1): 누락 DI 등록 보완

ViewModel DI 등록 누락 항목 보완.

**누락 항목**:
- `IAddPatientProcedureViewModel` → `AddPatientProcedureViewModel`
- `IMainViewModel` → `MainViewModel`

**수행 사항**:
- `src/HnVue.App/App.xaml.cs`에 두 ViewModel DI 등록 추가
- 기존 패턴(`services.AddTransient<IXxxViewModel, XxxViewModel>()`) 준수

**목표**: 모든 UI.Contracts 인터페이스에 DI 등록 완료

---

## Task 2 (P2): 통합테스트 보완

DI 등록 검증 통합테스트 추가.

**수행 사항**:
- `tests.integration/CoordinatorIntegrationTests.cs`에 DI 해결 검증 테스트 추가
- `IAddPatientProcedureViewModel` 및 `IMainViewModel` 서비스 프로바이더에서 해결 가능한지 확인

**목표**: DI 등록 누락 자동 감지 테스트 구축

---

## 파일 소유권 (role-matrix v2.0)

| 파일 | 소유 | 비고 |
|------|------|------|
| `src/HnVue.App/App.xaml.cs` | Coordinator | DI 등록 |
| `tests.integration/**` | Coordinator | 통합테스트 |
| `src/HnVue.UI/DesignTime/**` | **Design** | **수정 금지** |

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.App/ tests.integration/
git commit -m "feat(coordinator): S08-R2 DI 등록 누락 보완 + 통합테스트 (#issue)"
git push origin team/coordinator
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: DI 등록 보완 (P1) | COMPLETED | 2026-04-14 | AddPatientProcedureViewModel DI 등록 추가. IMainViewModel 기존 Singleton 등록 확인 |
| Task 2: 통합테스트 (P2) | COMPLETED | 2026-04-14 | DI_ResolveAllViewModels에 IAddPatientProcedureViewModel 검증 추가. 전체 55P/0F 통과 |
| Git 완료 프로토콜 | IN_PROGRESS | | |
