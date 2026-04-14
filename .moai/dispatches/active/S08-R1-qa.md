# DISPATCH: QA — S08 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center |
| **대상** | QA |
| **브랜치** | team/qa |
| **유형** | S08 R1 — StudylistView 구현 후 품질검증 |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S08-R1-qa.md)만 Status 업데이트.

---

## 컨텍스트

S08-R1은 StudylistView (PPT slides 5-7) 구현 라운드.
Coordinator와 Design이 StudylistViewModel + XAML 구현 후 QA가 품질 검증.
**QA는 전팀 COMPLETED 후 검증 시작.**

---

## 사전 확인

```bash
git checkout team/qa
git pull origin main
```

---

## Task 1 (P1): 전체 품질게이트 검증

Coordinator + Design 작업 main 반영 후 전체 검증.

**수행 사항**:
- `dotnet build` 0에러 확인
- `dotnet test` 전체 실행 — 기존 2539테스트 + 신규 테스트
- 아키텍처 테스트 11+건 통과 확인 (새 인터페이스 추가로 증가 가능)
- 모듈 경계 검증 (Design이 금지 모듈 참조 없는지)

**목표**: 빌드 0에러, 테스트 0실패, 아키텍처 통과

---

## Task 2 (P2): 커버리지 리포트

신규 코드 반영 커버리지 측정.

**수행 사항**:
- Cobertura 커버리지 수집
- 신규 모듈(ViewModel, View) 커버리지 확인
- Safety-Critical 90%+ 유지 확인

**목표**: 전 모듈 85%+, Safety-Critical 90%+

---

## Git 완료 프로토콜 [HARD]

```bash
git add docs/testing/ TestReports/
git commit -m "docs(qa): S08-R1 품질게이트 + 커버리지 리포트 (#issue)"
git push origin team/qa
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: 품질게이트 검증 (P1) | NOT_STARTED | | |
| Task 2: 커버리지 리포트 (P2) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |
