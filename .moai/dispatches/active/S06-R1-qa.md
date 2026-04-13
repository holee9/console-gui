# DISPATCH: QA Team — S06 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-13 |
| **발행자** | Commander Center |
| **대상** | QA Team |
| **브랜치** | team/qa |
| **유형** | S06 R1 — S05-R2 전체 빌드 검증 + 커버리지 리포트 갱신 |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S06-R1-qa.md)만 Status 업데이트.

---

## 컨텍스트

S05-R2 전원 MERGED 완료. main 브랜치에 Team B Dicom 86%, Team A PHI 암호화, Design WorkflowView,
Coordinator ViewModel 모두 반영됨. 전체 솔루션 빌드 + 테스트 검증 필요.

---

## 사전 확인

```bash
git checkout team/qa
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
```

---

## Task 1 (P1): 전체 솔루션 빌드 + 테스트 검증

1. `dotnet build HnVue.sln --configuration Release` — 0 errors 확인
2. `dotnet test HnVue.sln` — 전원 통과 확인
3. 총 테스트 수, 실패 수, 에러 수 기록

### 검증 결과를 DISPATCH Status에 기록

---

## Task 2 (P2): 모듈별 커버리지 리포트 갱신

1. Safety-Critical 모듈 (Dose, Incident, Security, Update) — 90%+ 확인
2. Standard 모듈 — 85%+ 확인
3. 전체 커버리지 요약 테이블 작성

---

## Git 완료 프로토콜 [HARD]

```bash
git add docs/testing/ TestReports/ 2>/dev/null
git commit -m "qa: S06-R1 전체 빌드 검증 + 커버리지 리포트"
git push origin team/qa
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: 전체 빌드+테스트 (P1) | NOT_STARTED | -- | 0 errors, all pass |
| Task 2: 커버리지 리포트 (P2) | NOT_STARTED | -- | 모듈별 현황 |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
