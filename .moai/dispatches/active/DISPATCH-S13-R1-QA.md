# DISPATCH - QA (S13-R1)

> **Sprint**: S13 | **Round**: 1 | **팀**: QA (Quality Assurance)
> **발행일**: 2026-04-19
> **상태**: NOT_STARTED

---

## 1. 작업 개요

S13-R1 진입 게이트 평가 + 전체 빌드/테스트/커버리지 검증.

## 2. 작업 범위

### Task 1: S13-R1 진입 게이트 평가

**목표**: 전체 솔루션 빌드/테스트/커버리지 기준 확인

- `dotnet build HnVue.sln -c Release` → 0 errors
- `dotnet test HnVue.sln` → 전체 통과
- Safety-Critical 커버리지 확인: Dose, Incident, Update, Security 90%+
- 전체 모듈 커버리지 85%+ 확인
- Architecture Tests 11/11 통과 확인

### Task 2: 커버리지 리포트 생성

**목표**: S13-R1 기준 커버리지 리포트 생성

- Coverlet 리포트 생성
- 모듈별 커버리지 상세 분석
- Safety-Critical 모듈 90%+ 게이트 판정
- TestReports/에 리포트 저장

### Task 3: 아키텍처 테스트 검증

**목표**: NetArchTest 아키텍처 규칙 준수 확인

- 모듈 의존성 규칙 11개 전체 검증
- DesignTime/ 디렉토리 접근 규칙 검증
- 팀간 소유권 위반 검출
- 결과 보고

---

## 3. DISPATCH Status

| 작업 ID | 설명 | 상태 | 할당자 | 우선순위 | 비고 |
|---------|------|------|--------|----------|------|
| T1 | 진입 게이트 평가 | NOT_STARTED | QA | P0 | 빌드/테스트/커버리지 |
| T2 | 커버리지 리포트 | NOT_STARTED | QA | P1 | 모듈별 상세 분석 |
| T3 | 아키텍처 테스트 | NOT_STARTED | QA | P1 | 11/11 통과 확인 |

---

## 4. 완료 조건

- [ ] 전체 빌드 0 errors
- [ ] 전체 테스트 0 failures
- [ ] 커버리지 리포트 TestReports/ 저장
- [ ] Architecture Tests 통과
- [ ] PASS/CONDITIONAL PASS/FAIL 판정
- [ ] DISPATCH Status COMPLETED + 빌드 증거

---

## 5. Build Evidence

_(작업 완료 후 기록)_

---

## 6. 비고

- QA 판정은 최종 — CC가 뒤집을 수 없음
- 이슈 등록: 커버리지 < 85% → `qa-result` + `priority-high` 라벨
- 이슈 등록: 변이 점수 < 70% → `qa-result` + `priority-medium` 라벨
