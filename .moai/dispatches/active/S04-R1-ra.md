# DISPATCH: RA — S04 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-11 |
| **발행자** | Main (MoAI Orchestrator) |
| **대상** | RA 팀 (인허가) |
| **브랜치** | team/ra |
| **유형** | S04 Round 1 — CMP 승인 + SBOM/RTM 갱신 |
| **우선순위** | P1 (DOC-042 CMP Draft→Approved), P2 (SBOM, RTM) |
| **SPEC 참조** | 없음 (규제 문서 관리) |
| **Gitea API** | http://10.11.1.40:7001/api/v1 (repo: drake.lee/Console-GUI) |

---

## 실행 방법

이 문서 전체를 읽고 Task 순서대로 실행하라.
- Task 1 완료 후 Task 2 착수
- Task 3은 Task 2 완료 후 착수 (Team A SPEC-INFRA-002 완료 후 실행 가능)
- 각 Task 완료 후 Status 섹션 업데이트 필수

---

## 컨텍스트

S04에서 RA 팀은 3가지 핵심 규제 문서 작업을 담당:
1. DOC-042 CMP(Configuration Management Plan): Draft 상태를 Approved로 승격
2. DOC-019 SBOM: Team A의 NuGet 변경사항 반영
3. DOC-032 RTM: SWR-CS-080(PHI 암호화) 테스트 케이스 매핑

Team A의 SPEC-INFRA-002(PHI 암호화) 완료는 Task 3(RTM)의 선행 조건.

---

## 파일 소유권

```
docs/regulatory/
docs/planning/
docs/verification/
docs/risk/
docs/management/
scripts/ra/
docfx.json
CHANGELOG.md
```

---

## Task 1 (P1): DOC-042 CMP Draft → Approved

### 배경

DOC-042 CMP가 Draft 상태로 작성자/검토자/승인자가 모두 미지정.
IEC 62304 §5.1.4 형상관리 계획서는 소프트웨어 개발 전 승인이 필요.

### 실행

**1. DOC-042_v2.0.md 작성**

기존 DOC-042_v1.0을 기반으로 다음 항목 보완:

| 항목 | v1.0 상태 | v2.0 보완 내용 |
|------|-----------|---------------|
| 작성자 | 미지정 | "drake.lee" 지정 |
| 검토자 | 미지정 | "SW Dev Lead" 지정 |
| 승인자 | 미지정 | "PM" 지정 |
| 형상항목 | 불완전 | 17개 모듈 + SPEC 문서 + 테스트 스위트 |
| 베이스라인 전략 | 미정 | Sprint 단위 베이스라인 정의 |
| 변경통제 | 미정 | PR 기반 변경통제 프로세스 |
| CI/CD | 미정 | Gitea Actions 파이프라인 정의 |

**2. 승인 상태 갱신**

문서 헤더:
```
상태: Approved
승인일: 2026-04-XX
버전: v2.0
```

### 수용 기준

- DOC-042_v2.0.md 파일 존재
- 작성자/검토자/승인자 지정
- 형상항목 17개 모듈 + 산출물 포함
- 베이스라인 전략 Sprint 단위로 정의
- 변경통제 프로세스 PR 기반으로 명확화

---

## Task 2 (P2): DOC-019 SBOM 갱신

### 배경

Team A가 SPEC-INFRA-001에서 NuGet 패키지 업그레이드를 수행함.
SBOM(CycloneDX 1.5 JSON)을 최신 상태로 갱신 필요.

### 실행

1. 현재 Directory.Packages.props에서 패키지 버전 목록 추출
2. DOC-019 SBOM 항목 업데이트:
   - 버전 변경 패키지 표시
   - 신규 패키지 추가 (있다면)
   - 제거 패키지 삭제
3. 취약성 점수(CVSS) 재확인
4. DOC-033 SOUP 목록 동기화

### 수용 기준

- SBOM 항목 수가 Directory.Packages.props와 일치
- 변경 이력 기록 (이전 버전 → 현재 버전)
- CVSS >= 7.0 항목 0건 확인

---

## Task 3 (P2): DOC-032 RTM SWR-CS-080 TC 매핑

### 배경

Team A가 SPEC-INFRA-002로 PHI AES-256-GCM 암호화를 구현 중.
이 요구사항(SWR-CS-080)에 대한 테스트 케이스 매핑이 RTM에 필요.
**Team A DISPATCH 완료 후에만 실행 가능.**

### 실행

1. Team A S04 R1 DISPATCH 결과 확인 (완료 시까지 대기)
2. DOC-032 RTM에 다음 매핑 추가:

```
MR-xxx → PR-CS-080 → SWR-CS-080 → TC-SEC-PHI-001~010
```

3. 테스트 케이스 ID 할당:
   - TC-SEC-PHI-001: AES-256-GCM 암호화/복호화 왕복
   - TC-SEC-PHI-002: 변조 태그 감지
   - TC-SEC-PHI-003: null/empty 처리
   - TC-SEC-PHI-004: Nonce 랜덤성
   - TC-SEC-PHI-005: HKDF 키 파생
   - ... (Team A 테스트 항목에 맞춰 추가)

### 수용 기준

- DOC-032에 SWR-CS-080 매핑 항목 존재
- TC-SEC-PHI-001~N 테스트 케이스 10개 이상 매핑
- 100% SWR → TC 매핑 달성

---

## Git 완료 프로토콜 [HARD]

```bash
git add docs/management/DOC-042_CMP_v2.0.md
git add docs/verification/DOC-019_SBOM*.md
git add docs/verification/DOC-032_RTM*.md
git add docs/verification/DOC-033_SOUP*.md
git commit -m "docs(ra): CMP v2.0 approved + SBOM update + RTM SWR-CS-080 mapping"
git push origin team/ra
# PR 생성
```

---

## Status (작업 후 업데이트)

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: DOC-042 CMP Approved | NOT_STARTED | -- | -- |
| Task 2: DOC-019 SBOM 갱신 | NOT_STARTED | -- | Team A NuGet 변경 반영 |
| Task 3: DOC-032 RTM 매핑 | NOT_STARTED | -- | Team A 완료 후 실행 |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
