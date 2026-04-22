# S16-R2 Reset Plan — 프로세스 사망 나선 해소 + 실질 개발 재시작

## 진단 (Diagnosis)

### 증상
- S13-R1 ~ S16-R1 (4개 Sprint, 약 14회 라운드) 동안 실질 제품 개발 0건
- 모든 커밋이 ScheduleWakeup / IDLE CONFIRM / 프로토콜 패치
- DISPATCH 템플릿이 "살아있음 확인"으로 자기복제 중

### 최종 실질 커밋
- `1617350 fix(team-a): S14-R2 SecurityCoverageBoostV2Tests 87개 Trait 누락 수정 (#199)` (S14-R2)

### 근본 원인
CC의 DISPATCH 기획이 제품 로드맵(SPEC/MRD/PRD)에서 분리되어 자기보존 프로토콜 유지관리로 귀결.

---

## 재정비 전략 (Reset Strategy)

### 원칙
1. **제품 중심 회귀**: 모든 DISPATCH는 기존 SPEC 또는 제품 로드맵의 실질 작업을 근거로 함
2. **P0-Blocker 우선**: SPEC-INFRA-002, SPEC-COORDINATOR-001 즉시 착수
3. **기존 SPEC 실행**: 대기 중인 준비된 SPEC부터 집행
4. **품질 게이트 회복**: QA CONDITIONAL PASS(99.47%) 해소

### S16-R2 팀별 업무 분장

| 팀 | 업무 | 근거 SPEC / 문서 |
|----|------|----------------|
| Team A | SPEC-INFRA-002 plan/acceptance/tasks 작성 + AesGcmPhiEncryptionService 구현 착수 | SPEC-INFRA-002 (P0-Blocker) |
| Team B | SPEC-TEAMB-COV-001 실행 — Safety-Critical(Dose, Incident) 90% 커버리지 달성 | SPEC-TEAMB-COV-001 (준비 완료) |
| Coordinator | SPEC-COORDINATOR-001 plan/acceptance/tasks 작성 + NullRepository → EF Core 교체 착수 | SPEC-COORDINATOR-001 (P0-Blocker) |
| Design | UISPEC-002 PatientListView (44% → 95%), UISPEC-003 StudylistView (63% → 95%) | SPEC-UI-001 / UISPEC-002, UISPEC-003 |
| QA | 전체 커버리지 재측정 + Safety-Critical 90% 검증 + CONDITIONAL PASS(99.47%) 해소 | Quality Standards |
| RA | DOC-042 CMP Draft → v2.0 승격 + SPEC-GOVERNANCE-001 실행 | RA Priority + SPEC-GOVERNANCE-001 |

### 성공 기준 (Exit Criteria)

S16-R2 완료 조건:
- SPEC-INFRA-002: plan/acceptance/tasks 3개 파일 작성 + AesGcmPhiEncryptionService 최소 1개 구현 단계 진입
- SPEC-TEAMB-COV-001: Dose 90%+ 또는 Incident 90%+ 달성 (둘 중 1개 완료)
- SPEC-COORDINATOR-001: plan/acceptance/tasks 3개 파일 작성 + EfDoseRepository 또는 EfWorklistRepository 1개 구현
- Design: PatientListView 또는 StudylistView 중 1개 화면 준수도 90%+
- QA: 전체 테스트 재측정 + Safety-Critical 현황 보고
- RA: CMP v2.0 Draft 1차 작성 완료

### 프로세스 안정화 (CC 측면)

- [ ] `reactive-coalescing-fox.md` 내용을 team-common.md CC Monitoring Protocol에 병합
- [ ] IDLE CONFIRM 전용 DISPATCH 템플릿 폐기 (실질 업무 없을 때만 IDLE 보고)
- [ ] 매 라운드 DISPATCH 기획 시 "근거 SPEC/문서" 필드 의무화
- [ ] 3라운드 연속 실질 커밋 0건 발생 시 자동 에스컬레이션

---

## 병합 순서 (Phase 순서)

### Phase 1 (동시 시작) — SPEC 계획 작성
- Team A: SPEC-INFRA-002 plan/acceptance/tasks
- Coordinator: SPEC-COORDINATOR-001 plan/acceptance/tasks

### Phase 2 (Phase 1 완료 후) — 구현 착수
- Team A: AesGcmPhiEncryptionService 구현 시작
- Coordinator: EF Repository 1개 이상 구현 시작
- Team B: SPEC-TEAMB-COV-001 실행 (Phase 1과 병행 가능)
- Design: UISPEC 준수도 개선 (Phase 1과 병행 가능)
- RA: DOC-042 CMP 작성 (Phase 1과 병행 가능)

### Phase 3 (전팀 완료 후) — 품질 검증
- QA: 전체 커버리지 재측정 + Safety-Critical 90% 검증

---

## CC 모니터링 강화

- CronCreate 10분 주기 유지 (이미 활성)
- 매 라운드 종료 시 "실질 커밋 수" 추적 (Trait/프로토콜/빌드 증거 분리)
- 2라운드 연속 실질 커밋 0건 → 경고, 3라운드 연속 → 사용자 에스컬레이션

---

Version: 1.0.0
Author: MoAI Commander Center (ultrathink reset)
Created: 2026-04-22
Effective: S16-R2 ACTIVE
