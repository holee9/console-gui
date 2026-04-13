# DISPATCH: QA — S07 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression) |
| **대상** | QA |
| **브랜치** | team/qa |
| **유형** | S07 R1 — 전체 커버리지 현황 리포트 |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R1-qa.md)만 Status 업데이트.

---

## 컨텍스트

S06-R2 완료 후 전체 솔루션 빌드 안정성 및 커버리지 현황 점검 필요.
S07-R1에서 각 팀이 커버리지 작업 진행 예정이므로, QA는 베이스라인 측정 선행.

---

## 사전 확인

```bash
git checkout team/qa
git pull origin main
```

---

## Task 1 (P1): 전체 솔루션 빌드 + 테스트 현황 리포트

```bash
# 전체 빌드
dotnet build HnVue.sln --configuration Release 2>&1 | tail -20

# 모듈별 테스트 + 커버리지
for module in Detector Dicom Dose Incident Workflow Imaging CDBurning Security Update Common Data PatientManagement SystemAdmin; do
  echo "=== $module ==="
  dotnet test tests/HnVue.$module.Tests/ --collect:"XPlat Code Coverage" --configuration Release 2>&1 | grep -E "통과|실패|Passed|Failed" | tail -2
done
```

### 리포트 항목
1. 전체 빌드 에러/경고 수
2. 모듈별 테스트 통과/실패 수
3. 모듈별 라인 커버리지 (%)
4. Safety-Critical 모듈(Dose, Incident, Security, Update) 90% 달성 여부
5. 85% 미달 모듈 목록

---

## Task 2 (P2): 통합테스트 실행

```bash
dotnet test tests.integration/ --configuration Release 2>&1 | tail -10
```

---

## Git 완료 프로토콜 [HARD]

```bash
git add TestReports/
git commit -m "report(qa): S07-R1 전체 커버리지 현황 리포트 (#issue)"
git push origin team/qa
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: 전체 커버리지 리포트 (P1) | NOT_STARTED | -- | |
| Task 2: 통합테스트 (P2) | NOT_STARTED | -- | |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
