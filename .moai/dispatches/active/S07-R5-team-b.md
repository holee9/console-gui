# DISPATCH: Team B — S07 Round 5

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression v2) |
| **대상** | Team B |
| **브랜치** | team/team-b |
| **유형** | S07 R5 — 커버리지 갭 해소 + 의료모듈 안정화 |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R5-team-b.md)만 Status 업데이트.

---

## 컨텍스트

S07-R4 QA 검증 결과: 평균 89.4%, 3개 모듈 85% 미만.
Team B는 Dicom, Imaging 등 의료 모듈 커버리지 확인 후 85%+ 보강.

---

## 사전 확인

```bash
git checkout team/team-b
git pull origin main
```

---

## Task 1 (P1): 의료 모듈 커버리지 현황 확인 및 보강

QA 리포트에서 85% 미만 모듈 중 Team B 소유 모듈(Dicom, Detector, Imaging, Dose, Incident, Workflow, PatientManagement, CDBurning) 확인.

**수행 사항**:
- 각 모듈 현재 커버리지 측정
- 85% 미만 모듈 식별
- 부족한 테스트 케이스 보강 (엣지 케이스, 예외 경로)
- Safety-Critical(Dose, Incident) 90%+ 확인

**목표**: 전 모듈 85%+, Safety-Critical 90%+

---

## Task 2 (P2): 전체 빌드/테스트 검증

커버리지 보강 후 전체 솔루션 빌드/테스트로 회귀 없음 확인.

**수행 사항**:
- `dotnet build` 0에러 확인
- `dotnet test` 전체 실행
- 신규/수정 테스트 모두 통과 확인

**목표**: 빌드 0에러, 테스트 0실패

---

## Git 완료 프로토콜 [HARD]

```bash
git add [수정 파일]
git commit -m "feat(team-b): 커버리지 보강 + 의료모듈 안정화 (#issue)"
git push origin team/team-b
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: 커버리지 보강 (P1) | NOT_STARTED | | |
| Task 2: 빌드/테스트 검증 (P2) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |
