# DISPATCH: Team B — S07 Round 4

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression v2) |
| **대상** | Team B |
| **브랜치** | team/team-b |
| **유형** | S07 R4 — Dicom 커버리지 보강 |
| **우선순위** | P2-Medium |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R4-team-b.md)만 Status 업데이트.

---

## 컨텍스트

S07-R3에서 Imaging 85%+ / Incident 90%+ / Detector 유지 완료.
현재 Dicom 모듈 커버리지가 ~70%로 85% 목표에 미달.
Safety-Adjacent 모듈(Workflow, Imaging)은 85%+ 달성 완료.

---

## 사전 확인

```bash
git checkout team/team-b
git pull origin main
```

---

## Task 1 (P1): Dicom 커버리지 ~70% → 85%

현재 HnVue.Dicom.Tests 401건. 커버리지 ~70%.

**우선 보강 영역**:
- C-STORE SCP 연결 수락/거부 시나리오
- MWL (Modality Worklist) 쿼리 응답 파싱
- DICOM 태그 유효성 검증 엣지 케이스
- Association Negotiation 타임아웃/에러 처리
- DICOM 파일 메타정보 읽기/쓰기 예외 경로

**목표**: Dicom 커버리지 85%+

---

## Task 2 (P2): 기타 모듈 커버리지 유지

S07-R3에서 달성한 커버리지 수준 유지:
- Detector 91.7%+
- Dose 99.5%+ (Safety-Critical)
- Incident 90%+ (Safety-Critical)
- Imaging 85%+
- Workflow 91.4%+
- PatientManagement 100%

필요시 리팩토링으로 인한 커버리지 저하 복구.

**목표**: 전 모듈 85%+ (Safety-Critical 90%+)

---

## Git 완료 프로토콜 [HARD]

```bash
git add src/HnVue.Dicom/ tests/HnVue.Dicom.Tests/
git commit -m "test(team-b): S07-R4 Dicom 커버리지 85% 달성 (#issue)"
git push origin team/team-b
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Dicom 커버리지 85% (P1) | NOT_STARTED | | |
| Task 2: 기타 모듈 유지 (P2) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |
