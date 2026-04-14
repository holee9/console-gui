# DISPATCH: Coordinator — S07 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression v2) |
| **대상** | Coordinator |
| **브랜치** | team/coordinator |
| **유형** | S07 R2 — 아키텍처 위반 수정 + 통합테스트 확대 |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R2-coordinator.md)만 Status 업데이트.

---

## 컨텍스트

S06-R2 Detector DI 조건부 등록 완료.
현재 StudyItem 클래스가 UI.Contracts에 구체 클래스로 존재 → 아키텍처 위반.
통합테스트 53개 통과 중이나 엣지케이스 커버리지 필요.

---

## 사전 확인

```bash
git checkout team/coordinator
git pull origin main
```

---

## Task 1 (P1): StudyItem 아키텍처 위반 수정

`HnVue.UI.Contracts.Models.StudyItem`이 구체 클래스.
UI.Contracts는 인터페이스만 포함해야 함.

옵션:
- A) StudyItem을 HnVue.UI 또는 HnVue.Common으로 이동
- B) IStudyItem 인터페이스로 변환 + 구현체 분리

아키텍처 테스트 통과하도록 수정.

---

## Task 2 (P2): 통합테스트 확대

현재 53개 통합테스트. 엣지케이스 추가:
- Detector SDK 연동 시나리오 (Simulator 어댑터)
- Workflow 상태 전이 크로스모듈 테스트
- Patient → Study → Image 체인 테스트
- Settings 변경 시 ViewModel 반영 테스트

목표: 70개 이상 통합테스트

---

## Task 3 (P3): ViewModel 검증

14개 ViewModel 전체 기능 검증:
- 모든 ICommand 바인딩 정상 동작
- PropertyChanged 이벤트 발생 확인
- DesignTime Mock과 런타임 데이터 일치

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.UI.Contracts/ src/HnVue.UI.ViewModels/ tests/
git commit -m "fix(coordinator): S07-R2 StudyItem 아키텍처 수정 + 통합테스트 확대 (#issue)"
git push origin team/coordinator
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: StudyItem 아키텍처 수정 (P1) | COMPLETED | 2026-04-14 | IStudyItem 인터페이스 + 구현체 분리, 아키텍처 테스트 11P 통과 |
| Task 2: 통합테스트 확대 53→70 (P2) | COMPLETED | 2026-04-14 | +16 크로스모듈 통합테스트 (Detector/Workflow/Chain/Settings) |
| Task 3: ViewModel 검증 (P3) | COMPLETED | 2026-04-14 | +29 ViewModel 검증테스트 (ICommand/PropertyChanged/Interface) |
| Git 완료 프로토콜 | COMPLETED | 2026-04-14 | push 완료 41df9ea, 빌드 0에러, UI 569P, Arch 11P |
