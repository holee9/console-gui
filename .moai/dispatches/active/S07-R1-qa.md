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
| Task 1: 전체 커버리지 리포트 (P1) | COMPLETED | 2026-04-14 | Build 0E, 2187/2192P (Data 5F), Coverage ~80.2%, SC: Dose 96.5%/Security 95.6% PASS, Incident 70.2%/Update 75.7% FAIL |
| Task 2: 통합테스트 (P2) | COMPLETED | 2026-04-14 | Integration 53/53 Pass. Architecture 10/11 (StudyItem violation). |
| Git 완료 프로토콜 | COMPLETED | 2026-04-14 | Push: 3d09f55 team/qa |

### Build Evidence
- `dotnet build HnVue.sln -c Release`: 0 errors, 14796 warnings (StyleCop/IDE in test projects)
- Unit Tests: 2192 total, 2187 pass, 5 fail (Data: EfCdStudyRepository + EfDoseRepository)
- Integration Tests: 53/53 pass
- Architecture Tests: 10/11 pass (1 violation: StudyItem concrete class in UI.Contracts)

### Coverage Baseline (S07-R1)

| Module | Coverage | Target | Status |
|--------|----------|--------|--------|
| Dose (SC) | 96.5% | >=90% | PASS |
| Security (SC) | 95.6% | >=90% | PASS |
| Incident (SC) | 70.2% | >=90% | FAIL (-19.8%) |
| Update (SC) | 75.7% | >=90% | FAIL (-14.3%) |
| PatientMgmt | 97.8% | >=85% | PASS |
| CDBurning | 100.0% | >=85% | PASS |
| Imaging | 88.1% | >=85% | PASS |
| Dicom | 86.0% | >=85% | PASS |
| Workflow | 85.5% | >=85% | PASS |
| Common | 83.9% | >=85% | FAIL (-1.1%) |
| Detector | 73.9% | >=85% | FAIL (-11.1%) |
| Data | 47.4% | >=85% | FAIL (-37.6%) |
| SystemAdmin | 66.7% | >=85% | FAIL (-18.3%) |
