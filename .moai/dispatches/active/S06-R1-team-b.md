# DISPATCH: Team B — S06 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-13 |
| **발행자** | Commander Center |
| **대상** | Team B |
| **브랜치** | team/team-b |
| **유형** | S06 R1 — Dicom 커버리지 유지보수 + CDBurning 테스트 보강 |
| **우선순위** | P2-Medium |
| **SPEC 참조** | SPEC-TEAMB-COV-001 (implemented, 유지보수) |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S06-R1-team-b.md)만 Status 업데이트.

---

## 컨텍스트

SPEC-TEAMB-COV-001 implemented 달성. Dicom 86%, Detector 91.7%, Dose 99.5%, PM 100%.
남은 갭: CDBurning 모듈 테스트 커버리지가 목표치 미달 가능성.

---

## 사전 확인

```bash
git checkout team/team-b
git pull origin main
git checkout -- .
git clean -fd --exclude=".moai/reports/"
```

---

## Task 1 (P2): CDBurning 테스트 커버리지 확인 및 보강

### 범위

1. CDBurning 모듈 현재 커버리지 측정
2. 85% 미달 시 테스트 추가
3. 기본 CRUD + 에러 경로 테스트

### 검증

```bash
dotnet test tests/HnVue.CDBurning.Tests/ --collect:"XPlat Code Coverage" 2>&1 | tail -10
```

---

## Task 2 (P3): Dicom 커버리지 회귀 방지

1. Dicom 86% 유지 확인
2. 신규 테스트와 기존 테스트 충돌 여부 점검

---

## Git 완료 프로토콜 [HARD]

```bash
git add tests/HnVue.CDBurning.Tests/
git commit -m "feat(team-b): CDBurning 테스트 커버리지 보강"
git push origin team/team-b
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: CDBurning 커버리지 (P2) | NOT_STARTED | -- | 85% 목표 |
| Task 2: Dicom 회귀 방지 (P3) | NOT_STARTED | -- | 86% 유지 |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
