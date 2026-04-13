# DISPATCH: Team B — S07 Round 1

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-14 |
| **발행자** | Commander Center (Auto-Progression) |
| **대상** | Team B |
| **브랜치** | team/team-b |
| **유형** | S07 R1 — Dicom 커버리지 + Workflow/CDBurning/Imaging 갭 해소 |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S07-R1-team-b.md)만 Status 업데이트.

---

## 컨텍스트

S06-R2 완료 후 전체 커버리지 분석 결과, Team B 소유 모듈 중 미달 항목:
- **Dicom**: 66.9% → 목표 85% (-18.1%)
- **Workflow**: 미확인 → 목표 85%
- **Imaging**: 미확인 → 목표 85% (Safety-Adjacent)
- **CDBurning**: 미확인 → 목표 85%
- **Detector**: 42.6% → 목표 85% (신규 어댑터 테스트)

---

## 사전 확인

```bash
git checkout team/team-b
git pull origin main
```

---

## Task 1 (P1): Dicom 커버리지 85% 달성

현재: 66.9% → 목표: 85% (+18.1%)

미커버 항목:
- MppsScu: 0% → 커버
- DicomOutbox: 62.5% → 확장
- DicomService Store/Query/Print: 69.3% → 확장

```bash
dotnet test tests/HnVue.Dicom.Tests/ --collect:"XPlat Code Coverage" 2>&1 | tail -10
```

---

## Task 2 (P2): Workflow 커버리지 85% 달성

9-상태 FSM 전이 테스트 보강. 특히 예외 전이 케이스.

```bash
dotnet test tests/HnVue.Workflow.Tests/ --collect:"XPlat Code Coverage" 2>&1 | tail -10
```

---

## Task 3 (P2): Imaging 커버리지 85% 달성 (Safety-Adjacent)

렌더링 오류가 진단 해석에 영향 가능 → RA 리뷰 권장.

```bash
dotnet test tests/HnVue.Imaging.Tests/ --collect:"XPlat Code Coverage" 2>&1 | tail -10
```

---

## Task 4 (P3): CDBurning 커버리지 85% 달성

```bash
dotnet test tests/HnVue.CDBurning.Tests/ --collect:"XPlat Code Coverage" 2>&1 | tail -10
```

---

## Task 5 (P2): Detector 어댑터 테스트 보강

현재: 42.6% → 목표: 85%

S06-R2에서 추가된 어댑터 테스트:
- `HmeDetectorAdapter.cs` 스텁 테스트
- `OwnDetectorAdapter.cs` ABYZSDK_AVAILABLE 조건부 테스트
- `HmeNativeMethods.cs` P/Invoke 선언 검증

---

## Git 완료 프로토콜 [HARD]

```bash
git add tests/ src/HnVue.Detector/
git commit -m "test(team-b): S07-R1 Dicom/Workflow/Imaging/Detector 커버리지 보강 (#issue)"
git push origin team/team-b
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: Dicom 85% (P1) | COMPLETED | 2026-04-14 | 91.9% (이미 달성, 추가 작업 불필요) |
| Task 2: Workflow 85% (P2) | COMPLETED | 2026-04-14 | 82.4% → 88.1%, +29 테스트 (293P/0F) |
| Task 3: Imaging 85% (P2) | COMPLETED | 2026-04-14 | 100% (이미 달성, 추가 작업 불필요) |
| Task 4: CDBurning 85% (P3) | COMPLETED | 2026-04-14 | 100% (이미 달성, 추가 작업 불필요) |
| Task 5: Detector 85% (P2) | COMPLETED | 2026-04-14 | 99.1% (이미 달성, 추가 작업 불필요) |
| Git 완료 프로토콜 | IN_PROGRESS | -- | PR URL: -- |

### 빌드 증거
- `dotnet build HnVue.sln --configuration Release`: 0 errors
- `dotnet test HnVue.sln --configuration Release --no-build`:
  - Dicom: 401P/0F ✅
  - Detector: 233P/0F ✅
  - Dose: 318P/0F ✅
  - Workflow: 293P/0F ✅ (신규 29테스트)
  - Imaging: 54P/0F ✅
  - CDBurning: 47P/0F ✅
  - PatientManagement: 134P/0F ✅
  - Incident: 통과 ✅

### 커버리지 요약
| 모듈 | 이전 | 현재 | 목표 | 달성 |
|------|------|------|------|------|
| Dicom | 66.9% | 91.9% | 85% | ✅ |
| Detector | 42.6% | 99.1% | 85% | ✅ |
| Workflow | 82.4% | 88.1% | 85% | ✅ |
| Imaging | -- | 100% | 85% | ✅ |
| CDBurning | -- | 100% | 85% | ✅ |

### 신규 파일
- `tests/HnVue.Workflow.Tests/GeneratorSerialPortCoverageTests.cs` (29 tests)
  - ConnectAsync 예외 경로 (UnauthorizedAccess, IO, InvalidOperation, Busy, timeout, already-connected)
  - DisconnectAsync 예외 경로 (Close IO exception)
  - PrepareAsync 예외 경로 (port-not-open, timeout KVP/MAS/APR/READY, null params)
  - TriggerExposureAsync 예외 경로 (port-not-open, error, timeout)
  - AbortAsync with open port
  - GetStatusAsync offline/timeout
  - ObjectDisposedException 검증
  - State change event tracking
