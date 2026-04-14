# DISPATCH: Team B — S07 Round 3

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression v2) |
| **대상** | Team B |
| **브랜치** | team/team-b |
| **유형** | S07 R3 — Imaging 테스트 + Incident/Detector 검증 |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R3-team-b.md)만 Status 업데이트.

---

## 컨텍스트

S07-R2에서 Detector/Dose/PatientManagement 커버리지 보강 완료 (PR #96).
하지만 HnVue.Imaging 모듈은 QA 리포트에서 테스트 0건으로 나타남.
Incident 커버리지는 70.2%에서 시작했으나, S07-R2에서 추가 테스트 없이 Team B가 Detector/Dose/PM에 집중함.

---

## 사전 확인

```bash
git checkout team/team-b
git pull origin main
```

---

## Task 1 (P1): HnVue.Imaging 테스트 커버리지 0% → 85%

QA 리포트에서 Imaging 모듈 테스트가 0건으로 보고됨.
이미지 렌더링, 변환, 포맷 처리 등 핵심 로직 테스트 필요.
Safety-Adjacent 모듈이므로 85%+ 필요.

추가 필요:
- 이미지 로드/저장 기본 테스트
- 포맷 변환 (DICOM → display) 테스트
- 렌더링 파이프라인 테스트
- 예외 처리 (corrupt image, unsupported format)

**목표**: 최소 85% 커버리지

---

## Task 2 (P1): HnVue.Incident 커버리지 검증

S07-R1에서 70.2%였음. S07-R2 DISPATCH에서 90% 목표였으나 Incident 테스트가 추가되지 않음.
현재 커버리지 확인 후 90% 미달 시 추가 테스트 작성.

추가 필요 (90% 미달 시):
- 인시던트 CRUD 전체 시나리오
- 심각도 분류 로직
- 상태 전이 (Open → Investigating → Resolved → Closed)
- 감사 로그 연동

**목표**: 최소 90% (Safety-Critical)

---

## Task 3 (P2): HnVue.Detector 커버리지 유지

S07-R2에서 DetectorFinalCoverageTests 238LoC 추가.
병합 후 커버리지 유지 확인. 새 테스트와 충돌 없는지 검증.

**목표**: 85%+ 유지

---

## Git 완료 프로토콜 [HARD]

```bash
git add tests/ src/HnVue.Imaging/ src/HnVue.Incident/
git commit -m "test(team-b): S07-R3 Imaging 85% + Incident 90% + Detector 유지 (#issue)"
git push origin team/team-b
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Imaging 커버리지 0% → 85% (P1) | COMPLETED | 2026-04-14 | ImagingCoverageAdditionalTests 23 신규, 총 77 통과 |
| Task 2: Incident 커버리지 검증 → 90% (P1) | COMPLETED | 2026-04-14 | EfIncidentRepositoryTests 25 신규 (SQLite), 총 138 통과 |
| Task 3: Detector 커버리지 유지 (P2) | COMPLETED | 2026-04-14 | DetectorCoverageAdditionalTests 25 신규, 총 272 통과 |
| Git 완료 프로토콜 | COMPLETED | 2026-04-14 | commit a272c74 pushed to team/team-b |
