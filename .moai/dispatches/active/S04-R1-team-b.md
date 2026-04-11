# DISPATCH: Team B — S04 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-11 |
| **발행자** | Main (MoAI Orchestrator) |
| **대상** | Team B (의료영상 팀) |
| **브랜치** | team/team-b |
| **유형** | S04 Round 1 — Dicom 커버리지 + Update 커버리지 |
| **우선순위** | P1 (Dicom 49.6%→80%), P2 (Update 75%→85%) |
| **SPEC 참조** | SPEC-TEAMB-COV-001 v1.1 |
| **Gitea API** | http://10.11.1.40:7001/api/v1 (repo: drake.lee/Console-GUI) |

---

## 실행 방법

이 문서 전체를 읽고 Task 순서대로 실행하라.
- Task 1 완료 후 Task 2 착수
- 각 Task 완료 후 Status 섹션 업데이트 필수

---

## 컨텍스트

S03에서 Team B는 Detector 92.4%, Dose 99.5%, PatientManagement 100%, Incident 96.1%를 달성함.
그러나 HnVue.Dicom은 49.6%로 80% 목표에 미달. HnVue.Update은 75.0%로 85%에 미달.

S04에서 이 두 모듈의 커버리지를 목표치까지 끌어올리는 것이 핵심 과제.
Dicom은 외부 의존성(fo-dicom)이 많은 네트워크 코드로, Mock/In-memory 전략이 필요.

---

## 파일 소유권

```
HnVue.Dicom/
HnVue.Detector/
HnVue.Imaging/
HnVue.Dose/
HnVue.Incident/
HnVue.Workflow/
HnVue.PatientManagement/
HnVue.CDBurning/
HnVue.Update/       (Team B가 이번에 담당)
tests/HnVue.Dicom.Tests/
tests/HnVue.Update.Tests/  (없으면 신규 생성)
```

---

## Task 1 (P1): HnVue.Dicom 커버리지 49.6% → 80%

### 사전 확인

```bash
# 기존 Dicom 테스트 현황 파악
dotnet test tests/HnVue.Dicom.Tests/ --no-build 2>&1 | tail -5

# 0% 또는 저커버리지 클래스 식별
# DicomService, DicomStoreScu, MppsScu, WorklistScu 등 fo-dicom 래퍼 클래스
```

### 테스트 작성 전략

**외부 의존성(fo-dicom 네트워크) Mock 전략**:
- DicomClient, DicomServer는 Mock/In-memory로 대체
- 실제 TCP 연결 없이 Dicom 메시지 생성/파싱만 테스트
- C-STORE, C-FIND, MWL 요청/응답 시퀀스 Mock

**우선 작성 대상 (50+ 테스트)**:
1. DicomService: 연결 수명주기 (Initialize/Connect/Disconnect)
2. DicomStoreScu: C-STORE 요청 생성, 응답 처리, 에러 핸들링
3. MppsScu: MPPS 생성/업데이트 시퀀스
4. WorklistScu: MWL C-FIND 쿼리/응답 파싱
5. DICOM 태그 조작 유틸리티 (DatasetHelper 등)
6. 에러 복구: 연결 끊김, 타임아웃, 잘못된 응답

### 수용 기준

- HnVue.Dicom line coverage 80%+
- 신규 테스트 50개 이상
- 기존 테스트 회귀 없음
- `dotnet test tests/HnVue.Dicom.Tests/` 전원 PASS

---

## Task 2 (P2): HnVue.Update 커버리지 75.0% → 85%

### 사전 확인

```bash
# Update 테스트 현황
ls tests/HnVue.Update.Tests/ 2>/dev/null || echo "PROJECT NOT FOUND"

# Update 모듈 코드 분석
grep -r "class " src/HnVue.Update/ --include="*.cs" | head -20
```

### 테스트 작성 대상

1. UpdateService: 버전 비교, 업데이트 체크, 다운로드 검증
2. UpdateRepository: CRUD (In-memory SQLite)
3. 롤백 메커니즘: 실패 시 복구 경로
4. 코드 서명 검증: 위조 서명 거부, 만료 서명 처리
5. HTTPS 강제: HTTP 요청 거부

### 수용 기준

- HnVue.Update line coverage 85%+
- 신규 테스트 15개 이상
- `dotnet test` 전원 PASS

---

## 빌드 검증

```bash
dotnet build HnVue.sln
dotnet test tests/HnVue.Dicom.Tests/ --no-build
dotnet test tests/HnVue.Update.Tests/ --no-build
```

---

## Git 완료 프로토콜 [HARD]

모든 Task 완료 후 순서대로 실행:

```bash
# 1. 스테이징
git add tests/HnVue.Dicom.Tests/
git add tests/HnVue.Update.Tests/
# (변경된 파일 모두 명시적으로 추가)

# 2. 커밋
git commit -m "test(team-b): Dicom 49.6%→80% + Update 75%→85% coverage push (SPEC-TEAMB-COV-001)"

# 3. 푸시
git push origin team/team-b

# 4. PR 생성 (기존 열린 PR 확인 후 없으면 신규 생성)
# gitea-api.sh 사용 또는 curl로 생성
```

---

## Status (작업 후 업데이트)

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Dicom 49.6%→80% | NOT_STARTED | -- | -- |
| Task 2: Update 75%→85% | NOT_STARTED | -- | -- |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |

### 빌드 검증 결과

```
# 여기에 실제 빌드/테스트 결과 기록
dotnet build: ?
Dicom 테스트: ?
Update 테스트: ?
Dicom 커버리지: ?
Update 커버리지: ?
```
