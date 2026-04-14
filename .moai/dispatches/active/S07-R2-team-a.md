# DISPATCH: Team A — S07 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression v2) |
| **대상** | Team A |
| **브랜치** | team/team-a |
| **유형** | S07 R2 — 커버리지 85% 달성 (Data/SystemAdmin/Update/Common) |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R2-team-a.md)만 Status 업데이트.

---

## 컨텍스트

S07-R1에서 Security 95.6% 달성. Data 모듈은 47.4%로 심각한 갭.
5개 Data 테스트 실패가 전체 품질 게이트를 blocking.

---

## 사전 확인

```bash
git checkout team/team-a
git pull origin main
```

---

## Task 1 (P1): HnVue.Data 커버리지 47.4% → 85%

현재 1800/3798 라인 커버. 추가 필요:
- Repository async 메서드 테스트 (GetAllAsync, GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync)
- DbContext CRUD 시나리오
- SQLCipher 연결/암호화 테스트
- 트랜잭션 (IUnitOfWork) 테스트

**목표**: 최소 85% (3228/3798 라인)

---

## Task 2 (P1): Data 테스트 실패 5건 수정

현재 5개 테스트 실패 (entity tracking, FK constraints, null validation).
원인 분석 후 수정.

---

## Task 3 (P2): HnVue.SystemAdmin 커버리지 66.7% → 85%

288/432 라인 커버. 추가 필요:
- 관리자 설정 CRUD 테스트
- 시스템 로그 조회 테스트
- 권한 관리 테스트

---

## Task 4 (P2): HnVue.Update 커버리지 75.7% → 90% (Safety-Critical)

904/1194 라인 커버. Safety-Critical 모듈이므로 90% 목표.
- 업데이트 체크/다운로드/설치 흐름 테스트
- 롤백 시나리오 테스트
- 버전 비교 로직 테스트

---

## Task 5 (P3): HnVue.Common 커버리지 83.9% → 85%

530/632 라인 커버. 1.1% 갭만 메우면 됨.
- Result<T> 모나드 엣지케이스
- 공통 유틸리티 테스트

---

## Git 완료 프로토콜 [HARD]

```bash
git add tests/ src/HnVue.Data/ src/HnVue.SystemAdmin/ src/HnVue.Update/ src/HnVue.Common/
git commit -m "test(team-a): S07-R2 Data 85% + SystemAdmin 85% + Update 90% + Common 85% (#issue)"
git push origin team/team-a
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Data 커버리지 85% (P1) | NOT_STARTED | | |
| Task 2: Data 테스트 실패 수정 (P1) | NOT_STARTED | | |
| Task 3: SystemAdmin 커버리지 85% (P2) | NOT_STARTED | | |
| Task 4: Update 커버리지 90% (P2) | NOT_STARTED | | |
| Task 5: Common 커버리지 85% (P3) | NOT_STARTED | | |
| Git 완료 프로토콜 | NOT_STARTED | | |
