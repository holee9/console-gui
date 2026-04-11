# Team B Worktree 역할/규칙 점검 보고서

| 항목 | 내용 |
|------|------|
| 보고 대상 | Main Branch Commander Center |
| 작성 일시 | 2026-04-10 |
| 작성 주체 | team-b worktree 점검 |
| 점검 목적 | Team B 워크트리의 역할, 작업 경계, 이슈 등록 규칙, 우선 작업 지시 일치 여부 확인 |
| 점검 범위 | 로컬 규칙 문서 및 현재 DISPATCH 문서 |
| 점검 방식 | 문서 정합성 검토, 역할/규칙 추출, 상호 충돌 여부 확인 |

---

## 1. 점검 결론 요약

- 현재 워크트리는 `team-b`로 명확히 설정되어 있으며, 의료 영상 처리 파이프라인 전담 워크트리로 확인됨
- Team B 소유 모듈, 안전성-중요 규칙, 인터페이스 변경 승인 체계, 이슈 생성 조건이 문서상 명확히 정의되어 있음
- `DISPATCH.md`의 최신 지시사항은 Team B 규칙과 대체로 일치하며, 현재 최우선 작업은 `Task 1` 빌드 오류 수정으로 확인됨
- 작업 전 Git 이슈 생성은 일반 의무가 아니며, safety-critical 변경, workflow 상태 변경, 소유 범위 외 변경 등 특정 조건에서만 필수임
- 문서 경로 표기 1건에 경미한 불일치가 있음

---

## 2. 점검 근거 문서

1. `DISPATCH.md`
2. `.claude/rules/team-context.md`
3. `.claude/rules/teams/team-b.md`
4. `.claude/rules/teams/coordinator.md`
5. `docs/OPERATIONS.md`

---

## 3. Team B 역할 점검 결과

### 3.1 활성 팀 식별

`team-context.md` 기준 현재 워크트리의 활성 팀은 `team-b`이며, 역할은 다음과 같음.

- DICOM
- Detector SDK
- Imaging
- Dose safety
- Incident management
- Workflow engine
- Patient management
- CD burning

### 3.2 소유 모듈 확인

`team-b.md`와 `DISPATCH.md`에서 일치하게 확인된 Team B 소유 모듈은 다음과 같음.

- `HnVue.Dicom`
- `HnVue.Detector`
- `HnVue.Imaging`
- `HnVue.Dose`
- `HnVue.Incident`
- `HnVue.Workflow`
- `HnVue.PatientManagement`
- `HnVue.CDBurning`

### 3.3 작업 경계 확인

현재 워크트리는 다음 원칙으로 운영되어야 함.

- Team B 소유 범위 안에서만 직접 수정
- 소유 범위를 벗어나는 변경은 직접 수정하지 않고 이슈 생성 후 해당 팀 통지
- cross-team 인터페이스 변경은 Coordinator 승인 후 진행

판정: `역할 및 소유 경계가 명확함`

---

## 4. Team B 규칙 점검 결과

### 4.1 Safety-Critical 규칙

다음 항목이 강제 규칙으로 확인됨.

- `Dose`, `Incident`는 safety-critical 모듈
- 두 모듈은 `90%+ branch coverage` 기준 적용
- safety-critical 소스 변경 전 characterization test 선행 필요
- `Dose`의 4-level interlock logic은 불변으로 간주
- 해당 로직 변경 시 RA risk assessment 필요

판정: `규칙 명확, 고위험 변경 통제 체계 존재`

### 4.2 Detector 규칙

다음 규칙이 확인됨.

- 모든 detector 상호작용은 `IDetectorService` 추상화를 통해 수행
- 테스트는 simulator adapter 기반으로 수행
- vendor SDK는 adapter 패턴으로 래핑
- lifecycle은 `Initialize -> Connect -> Configure -> Acquire -> Disconnect`

판정: `하드웨어 종속성을 제어하는 아키텍처 규칙 명확`

### 4.3 Workflow 규칙

다음 규칙이 확인됨.

- 9-state transition model이 authoritative source
- 허용 전이 테이블 기준으로 상태 전이 검증 필요
- 잘못된 전이는 `InvalidOperationException`
- 상태 변경 이벤트는 UI 통지를 위해 발행해야 함

판정: `상태 머신 변경은 규제 추적 대상`

### 4.4 인터페이스 및 협업 규칙

다음 승인/조율 체계가 확인됨.

- `IDetectorService`, `IWorkflowEngine` 변경은 Coordinator 승인 필수
- Workflow state transition 변경은 RA 이슈 생성 후 RTM 업데이트 필요
- Patient data model 변경은 Team A와 조율 필요

판정: `cross-team 변경은 사전 승인 체계 하에 진행해야 함`

---

## 5. 작업 전 이슈 등록 규칙 점검 결과

### 5.1 결론

작업 전 Git 이슈 등록은 `항상 필수`가 아님.

### 5.2 이슈 생성이 필요한 경우

- Safety-critical 변경
  - 라벨: `team-b` + `priority-high`
- Workflow state 변경
  - 이슈 생성 후 RA 팀 통지 필요
- Team B 소유 범위를 벗어나는 변경 요청
  - 이슈 생성 후 소유 팀 통지 필요
- UI 계약 또는 공용 인터페이스 변경
  - Coordinator 승인 및 관련 이슈 필요

### 5.3 이슈 없이 진행 가능한 경우

- Team B 소유 범위 내 일반 구현/수정
- 테스트 보강
- 빌드 오류 수정
- 문서상 승인 조건을 건드리지 않는 내부 리팩터링

단, 이 경우에도 로컬 운영 규칙상 작업 접근 방식과 수정 범위를 먼저 명시하는 절차는 유지되어야 함.

판정: `이슈 생성은 조건부 의무`

---

## 6. 최신 DISPATCH와의 일치 여부

`DISPATCH.md` 기준 현재 Team B에 내려온 지시는 다음과 같이 해석됨.

- 최우선 순위는 `Task 1: VendorAdapterTemplateTests 빌드 오류 수정`
- 이후 Detector, Dose, Dicom, PatientManagement 순으로 커버리지 개선 수행
- Team B 소유 파일만 수정
- Safety-critical 소스 수정 시 characterization test 선행
- `IDetectorService`, `IWorkflowEngine` 변경 시 Coordinator 승인 필수

판정: `DISPATCH와 team-b 규칙 간 실질 충돌 없음`

---

## 7. 발견 사항 및 운영상 주의점

### 7.1 문서 경로 표기 불일치

`DISPATCH.md`에는 `rules/teams/team-b.md`라고 표기되어 있으나, 실제 확인된 규칙 파일의 경로는 `.claude/rules/teams/team-b.md`임.

영향:

- 신규 작업자 또는 자동화 에이전트가 잘못된 경로를 참조할 수 있음
- 규칙 근거 추적 시 혼선 가능

권고:

- `DISPATCH.md`의 규칙 참조 경로를 실제 경로로 정정

### 7.2 규칙 우선순위의 실무 해석

현재 Team B는 단순 구현 팀이 아니라, 안전성-중요 로직과 하드웨어 연동 경계를 함께 관리하는 팀으로 해석해야 함. 따라서 다음 작업은 일반 기능 개발보다 `안전성 규칙 준수 여부`를 먼저 검토해야 함.

---

## 8. Commander Center 전달용 최종 판정

- Team B 워크트리 역할 정의: `정상`
- Team B 소유 경계: `명확`
- Safety-critical 규칙: `명확`
- 인터페이스 승인 체계: `명확`
- 작업 전 이슈 등록 규칙: `조건부 필수`
- 최신 DISPATCH와의 정합성: `양호`
- 즉시 보완 필요 항목: `DISPATCH 내 규칙 파일 경로 표기 수정`

---

## 9. 보고 문안 요약

Main Branch Commander Center에 다음과 같이 보고 가능함.

> Team B worktree의 역할 및 운영 규칙 점검 결과, 현재 워크트리는 의료 영상 처리 파이프라인 전담 범위로 정상 설정되어 있으며 소유 모듈과 협업 경계가 문서상 명확히 정의되어 있습니다. Safety-critical 규칙, 인터페이스 승인 체계, workflow 변경 시 RA 연계 규칙도 일관되게 확인되었습니다. 작업 전 Git 이슈 등록은 일반 의무가 아니라 safety-critical 변경, workflow 상태 변경, 소유 범위 외 변경 등 특정 조건에서만 필수입니다. 최신 DISPATCH는 Team B 규칙과 실질 충돌이 없으며, 현재 우선 작업은 Task 1 빌드 오류 수정입니다. 다만 DISPATCH 내 규칙 파일 참조 경로는 실제 경로인 `.claude/rules/teams/team-b.md`로 정정이 필요합니다.

---

## 10. 점검 한계

- 본 보고는 코드 수정이나 테스트 실행이 아니라 문서 기반 운영 규칙 점검 결과임
- GitHub/Gitea 원격 이슈 상태나 실제 라벨 체계 존재 여부는 이번 점검 범위에 포함하지 않음
