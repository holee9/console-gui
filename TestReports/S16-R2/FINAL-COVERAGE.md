# S16-R2 QA Coverage Report (Final)

**측정 일시**: 2026-04-22
**빌드 결과**: 0 errors, 22860 warnings (StyleCop informational)
**전체 테스트**: 4,754 PASS / 13 FAIL / 4,768 총계 (99.73% 성공률)

---

## Safety-Critical 90% Gate (Final Verdict)

| Module | Coverage | Target | Status | 비고 |
|--------|----------|--------|--------|------|
| HnVue.Dose | 100.00% (100/100) | 90% | PASS | S16-R2 Team B Dose 90%+ 목표 달성 (DISPATCH 승인) |
| HnVue.Incident | 95.24% (80/84) | 90% | PASS | 유지 |
| HnVue.Security | 89.62% (397/443) | 90% | **FAIL** | 0.38%p 미달. `priority-high` 이슈 필요 |
| HnVue.Update | 96.00% (168/175) | 90% | PASS | 단독 재측정으로 확인 |

**Safety-Critical Gate: 3/4 PASS (75%)**

---

## Standard Module 85% Gate

| Module | Coverage | Target | Status |
|--------|----------|--------|--------|
| HnVue.Common | 48.74% (116/238) | 85% | FAIL |
| HnVue.Data | 90.41% (424/469) | 85% | PASS |
| HnVue.Dicom | 54.10% (211/390) | 85% | FAIL |
| HnVue.Detector | 89.19% (66/74) | 85% | PASS |
| HnVue.Imaging | 92.49% (579/626) | 85% | PASS |
| HnVue.Workflow | 91.48% (204/223) | 85% | PASS |
| HnVue.PatientManagement | 100.00% (14/14) | 85% | PASS |
| HnVue.CDBurning | 100.00% (14/14) | 85% | PASS |
| HnVue.SystemAdmin | 62.90% (39/62) | 85% | FAIL |
| HnVue.UI | 88.86% (814/916) | 85% | PASS |
| HnVue.UI.Contracts | 100.00% (7/7) | 85% | PASS |
| HnVue.UI.ViewModels | 92.64% (403/435) | 85% | PASS |

**Standard Gate: 9/12 PASS (75%)**

---

## Overall Verdict

| 영역 | 판정 |
|------|------|
| 빌드 (0 errors) | PASS |
| 전체 테스트 (99.73% pass rate, 13 failures in 4768) | **CONDITIONAL PASS** |
| Safety-Critical 커버리지 | **FAIL** (Security 89.62% < 90%) |
| Standard 커버리지 | FAIL (Common/Dicom/SystemAdmin 85% 미달) |

**S16-R2 최종 판정: CONDITIONAL PASS**
- Dose/Incident/Update 3개 Safety-Critical 모듈 90%+ 달성 확인
- Security 0.38%p 미달 — Team A에 priority-high 이슈 통지 필요
- 13개 실패 테스트는 S14-R2 17개 대비 4개 감소 (회복 중)

---

## 실패 테스트 13개 (모듈별 분포)

| 모듈 | 실패 수 | 비고 |
|------|---------|------|
| HnVue.UI.Tests | 4 | placeholder SaveAsync 관련 2개, 기타 |
| HnVue.Imaging.Tests | 1 | 성능 테스트 (5000ms 제한 초과 — 13211ms) |
| HnVue.Update.Tests | 6 | 병렬 실행 시만 발생, 단독 실행 시 317/317 전체 통과 → **flaky** |
| HnVue.SystemAdmin.Tests | 1 | backups to contain 2 item(s), found 0 |
| HnVue.IntegrationTests | 1 | — |

**Update.Tests 6개는 Coverlet 병렬 instrumentation issue에 의한 flaky failure로 재측정 시 PASS.**

---

## CONDITIONAL PASS 해소 분석 (T3)

S14-R2: 4107 PASS / 4124 total = 17 failures, 99.47% success
S16-R2: 4754 PASS / 4768 total = 13 failures, 99.73% success (+0.26%p)

**회복된 테스트: 4개**
**미해결 테스트: 13개**

최종 판정: **CONDITIONAL PASS 유지** (100% 회복은 아니지만 방향성 개선)
