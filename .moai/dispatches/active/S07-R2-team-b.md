# DISPATCH: Team B — S07 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression v2) |
| **대상** | Team B |
| **브랜치** | team/team-b |
| **유형** | S07 R2 — Safety-Critical Incident 90% + Detector 85% |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R2-team-b.md)만 Status 업데이트.

---

## 컨텍스트

S07-R1에서 Dicom 91.9%, Workflow 88.1%, Detector 99.1% 달성.
하지만 Incident(70.2%)가 Safety-Critical 기준(90%) 미달.
Detector SDK 어댑터는 달성했으나 전체 모듈 커버리지는 73.9%로 갭 존재.

---

## 사전 확인

```bash
git checkout team/team-b
git pull origin main
```

---

## Task 1 (P1): HnVue.Incident 커버리지 70.2% → 90% (Safety-Critical)

400/570 라인 커버. Safety-Critical 모듈.
추가 필요:
- 인시던트 생성/조회/수정/삭제 CRUD 테스트
- 심각도 분류 로직 (Critical/Major/Minor)
- 알림 발송 시나리오
- 감사 로그 연동 테스트
- 인시던트 상태 전이 (Open → Investigating → Resolved → Closed)

**목표**: 최소 90% (513/570 라인)

---

## Task 2 (P2): HnVue.Detector 커버리지 73.9% → 85%

300/406 라인 커버. SDK 어댑터는 완성, 전체 모듈 커버리지 갭.
추가 필요:
- DetectorConnection lifecycle 테스트
- 예외 처리 시나리오 (timeout, disconnect, invalid config)
- Simulator 어댑터 엣지케이스

---

## Task 3 (P3): HnVue.Workflow 커버리지 유지 (85.5%)

현재 85.5%로 타겟 충족하지만 경계선.
새 테스트 추가 시 기존 커버리지 유지 확인.

---

## Git 완료 프로토콜 [HARD]

```bash
git add tests/ src/HnVue.Incident/ src/HnVue.Detector/
git commit -m "test(team-b): S07-R2 Incident 90% + Detector 85% (#issue)"
git push origin team/team-b
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Incident 커버리지 90% (P1) | NOT_STARTED | | |
| Task 2: Detector 커버리지 85% (P2) | NOT_STARTED | | |
| Task 3: Workflow 커버리지 유지 (P3) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |
