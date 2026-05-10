---
id: SPEC-AUTODRIVE-GATE-001
type: acceptance
created: 2026-05-10
updated: 2026-05-10
parent: SPEC-METHODOLOGY-001
---

## Definition of Done

- [ ] 9개 요구사항(REQ-AUTOGATE-001~009) 모두 EARS 형식
- [ ] SPEC-GOVERNANCE-001 REQ-GOV-001(커밋 의무), REQ-GOV-006(QA 보고서) 흡수 매핑 명시
- [ ] Self-Verification 7항목 명세 (REQ-AUTOGATE-003)
- [ ] 사망 나선 감지 5 라운드 임계값 명시 (REQ-AUTOGATE-005)
- [ ] 빌드 범위 분기 표 (REQ-AUTOGATE-002) 작성
- [ ] 운영 흐름도 본 spec.md 포함

---

## 시나리오 1: COMPLETED 보고에 빌드 증거 누락 차단

**Given** Team B가 DISPATCH Task를 COMPLETED로 마킹하지만 Status 비고 열에 빌드 결과를 기재하지 않은 상태에서
**When** REQ-AUTOGATE-001 게이트가 동작하면
**Then** Status 변경이 거부되어야 하며, "전체 솔루션 빌드 결과 + 자기 소유 테스트 결과 + 변경 파일 목록 필수" 메시지를 반환한다.

검증 방법:
- DISPATCH Status 비고 열 grep: `dotnet build`, `passed`, `git diff` 키워드 모두 존재해야 함.
- 누락 발견 시 BLOCKED + Self-Verification 7항목 재실행 안내.

---

## 시나리오 2: Self-Verification 7번 항목 (실질 커밋) 차단

**Given** 팀이 6항목까지 통과했으나 7번 항목("실질 제품 커밋 1건 이상")이 No인 상태에서 (메타 작업만 수행)
**When** REQ-AUTOGATE-003 게이트가 동작하면
**Then** COMPLETED 보고가 차단되고, 사망 나선 감지(REQ-AUTOGATE-005) 카운터가 증가한다.

검증 방법:
- 팀의 라운드 동안 git log에서 소스/테스트/문서 변경 커밋 존재 여부 확인.
- 메타 키워드만 매치되면 7번 No로 판정.

---

## 시나리오 3: QA 재검증 실패 → IN_PROGRESS 회귀

**Given** Coordinator가 COMPLETED 보고했으나 QA 재검증에서 다음 중 하나가 발견된 상황:
- 빌드 증거가 실제 빌드 결과와 불일치
- 자기 소유권 외 파일 변경
- Self-Verification 흔적 누락

**When** REQ-AUTOGATE-004 게이트가 동작하면
**Then**:
- Coordinator의 해당 Task Status가 자동으로 COMPLETED → IN_PROGRESS로 회귀
- 사유가 DISPATCH 비고에 기록
- Coordinator에게 재작업 안내

검증 방법:
- DISPATCH Status 변경 이력에 회귀 기록 존재.
- QA TestReports/ 폴더에 회귀 사유 보고서 저장.

---

## 시나리오 4: 사망 나선 5 라운드 감지

**Given** S15~S16-R1 패턴 재현 — 5 라운드 연속으로 7팀 합산 실질 제품 커밋 0건
**When** REQ-AUTOGATE-005 감지가 동작하면
**Then**:
- CC가 사용자에게 "사망 나선 의심" 알림 발송
- 다음 DISPATCH 발행 정지
- 5 라운드 git log 분석 보고서 생성

검증 방법:
- `_CURRENT.md` 라운드 카운터에서 무효 라운드 5건 누적 확인.
- 사용자 알림 트리거 발화 (이슈 생성 또는 AskUserQuestion 요청).
- 보고서 파일 `.moai/reports/death-spiral-detection-{TS}.md` 존재.

---

## 시나리오 5: Safety-Critical 90% 미달 PR 블록

**Given** Team B가 Dose 모듈 변경이 포함된 PR을 생성했으나 라인 커버리지 87%인 상태에서
**When** REQ-AUTOGATE-006 게이트가 동작하면
**Then**:
- PR 머지가 블록됨
- `priority-critical` 레이블 이슈 자동 생성
- 사용자 알림 발화

검증 방법:
- QA TestReports/ 보고서에서 Dose 모듈 커버리지 추출.
- 90% 미달 시 PR 상태 changes_requested 자동 마킹.

---

## 시나리오 6: QA 보고서 파일 저장 검증

**Given** QA가 교차 리뷰/커버리지 측정/변이 테스트를 완료한 상태에서
**When** REQ-AUTOGATE-008 게이트가 동작하면
**Then** `TestReports/{REPORT-TYPE}_{YYYY-MM-DD}.md` 형식의 보고서 파일이 생성되어야 한다.

검증 방법:
- `ls TestReports/*_{TODAY}.md` ≥ 1
- 보고서 파일 콘텐츠가 콘솔 출력과 일치.

---

## 시나리오 7: DISPATCH 완료 미커밋 차단

**Given** 팀이 DISPATCH Status를 COMPLETED로 마킹하려 하지만 `team/{team}` 브랜치에 새 커밋이 없는 상태에서
**When** REQ-AUTOGATE-009 게이트가 동작하면
**Then** Status 변경이 거부되어야 하며, "최소 1개 커밋 필요" 메시지를 반환한다.

검증 방법:
- `git log origin/team/{team}..HEAD --oneline` ≥ 1
- 0건이면 BLOCKED + 팀에게 커밋 + push 안내.

---

## 엣지 케이스

- **빈 PR (코드 변경 없음)**: 문서/주석만 변경한 PR도 실질 커밋으로 인정. 7번 항목 통과.
- **메타 작업이 정당한 경우**: ScheduleWakeup 갱신이 헌법 변경(REQ-CONST-NNN) 적용을 위한 경우 → 사용자가 명시 예외 승인 가능.
- **QA 자체 사망 나선**: QA가 5 라운드 연속 검증 작업만 수행하면 무효 라운드 카운트 (QA 검증은 메타 작업이 아닌 실질 작업).

---

## Quality Gate

- 9개 요구사항이 quality-standards.md v1.3.0과 1:1 매핑 ✓
- Self-Verification 7번 항목 신규 추가 ✓
- 사망 나선 5 라운드 임계가 SPEC-CONSTITUTION-001 REQ-CONST-005와 일치 ✓
- 시간 예측 사용 0회 ✓
