# HnVue 개발운영 지침서 (Development Operations Guidelines)

Version: 1.0.0
Last Updated: 2026-04-10
Classification: HARD (전 팀 필수 준수)

---

## 1. DISPATCH 라이프사이클

### 1.1 발행 규칙 (Commander Center)

- DISPATCH 발행 시 반드시 `.claude/rules/teams/{team}.md` 참조
- 필수 섹션: 팀 역할 재확인, Constraints, Git Completion Protocol
- 팀별 규칙에서 파생된 제약조건 명시
- DISPATCH 스키마 준수: `.claude/rules/moai/workflow/dispatch-schema.md`

### 1.2 실행 규칙 (각 팀)

- Task 순서 준수 (P0 -> P1 -> P2 -> P3)
- 체크박스 업데이트는 검증 완료 후에만
- Status는 실제 상태만 기록 (허위 보고 금지)
- 실행 중 타팀 파일 수정 필요 발견 시 즉시 이슈 생성

### 1.3 완료 규칙 (각 팀) [HARD]

COMPLETED 보고 전 필수:

1. 자기 소유 모듈 빌드 성공 (`dotnet build` 또는 MSBuild) -- **HARD**
2. 자기 소유 테스트 전원 통과 -- **HARD**
3. 전체 솔루션 빌드 시도 -- **SHOULD** (타팀 오류는 보고서에 명시)
4. 빌드 출력 마지막 줄을 Status에 복사 -- **HARD**

빌드 증거 없는 COMPLETED = 허위 보고로 간주한다.

---

## 2. Git Completion Protocol [HARD]

### 2.1 완료 후 필수 절차

1. `git add` (DISPATCH.md + 변경 파일, 민감파일 제외)
2. `git commit` (conventional commit 형식)
3. `git push origin team/{team-name}`
4. PR 생성 (main 대상, Gitea API)
5. PR URL을 DISPATCH Status에 기록

### 2.2 PR 생성 규칙

- 기존 open PR 확인 후 중복 방지
- push 실패 시 "PUSH_FAILED" 상태로 보고 (교착 금지)
- 커밋 메시지는 DISPATCH 제목에서 파생

### 2.3 .gitignore 정책

다음 개발 환경 아티팩트는 절대 커밋 금지:

| 패턴 | 설명 |
|------|------|
| `temp_ppt_extract/` | PPT 추출 임시 파일 |
| `.dotnet-home/` | NuGet 캐시 (2068+ 파일) |
| `tmp/` | 임시 작업 디렉토리 |
| `_workspace*/` | 워크트리 작업 디렉토리 |
| `*.user` | VS 사용자 설정 |
| `bin/`, `obj/` | 빌드 출력 |

새 도구 도입 시 해당 도구의 아티팩트 패턴을 `.gitignore`에 추가 필수.

---

## 3. Commander Center 통합 검증 [HARD]

### 3.1 분산 검증 + 중앙 수집 (재설계안)

- 각 팀이 자체 빌드 검증 -> Status에 결과 기록
- Commander Center는 Status 확인만 (직접 빌드 안 함)
- 전원 PASS -> 머지 순서 결정 -> 순차 머지
- FAIL 있으면 해당 팀에만 수정 지시

### 3.2 머지 순서 규칙

| 순서 | 팀 | 이유 |
|------|-----|------|
| 1 | QA/RA | 소스 변경 없음, 안전 |
| 2 | Design | UI 전용, 타 모듈 의존성 낮음 |
| 3 | Team A | 인프라 (Common, Data, Security) |
| 4 | Team B | 의료 (Dicom, Detector, Dose 등) |
| 5 | Coordinator | 통합 (App, ViewModels, Contracts), 마지막 |

### 3.3 머지 전 체크리스트

- [ ] 6팀 DISPATCH Status 전부 COMPLETED 확인
- [ ] 각 팀 빌드 증거 확인 (Status에 기록된 빌드 출력)
- [ ] PR mergeable 상태 확인
- [ ] 충돌 팀은 rebase 후 재push
- [ ] 머지 후 전체 솔루션 빌드 검증

---

## 4. 팀별 역할 경계 [HARD]

### 4.1 소유권 규칙

- 각 팀은 자기 소유 모듈 파일만 수정
- 타팀 파일 수정 필요 시 이슈 생성 + 해당 팀 알림
- UI.Contracts 인터페이스: Coordinator만 수정 (SOLE modifier)

팀별 소유 모듈:

| 팀 | 소유 모듈 |
|----|----------|
| Team A | Common, Data, Security, SystemAdmin, Update |
| Team B | Dicom, Detector, Imaging, Dose, Incident, Workflow, PatientManagement, CDBurning |
| Design | UI/Views, Styles, Themes, Components, Converters, Assets, DesignTime |
| Coordinator | UI.Contracts, UI.ViewModels, App |
| QA | .github/workflows, scripts/ci, scripts/qa, TestReports |
| RA | docs/regulatory, docs/planning, docs/risk, docs/verification, scripts/ra |

### 4.2 교차 의존성 프로토콜

| 변경 유형 | 필수 조치 |
|----------|----------|
| Common 인터페이스 변경 | `breaking-change` 이슈 + Coordinator 알림 |
| NuGet 패키지 변경 | `soup-update` 이슈 + RA 알림 |
| Safety-critical 소스 수정 | characterization test 선행 + RA 위험평가 |
| Workflow state 변경 | RA RTM 업데이트 이슈 |
| UI.Contracts 인터페이스 추가 | Coordinator만 수정, impact analysis 필수 |
| DB 스키마 변경 | Team A + Coordinator 알림 |

---

## 5. 사고 사례 및 교훈

### 5.1 Round 1 사고 (2026-04-10)

| 팀 | 위반 | 근본 원인 | 재발 방지 |
|----|------|----------|----------|
| Team B | COMPLETED 허위 보고 (0/16 체크박스 미완) | 빌드 검증 없이 상태 변경 | 완료 규칙 1.3 HARD |
| Design | 14MB 오염 커밋 (temp_ppt_extract/ + .dotnet-home/) | .gitignore 미비 | .gitignore 정책 2.3 |
| Team A | 생성자 불일치 미감지 | 모듈별 빌드만 수행, 전체 솔루션 빌드 누락 | 전체 솔루션 빌드 SHOULD |
| 4팀 | 미push, PR 미생성 | git 완료 절차 부재 | Git Protocol 2.1 HARD |
| Commander | 팀 규칙 미반영 DISPATCH 발행 | DISPATCH 스키마 없음 | 발행 규칙 1.1 + 스키마 제정 |
| Commander | 빌드 미검증 머지 수락 | 신뢰 기반 수용 | 통합 검증 3.1 HARD |

### 5.2 Commander Center 자체 검증 실패 (2026-04-10, 동일 세션)

| 시점 | 주장 | 실제 |
|------|------|------|
| 교차검증 보고 | "15/15 PASS" | Design DISPATCH에 Team B 내용 오염, 5팀 필수 섹션 누락 |
| 원인 | `grep -c` 키워드 존재만 확인 | 파일 내용을 실제로 읽지 않음 |

이는 Team B 허위 보고와 동일 구조: 확인 없이 완료 보고.

### 5.3 Commander Center 자체 검증 규칙 [HARD]

Commander Center가 "PASS/완료/검증 통과" 보고 전 필수:

1. **Read 검증**: grep count가 아닌 실제 파일 내용을 Read로 확인 -- HARD
2. **제목 정합성**: 각 팀 DISPATCH 제목이 해당 팀과 일치하는지 head -1 확인 -- HARD
3. **섹션 실재**: 필수 섹션의 라인 번호를 grep -n으로 확인 (존재 유무가 아닌 위치) -- HARD
4. **교차 참조**: 스키마에서 요구하는 필드가 DISPATCH에 실제로 존재하는지 1:1 대조 -- HARD
5. **자기 보고 의심**: "전원 PASS"는 의심 신호. 최소 2개 파일은 전문 읽기로 확인 -- HARD

위반 시: 사용자에게 "미확인 항목 있음"으로 보고. 확인 완료된 항목만 PASS 보고.

### 5.4 교훈 요약

1. **빌드 증거 없는 완료 보고는 허위 보고다** -- 가장 빈번하고 치명적인 위반
2. **.gitignore는 도구 도입 시점에 업데이트해야 한다** -- 사후 정리는 고비용
3. **전체 솔루션 빌드는 교차 의존성 오류의 유일한 탐지 수단이다**
4. **Git push + PR은 작업의 최종 완료 행위다** -- 커밋만으로는 불완전
5. **Commander Center는 검증자이지 신뢰자가 아니다** -- 증거 기반 수용

---

## 6. 참조 문서

| 문서 | 경로 |
|------|------|
| 팀 규칙 | `.claude/rules/teams/{team}.md` |
| DISPATCH 스키마 | `.claude/rules/moai/workflow/dispatch-schema.md` |
| 에이전트 정의 | `.claude/agents/hnvue/` |
| CI/CD | `.github/workflows/desktop-ci.yml` |
| 운영 전략 가이드 | `docs/OPERATIONS.md` |
| Git 워크플로우 | `docs/development/git-workflow.md` |
