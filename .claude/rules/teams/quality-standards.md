# Quality Standards [SINGLE SOURCE OF TRUTH]

HnVue 프로젝트의 품질 지표 정의. **모든 품질 기준은 이 문서에서 정의**되며,
다른 문서(qa.md, team-*.md, DISPATCH 등)는 이 표를 참조만 한다.

이 문서는 `team-common.md`에서 분리되었다 (2026-04-22 재정비).

---

## 1. Project Philosophy [CONSTITUTIONAL]

> **"Speed is not the goal. Quality and completeness are."**

- [HARD] Completeness first: 3 tasks at 100% > 10 tasks at 80%
- [HARD] Self-verification required: prove "0 errors" — assume 금지
- [HARD] No false reports: unverified COMPLETED = 프로토콜 위반
- [HARD] Scope compliance: only do what DISPATCH instructs
- [HARD] Evidence-based: all completion claims include build logs + test results

---

## 2. Quality Metrics Table [SSOT]

| Metric | Minimum | Safety-Critical | Notes |
|--------|---------|----------------|-------|
| **Build** | 0 errors | 0 errors, 0 warnings | `dotnet build HnVue.sln -c Release` |
| **Tests** | All pass | All pass | `dotnet test` |
| **Line Coverage** | 85% | 90%+ | Safety-Critical: Dose, Incident, Security, Update |
| **SonarCloud Bug** | 0 | 0 | Blocker on PR |
| **SonarCloud Vulnerability** | 0 | 0 | Blocker on PR |
| **SonarCloud Code Smell** | <50 | <50 | Warning, not blocker |
| **Stryker Mutation Score** | N/A | ≥70% | Safety-Critical 모듈만 |
| **OWASP CVSS** | <7.0 | <7.0 | ≥7.0 triggers build failure |

### Safety-Critical 모듈 정의

- **HnVue.Dose** (IEC 60601-2-54 인터락)
- **HnVue.Incident** (사고 보고)
- **HnVue.Security** (인증, PHI 암호화)
- **HnVue.Update** (스테이지드 업데이트)

### Safety-Adjacent (85% 이상, RA 검토 권장)

- **HnVue.Imaging** — 렌더링 오류가 진단 해석 영향
- **HnVue.Workflow** — 상태 머신 오류가 환자 안전 시퀀스 영향

---

## 3. Self-Verification Checklist [HARD — Before COMPLETED]

DISPATCH 작업 완료 보고 전 모두 검증:

- [ ] All Task acceptance criteria met?
- [ ] **전체 솔루션 빌드** `dotnet build HnVue.sln` 0 errors confirmed? (모듈 빌드만으로는 의존성 회귀 검증 불가)
- [ ] **자기 소유 테스트** `dotnet test` all passed confirmed? (자기 소유 테스트 프로젝트만 — 타팀 테스트는 QA가 검증)
- [ ] Only modified files within ownership scope? (`git diff --name-only` 확인)
- [ ] DISPATCH Status 테이블에 build evidence 기재?
- [ ] Incomplete items honestly marked as PARTIAL?
- [ ] ScheduleWakeup(읽은 값) 재설정 완료? (session-lifecycle.md 참조)

### 빌드 범위 기준 [HARD — S14-R2 교훈]

| 팀 | 빌드 범위 | 이유 |
|----|----------|------|
| Team A | **HnVue.sln 전체** | 인프라 모듈 변경이 전체에 영향 |
| Team B | **HnVue.sln 전체** | 의료 도메인 모듈이 다른 모듈 참조 |
| Coordinator | **HnVue.sln 전체** | DI/통합 변경이 전체에 영향 |
| Design | **HnVue.UI 프로젝트** | UI는 독립적, 전체 빌드 불필요 |
| QA | **HnVue.sln 전체** | 검증 기관이므로 전체 범위 |
| RA | **빌드 불필요** | 문서만 작업 |

- [HARD] 구현팀(TA, TB, CO)은 반드시 **전체 솔루션 빌드**로 자기 변경이 다른 모듈에 회귀를 일으키지 않는지 확인
- [HARD] 모듈 빌드만 수행하고 COMPLETED 보고 = Self-Verification 위반

---

## 4. Evidence Required for COMPLETED

DISPATCH Status 비고 열에 아래 3개 증거 필수:

1. **Build 결과**: `dotnet build` 요약 문자열 (errors/warnings 개수)
2. **Test 결과**: 추가/수정한 테스트의 PASS/FAIL 수치
3. **변경 파일 목록**: `git diff --name-only` 출력 (소유권 확인용)

Safety-Critical 모듈 수정 시 추가:
- Stryker Mutation Score (이전 vs 현재)
- Characterization test 유지 확인 (기존 동작 불변)

---

## 5. QA 독립성 [CONSTITUTIONAL]

- [HARD] QA의 PASS/FAIL 판정은 **최종** — 사용자 승인 없이 번복 불가
- [HARD] QA는 구현에 관여하지 않고 검증에만 관여
- [HARD] 소유 도구: `dotnet build/test`, Coverlet, Stryker, SonarCloud, StyleCop, OWASP 도구

---

## 6. CI 품질 게이트 연동

GitHub Actions / Gitea CI에서 다음 게이트 강제:

- [ ] PR에 `team-*` 라벨 없으면 → 블록
- [ ] NetArchTest 실패 → 블록
- [ ] StyleCop 새 경고 → 블록 (기존 경고는 별도 트래킹)
- [ ] Line Coverage 해당 모듈 임계값 미달 → 경고
- [ ] Safety-Critical 모듈 Stryker <70% → 블록
- [ ] OWASP CVSS ≥7.0 → 블록

---

Version: 1.3.0 (빌드 범위 기준 명확화 — 전체 솔루션 빌드 의무화)
Effective: 2026-04-22
Cross-ref: `qa.md`, `dispatch-protocol.md`, `role-matrix.md`
