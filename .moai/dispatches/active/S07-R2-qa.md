# DISPATCH: QA — S07 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression v2) |
| **대상** | QA |
| **브랜치** | team/qa |
| **유형** | S07 R2 — 품질게이트 통과 + CI/CD 커버리지 게이트 |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R2-qa.md)만 Status 업데이트.

---

## 컨텍스트

S07-R1 전체 커버리지 리포트 완료 (2192P/5F).
5개 Data 테스트 실패 + StudyItem 아키텍처 위반 감지.
S07-R2에서는 Team A/Coordinator 수정 후 품질게이트 재검증.

---

## 사전 확인

```bash
git checkout team/qa
git pull origin main
```

---

## Task 1 (P1): S07-R2 전체 품질게이트 재검증

Team A (Data/Update/Common/SystemAdmin) + Team B (Incident/Detector) +
Coordinator (StudyItem/통합테스트) 수정 후:

- `dotnet build` 0 errors 확인
- `dotnet test` ALL passed 확인 (5F → 0F 목표)
- 아키텍처 테스트 통과 확인
- 전체 커버리지 85% 이상 확인

---

## Task 2 (P2): CI/CD 커버리지 게이트 강화

.github/workflows/desktop-ci.yml에:
- 커버리지 85% 미만 시 빌드 실패 게이트 추가
- Safety-Critical 모듈 90% 미만 시 빌드 실패 게이트 추가
- 커버리지 리포트 아티팩트 업로드

---

## Task 3 (P3): S07-R2 전체 커버리지 리포트

최종 커버리지 현황 리포트:
- 모듈별 커버리지 (목표 vs 실제)
- Safety-Critical 모듈 개별 현황
- 전체 테스트 통과/실패 카운트
- S07-R1 대비 개선 추이

---

## Git 완료 프로토콜 [HARD]

```bash
git add .github/workflows/ TestReports/ scripts/ci/
git commit -m "ci(qa): S07-R2 품질게이트 통과 + CI 커버리지 게이트 (#issue)"
git push origin team/qa
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: 전체 품질게이트 재검증 (P1) | NOT_STARTED | | |
| Task 2: CI/CD 커버리지 게이트 (P2) | NOT_STARTED | | |
| Task 3: 전체 커버리지 리포트 (P3) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |
