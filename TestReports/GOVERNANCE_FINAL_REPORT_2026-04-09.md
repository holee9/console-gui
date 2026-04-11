# SPEC-GOVERNANCE-001 최종 거버넌스 감사 보고서

**날짜**: 2026-04-09  
**작성**: MoAI 커맨더센터 (main 브랜치)  
**감사 범위**: 04-08 Dispatch 완료 검증 + 거버넌스 위반 시정 + 교차검증 전체  
**최종 상태**: ✅ 모든 시정 조치 완료

---

## 1. 감사 배경

### 문제 발단
QA 팀의 교차 리뷰 결과, 각 팀이 "DISPATCH 완료" 보고를 했으나 실제 git commit이 없는 상태임이 확인되었다. 이 외에도 다음 거버넌스 위반이 탐지되었다:

| 위반 코드 | 위반 내용 | 탐지 경로 |
|-----------|-----------|-----------|
| V-001 | 팀 브랜치에 git commit 없이 COMPLETE 상태 보고 | QA 교차 리뷰 |
| V-002 | 이슈 미등록 상태로 작업 실행 (Issue-First 위반) | 거버넌스 감사 |
| V-003 | 한글 파일명 깨짐 (core.quotepath=true 기본값) | git status 검사 |
| V-004 | QA 보고서 파일 미저장 (콘솔 출력만) | 파일 시스템 검사 |
| V-005 | Dispatch 규칙 문서에 Commit Discipline 항목 없음 | 규칙 문서 검토 |
| V-006 | Branch Hierarchy 규칙 미문서화 | 거버넌스 감사 |

---

## 2. 시정 조치 완료 내역

### 2.1 긴급 시정 (즉시 실행)

| 조치 | 결과 | 커밋/증거 |
|------|------|-----------|
| 5개 팀 worktree git commit 생성 | ✅ 완료 | 아래 표 참조 |
| git core.quotepath=false 설정 | ✅ 완료 | `git config core.quotepath false` |
| .gitattributes UTF-8 정책 추가 | ✅ 완료 | main 0e706ce |
| QA 교차 리뷰 보고서 파일 저장 | ✅ 완료 | TestReports/QA_CROSS_REVIEW_REPORT_2026-04-09.md |
| PatientEntity 암호화 Gitea 이슈 등록 | ✅ 완료 | Issue #61 |
| Null Repository 스텁 Gitea 이슈 등록 | ✅ 완료 | Issue #62 |

### 2.2 팀 브랜치 커밋 현황

| 팀 브랜치 | 커밋 해시 | 커밋 메시지 요약 |
|-----------|-----------|-----------------|
| team/coordinator | 3e1d937 | 테스트 안정화, SCS0005 억제, UI.Contracts 수정 |
| team/qa | 1713655 | CI 인프라 로컬 전환, Roslyn 분석기 통합 |
| team/team-a | 7705d02 | NuGet 취약성 해소, GlobalSuppressions 5개 모듈 |
| team/team-b | 138301c | GlobalSuppressions 8개 모듈 적용 |
| team/ra | 8aee32a | DOC-042 CMP v1.1, DOC-019 SBOM v1.1 |
| team/team-design | 완료 전 커밋 존재 | e21d352, d5149f7 |

### 2.3 규칙 문서 시정

| 문서 | 추가 내용 |
|------|-----------|
| dispatch-system.md | ## Commit Discipline [HARD] — COMPLETE 전 commit 필수 |
| dispatch-system.md | ## Issue-First Policy [HARD] — 이슈 먼저 등록 의무화 |
| dispatch-system.md | ## Branch Hierarchy [HARD] — 브랜치 계층 구조 명시 |
| dispatch-system.md | ## QA Report Persistence [HARD] — 보고서 파일 저장 의무 |
| worktree-integration.md | ## Project Branch Hierarchy [HARD] — 머지 흐름 다이어그램 추가 |

### 2.4 Team A GlobalSuppressions.cs 생성

Team A가 5개 모듈에 GlobalSuppressions.cs 파일을 누락한 상태였음. 전체 생성 완료:

| 모듈 | 파일 경로 | 상태 |
|------|-----------|------|
| HnVue.Common | src/HnVue.Common/GlobalSuppressions.cs | ✅ 생성 |
| HnVue.Data | src/HnVue.Data/GlobalSuppressions.cs | ✅ 생성 |
| HnVue.Security | src/HnVue.Security/GlobalSuppressions.cs | ✅ 생성 (보수적 적용) |
| HnVue.SystemAdmin | src/HnVue.SystemAdmin/GlobalSuppressions.cs | ✅ 생성 |
| HnVue.Update | src/HnVue.Update/GlobalSuppressions.cs | ✅ 생성 |

---

## 3. 교차검증 결과 (8/8 PASS)

| 항목 | 검증 내용 | 결과 |
|------|-----------|------|
| V-001 | 5개 팀 브랜치에 commit 존재 | ✅ PASS |
| V-002 | GlobalSuppressions.cs 5개 파일 존재 | ✅ PASS |
| V-003 | .gitattributes UTF-8 규칙 포함 | ✅ PASS |
| V-004 | dispatch-system.md에 4개 HARD 섹션 | ✅ PASS |
| V-005 | QA 교차 리뷰 보고서 파일 존재 | ✅ PASS |
| V-006 | Gitea 이슈 #61, #62 유효 | ✅ PASS |
| V-007 | SPEC-GOVERNANCE-001 spec/plan/acceptance 존재 | ✅ PASS |
| V-008 | git core.quotepath=false 설정됨 | ✅ PASS |

---

## 4. 미해결 릴리즈 블로커

아래 항목은 시정 조치는 완료했으나 **구현이 아직 필요한** 릴리즈 블로커다.

### R-001: PatientEntity AES-256-GCM 암호화 [CRITICAL]
- **이슈**: #61 (security + priority-critical + release-blocker)
- **관련 요구사항**: SWR-CS-080
- **현황**: @MX:TODO 태그 존재, 실제 암호화 코드 미구현
- **책임팀**: Team A (HnVue.Data)
- **영향**: HIPAA/GDPR 규정 위반 → 릴리즈 불가

### R-002: Null Repository 스텁 6개 [HIGH]
- **이슈**: #62 (team-a + team-b + priority-high + infrastructure)
- **현황**: App.xaml.cs에 NullDoseRepository 등 6개 스텁
- **책임팀**: Team A + Team B 협력
- **영향**: Phase 1 스텁 의도적 → 실 DI 전환 필요

### R-003: 커버리지 게이트 미달 [HIGH]
- **현황**: 전체 75.6% (목표 85%), Dose 브랜치 67.6% (목표 90% HARD GATE)
- **04-09 Dispatch 목표**: 80% 중간 게이트 달성
- **상태**: 04-09 Dispatch 아직 PENDING — 실행 필요

---

## 5. 팀별 오류 재발방지책

### 5.1 Team A (Infrastructure & Foundation)

**04-08 위반**: GlobalSuppressions.cs 5개 모듈 누락, NuGet 취약성 미점검

**재발방지책**:
1. **체크리스트 의무화**: 새 모듈 생성 시 GlobalSuppressions.cs 포함 여부를 PR 체크리스트에 추가
2. **NuGet 보안 스캔**: Dispatch 완료 전 `dotnet list package --vulnerable` 실행을 작업 완료 조건으로 명시
3. **PR 자동 검증**: .csproj 추가 시 Directory.Packages.props 업데이트 확인, SOUP 알림 트리거
4. **Issue-First 준수**: 모든 Dispatch 작업 시작 전 Gitea 이슈 번호 확인 후 착수
5. **Commit-Before-COMPLETE**: Dispatch 상태를 COMPLETE로 변경하기 전 `git log main..HEAD --oneline` 출력으로 커밋 존재 반드시 확인

### 5.2 Team B (Medical Imaging Pipeline)

**04-08 위반**: 작업 완료 미커밋 (dirty working tree 상태), SCS0005 pragma 임시 억제

**재발방지책**:
1. **Commit Discipline 자동화**: Dispatch 실행 스크립트에 `git status --porcelain` 빈값 확인 추가 — dirty tree 상태면 실행 불가
2. **SCS0005 pragma 사용 금지**: GlobalSuppressions.cs 방식으로 일원화, 코드 내 pragma 금지 팀 규칙 추가
3. **안전-크리티컬 변경 프로세스**: Dose/Incident 모듈 변경 시 RA 이슈 먼저 등록, 90% 브랜치 커버리지 게이트 통과 후 PR
4. **워크트리 격리 준수**: 팀 간 파일 소유권 경계 위반 시 즉시 Coordinator 알림
5. **패치 사이클 단축**: 매 Dispatch 완료 시 `dotnet test --collect:"XPlat Code Coverage"` 실행하여 커버리지 누락 즉시 감지

### 5.3 Coordinator (Integration Gate)

**04-08 위반**: Team A/B PR 리뷰 없이 작업 진행 (브랜치 계층 역할 미이행)

**재발방지책**:
1. **Gate 역할 명확화**: Coordinator는 team-a/b 작업을 리뷰만 하며 직접 구현 금지. PR 리뷰 체크리스트를 DISPATCH에 명시
2. **PR 리뷰 의무화**: team-a, team-b가 PR 생성하면 Coordinator 리뷰 전 main 머지 불가 (CODEOWNERS 설정)
3. **DI 등록 검증**: 모든 PR에서 App.xaml.cs DI 등록 완정성 확인, integration test 통과 조건 추가
4. **인터페이스 변경 알림**: UI.Contracts 변경 시 `interface-contract` 레이블 이슈 자동 생성 후 팀 전체 알림
5. **통합 테스트 CI 게이트**: HnVue.IntegrationTests 전체 통과를 PR 머지 필수 조건으로 설정

### 5.4 QA Team (Quality Gate)

**04-08 위반**: 보고서 파일 미저장 (콘솔 출력만), 보고 내용과 실제 상태 불일치

**재발방지책**:
1. **보고서 파일 저장 자동화**: QA Dispatch 완료 기준에 `TestReports/{TYPE}_{YYYY-MM-DD}.md` 파일 생성 포함
2. **파일 존재 검증**: Dispatch 완료 보고 전 `ls TestReports/` 명령으로 파일 물리적 존재 확인
3. **독립 검증 강화**: 팀 완료 보고 후 QA가 실제 파일 변경, 커밋, 빌드 성공 독립 검증 — 선언이 아닌 실증
4. **커버리지 측정 표준화**: `dotnet test --collect:"XPlat Code Coverage"` 실행 후 coverlet 결과로 실제 수치 보고
5. **릴리즈 게이트 자동화**: `scripts/qa/Generate-ReleaseReport.ps1` 실행 결과를 TestReports에 자동 저장

### 5.5 RA Team (Regulatory Affairs)

**04-08 성과**: DOC-042 CMP v1.1, DOC-019 SBOM v1.1 (42→45개) 업데이트 완료 — 양호  
**잠재 위험**: CMP 미완성, RMP v2.0 업데이트 예정, 이슈 등록 없이 작업 시작 관행

**재발방지책**:
1. **Issue-First 철저 준수**: 모든 규제 문서 업데이트 전 `ra-update` 레이블 이슈 등록 후 착수
2. **문서 버전 관리**: 마이너 수정도 개정 이력 테이블에 날짜/변경내용 기록 의무화
3. **SBOM 트리거 자동화**: NuGet 패키지 변경 이슈에 `soup-update` 레이블 부착 시 RA에 알림 — 수동 추적 대신 이슈 기반
4. **DOC-042 완료 시한**: CMP Draft → v1.0 완성을 다음 Dispatch에서 최우선 과제로 명시
5. **RTM 추적 갱신**: 코드 변경마다 RTM (DOC-032) 갱신 여부 Dispatch 완료 조건에 포함

### 5.6 Team Design (UI Design)

**04-08 성과**: LoginView + PatientListView PPT 리디자인 완료  
**잠재 위험**: PPT 범위 초과 위험 (이슈 #59 재발 방지), 이슈 미등록

**재발방지책**:
1. **PPT 범위 준수 체크리스트**: 구현 완료 후 PPT 지정 페이지 번호와 대조 검증 — 지정 외 요소 포함 시 즉시 제거
2. **5-Phase 워크플로우 준수**: DESIGN_TO_XAML_WORKFLOW.md Phase 1→5 단계 완료 확인 후 COMPLETE 보고
3. **Issue-First 준수**: 디자인 작업 시작 전 Gitea 이슈 등록, DesignTime/ Mock 먼저 구현
4. **IEC 62366 접근성 자체검사**: 컬러 대비 4.5:1 이상, 터치 타겟 44×44px 이상 측정 후 보고서에 포함
5. **Emergency Stop 변경 특별 프로세스**: 위치/크기 변경 시 QA/RA 리뷰 이슈 등록 필수

---

## 6. 브랜치 계층 및 머지 전략

```
main (MoAI 커맨더센터) ← 구현 직접 금지
  ├─ team/coordinator  ← team-a/b 통합 게이트
  │    ├─ team/team-a  (Infrastructure)  → coordinator 리뷰 → main
  │    └─ team/team-b  (Medical Imaging) → coordinator 리뷰 → main
  ├─ team/qa           → 독립 PR → main
  ├─ team/ra           → 독립 PR → main
  └─ team/team-design  → 독립 PR → main
```

**현재 머지 준비 상태**:

| 팀 | 커밋 상태 | 커버리지 게이트 | 머지 가능 여부 |
|----|-----------|-----------------|---------------|
| team/coordinator | 1 commit ahead | 검증 필요 | 조건부 (통합 테스트 통과 후) |
| team/qa | 1 commit ahead | N/A (QA 소유) | Coordinator 리뷰 불필요 → PR 가능 |
| team/team-a | 1 commit ahead | 85% 미달 (75.6%) | **불가** — 커버리지 해소 후 |
| team/team-b | 1 commit ahead | 85% 미달, Dose 90% 미달 | **불가** — 안전크리티컬 게이트 미달 |
| team/ra | 1 commit ahead | N/A (문서 팀) | PR 가능 (CMP 완성 조건) |
| team/team-design | 기존 커밋 존재 | N/A (UI 팀) | PR 가능 (PPT 범위 확인 후) |

---

## 7. 다음 단계 (04-09 Dispatch PENDING)

아래 5개 Dispatch가 PENDING 상태로 실행 대기 중:

| Dispatch | 팀 | 핵심 목표 |
|----------|----|-----------|
| DISPATCH-COORDINATOR-2026-04-09 | coordinator | Team A/B PR 리뷰 + 통합 검증 |
| DISPATCH-DESIGN-2026-04-09 | team-design | 나머지 화면 PPT 리디자인 |
| DISPATCH-QA-2026-04-09 | qa | 커버리지 80% 중간 게이트 달성 |
| DISPATCH-TEAM-A-2026-04-09 | team-a | 커버리지 85% 목표 + R-001 PatientEntity 암호화 |
| DISPATCH-TEAM-B-2026-04-09 | team-b | Dose 90% HARD GATE 달성 |

---

## 8. SPEC-GOVERNANCE-001 요약

| REQ | 내용 | 상태 |
|-----|------|------|
| REQ-GOV-001 | git commit before COMPLETE | ✅ 규칙화 + 시정 |
| REQ-GOV-002 | Branch Hierarchy 준수 | ✅ 문서화 완료 |
| REQ-GOV-003 | Coordinator Gate 이행 | 🔄 다음 Dispatch에서 검증 |
| REQ-GOV-004 | Issue-First 정책 | ✅ 규칙화 (이슈 #61, #62 생성) |
| REQ-GOV-005 | 한글 인코딩 보존 | ✅ .gitattributes + quotepath 설정 |
| REQ-GOV-006 | QA 보고서 파일 저장 | ✅ 규칙화 + 보고서 생성 |
| REQ-GOV-007 | Dispatch 라이프사이클 | ✅ 04-08 완료/이동, 04-09 PENDING |
| REQ-GOV-008 | 릴리즈 블로커 추적 | ✅ Gitea 이슈 #61, #62 등록 |

---

*보고서 생성: MoAI 커맨더센터 | 기준일: 2026-04-09 | 교차검증: 8/8 PASS*
