# SPEC-GOVERNANCE-001 Progress Tracking

**SPEC ID**: SPEC-GOVERNANCE-001
**Title**: 팀 개발 거버넌스 강제화 — 워크트리 규율·이슈 추적·한글 인코딩
**Created**: 2026-04-09 (UltraThink 감사 결과 기반)
**Status**: IN_PROGRESS (2026-04-22 S16-R2 착수)
**Last Updated**: 2026-04-22

---

## 진행 이력

| 날짜 | 이벤트 | 상태 | 담당 |
|------|--------|------|------|
| 2026-04-09 | SPEC 초안 작성 (UltraThink 독립 감사) | draft | MoAI |
| 2026-04-22 | S16-R2 RA 팀 DISPATCH T2 착수 | in_progress | RA |
| 2026-04-22 | S17-R1 T1 완료: 추적성 감사 (갭 없음 확인) | completed | RA |
| 2026-04-22 | 수용 기준 활성화: 시나리오 1 빌드 증거 연동 강화 | in_progress | RA |

---

## 수용 기준 진행 상태

### 시나리오 1: Dispatch 커밋 의무 검증
- **기대**: team/team-b 커밋 존재 확인
- **현황**: **충족** - Git Protocol 강화됨 (team-common.md §8)
- **증거**:
  - `Git Completion Protocol` 규칙 명문화 (team-common.md)
  - `git add → commit → push` 프로세스 강제
  - DISPATCH Status 업데이트 타임스탬프 의무화
  - S17-R1 DISPATCH 빌드 증거 요구 (Evidence Required 섹션 명시)
- **S17-R1 확인**: RA DISPATCH Status 업데이트 시 빌드 증거 요구 완전 준수

### 시나리오 2: 한글 인코딩 표시 정합성
- **기대**: `core.quotepath=false` 설정
- **현황**: **충족** - .gitattributes에 설정 완료
- **증거**: `.gitattributes`에 `core.quotepath=false` 명시

### 시나리오 3: Team A SA* 억제 적용
- **기대**: GlobalSuppressions 적용
- **현황**: **미확인** - Team A 전담 영역
- **갭**: Team A DISPATCH 확인 필요

### 시나리오 4: Dispatch 수명주기 완전 이행
- **기대**: active/ ↔ completed/ 이행
- **현황**: **부분 충족** - CC 전용 관리 프로세스 확립
- **증거**:
  - CC 머지 후 active/ → completed/ 이동 규칙 (team-common.md)
  - _CURRENT.md MERGED 상태 관리
- **갭**: 라운드 종료 시 자동 정리 프로세스 강화 필요

### 시나리오 5: 규칙 문서 업데이트
- **기대**: [HARD] 커밋 의무, Issue-First Policy 문서화
- **현황**: **충족** - team-common.md 개정 완료
- **증거**:
  - `Issue Tracking Protocol [HARD]` (team-common.md §18)
  - `DISPATCH Status Update Protocol [HARD]` (team-common.md §22)
  - `Git Completion Protocol [HARD]` (team-common.md §17)

### 시나리오 6: 릴리즈 블로커 이슈 등록
- **기대**: R-001, R-002 Gitea 이슈 생성
- **현황**: **미확인** - 이슈 추적 필요
- **갭**: RA 팀 이슈 생성 태스크 포함 필요

---

## SPEC 생성 이후 주요 변경사항 (2026-04-09 ~ 2026-04-22)

### 시스템 재정비 (2026-04-22)
- **문서**: SYSTEM-REFORM-2026-04-22.md
- **변경**: team-common.md 564행 → 5개 핵심 파일 분해
- **영향**: SPEC-GOVERNANCE-001 요구사항이 team-common.md에 통합 반영됨
  - Dispatch Resolution Protocol [HARD]
  - Git Completion Protocol [HARD]
  - Issue Tracking Protocol [HARD]
  - CC Auto-Progression Protocol [HARD]

### role-matrix.md 도입 (S09-R3 사고교훈)
- **CONSTITUTIONAL** 문서: 7팀 역할 경계 최상위 규약
- **강제 규칙**: CC 자가점검 5문항, 팀 자가점검 6문항
- **SPEC-GOVERNANCE-001 정합성**: 시나리오 1, 4, 5 충족

### 한글 인코딩 해결 (feedback_gitea_encoding.md)
- **문제**: curl 인라인 한글 깨짐 (U+FFFD)
- **해결**: scripts/issue/gitea-api.sh 파일 경유 방식 도입

---

## 다음 단계 (Action Items)

1. **[P1]** 시나리오 6 이슈 생성: R-001, R-002 Gitea 이슈 등록 (RA 또는 해당 팀)
2. **[P2]** 시나리오 3 확인: Team A GlobalSuppressions 적용 상태 확인
3. **[P2]** 시나리오 4 정리: Dispatch 수명주기 자동화 프로세스 검토
4. **[P3]** 진입 기록 정기 갱신: 매 라운드 종료 시 progress.md 업데이트

---

## 비고

- 이 SPEC은 2026-04-09 독립 감사(UltraThink) 결과 기반으로 작성되었습니다.
- 2026-04-22 시스템 재정비(SYSTEM-REFORM-2026-04-22.md)로 인해 대부분 요구사항이 team-common.md에 통합 반영되었습니다.
- 현재 진행 중인 S16-R2 라운드에서 이 SPEC의 이행 상태를 재점검하고 있습니다.
