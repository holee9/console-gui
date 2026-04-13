# DISPATCH: RA Team — S06 Round 2

| 항목 | 내용 |
|------|------|
| **발행일** | 2026-04-13 |
| **발행자** | Commander Center |
| **대상** | RA Team |
| **브랜치** | team/ra |
| **유형** | S06 R2 — 신규 Detector SDK SBOM + SOUP 등록 |
| **우선순위** | P1-High |

---

## 규칙: 이 파일은 CC 전용입니다

> [HARD] 이 파일(.moai/dispatches/active/S06-R2-ra.md)만 Status 업데이트.

---

## 컨텍스트

IEC 62304 §5.3.3: 외부 SW 컴포넌트는 SBOM에 등록 필수.

신규 SDK 2종 추가됨:
1. **자사 AbyzSdk** (`sdk/own-detector/bluesdk/AbyzSdk.dll`)
   - Version: 0.1.0.0
   - Type: .NET managed (IL Only, CLR 2.05)
   - License: Proprietary (자사)
   - Namespace: AbyzSdk.*
   - Dependencies: Microsoft.Extensions.Logging, Microsoft.Extensions.DependencyInjection
   - 파일 크기: 185,344 bytes (AbyzSdk.dll) + 20,016 bytes (AbyzSdk.Imaging.dll)

2. **HME libxd2** (`sdk/third-party/hme-licence/dll/libxd2.dll`)
   - Vendor: HME (라이선스 취득)
   - Type: Native C DLL (146 exported functions)
   - Supported models: S4335-CA (3072x2560), SZ4335-W (3072x2560), S4343-CA (3072x3072)
   - Network: 5-소켓 TCP (Port 25000-25004: Control/Data/Trigger/Status/SAlign)
   - Associated DLLs:
     - `libxd.dll` (100 exports) — 1G SDK
     - `CIB_Mgr.dll` (9 exports) — CR/DR 회전/노출 제어
     - `ucrtbased.dll`, `vcruntime140d.dll`, `vcruntime140_1d.dll` — VC++ Runtime Debug
   - 포함 도구:
     - `SDXUpdater.exe` (v1.0.0.0) — 펌웨어 업데이터
     - `EzView.exe` — 뷰어 애플리케이션
     - `XAS_W.exe` — 테스트 툴
     - `caleng.exe` — 교정 엔진

---

## 사전 확인

```bash
git checkout team/ra
git pull origin main
```

---

## Task 1 (P1): DOC-019 SBOM CycloneDX 업데이트

파일: `docs/regulatory/DOC-019_SBOM_v*.json` (CycloneDX 1.5 JSON)

신규 컴포넌트 **7개** 추가 (DLL export 분석 기반 정확한 정보):

```json
// 1. AbyzSdk 컴포넌트
{
  "type": "library",
  "name": "AbyzSdk",
  "version": "0.1.0.0",
  "purl": "pkg:nuget/AbyzSdk@0.1.0.0",
  "licenses": [{ "license": { "name": "Proprietary" } }],
  "description": "자사 CsI FPD 디텍터 SDK (.NET managed, IL Only, CLR 2.05)",
  "supplier": { "name": "Abyzr Co.,Ltd." }
}

// 2. AbyzSdk.Imaging 컴포넌트
{
  "type": "library",
  "name": "AbyzSdk.Imaging",
  "version": "0.1.0.0",
  "purl": "pkg:nuget/AbyzSdk.Imaging@0.1.0.0",
  "licenses": [{ "license": { "name": "Proprietary" } }],
  "description": "자사 FPD 이미징 처리 라이브러리 (.NET managed)",
  "supplier": { "name": "Abyzr Co.,Ltd." }
}

// 3. HME libxd2 컴포넌트 (146 exports)
{
  "type": "library",
  "name": "libxd2",
  "version": "2.0",
  "licenses": [{ "license": { "name": "Commercial (HME license)" } }],
  "description": "HME 2G Wireless FPD Detector SDK — Native C DLL (146 exported functions: lifecycle, acquisition, calibration, diagnostics, firmware update)",
  "supplier": { "name": "HME" },
  "properties": [
    { "name": "supported_models", "value": "S4335-CA, SZ4335-W, S4343-CA" },
    { "name": "protocol", "value": "TCP 5-socket (ports 25000-25004)" },
    { "name": "frame_sizes", "value": "3072x2560, 3072x3072" }
  ]
}

// 4. HME libxd 컴포넌트 (100 exports)
{
  "type": "library",
  "name": "libxd",
  "version": "1.0",
  "licenses": [{ "license": { "name": "Commercial (HME license)" } }],
  "description": "HME 1G FPD Detector SDK — Native C DLL (100 exported functions)",
  "supplier": { "name": "HME" }
}

// 5. HME CIB_Mgr 컴포넌트 (9 exports)
{
  "type": "library",
  "name": "CIB_Mgr",
  "version": "1.0",
  "licenses": [{ "license": { "name": "Commercial (HME license)" } }],
  "description": "HME CIB 제어 모듈 — CR/DR 회전/노출 제어 (9 exports: ComOpen/Close, ExposeOn, CR_DR mode)",
  "supplier": { "name": "HME" }
}

// 6-7. VC++ Debug Runtime (ucrtbased.dll, vcruntime140d.dll, vcruntime140_1d.dll)
// NOTE: Debug DLL은 배포 빌드에서 제외될 수 있음. Release DLL 포함 여부 확인 필요.
```

---

## Task 2 (P1): DOC-033 SOUP 목록 업데이트

파일: `docs/verification/DOC-033_SOUP_*.md`

신규 SOUP 항목 5개 (DLL export 분석 기반):

| # | 명칭 | 버전 | 공급사 | 용도 | 타입 | Export 수 | 라이선스 |
|---|------|------|--------|------|------|-----------|----------|
| 1 | AbyzSdk | 0.1.0.0 | Abyzr | 자사 FPD 연결/획득 | .NET managed | N/A | Proprietary |
| 2 | AbyzSdk.Imaging | 0.1.0.0 | Abyzr | 이미징 처리 | .NET managed | N/A | Proprietary |
| 3 | libxd2 | 2.0 | HME | 2G 무선 FPD 연결/획득 | Native C | 146 | Commercial |
| 4 | libxd | 1.0 | HME | 1G FPD 연결/획득 | Native C | 100 | Commercial |
| 5 | CIB_Mgr | 1.0 | HME | CR/DR 회전/노출 제어 | Native C | 9 | Commercial |

### libxd2 기능 분류 (IEC 62304 §7.4.3 SOUP 평가용)
| 기능군 | 함수 수 | 대표 함수 | 안전영향 |
|--------|---------|-----------|----------|
| Detector Lifecycle | 10+ | SD_Create/Destroy, CheckConnection | 낮음 |
| Acquisition | 30+ | SDAcq_CreateEx_*, Execute, Abort | **높음** (노출제어) |
| Calibration | 14 | SDCal_*, GenerateBPM, Validate | **높음** (영상품질) |
| Sleep/Power | 5 | Sleep, WakeUp, PowerOff, Reboot | 중간 |
| Firmware Update | 15 | SDUpdater_* | 중간 |
| Diagnostic | 10+ | SDDiag_*, SDDebug_* | 낮음 |
| File Transfer | 14 | SDFile_*, SDRemote_* | 낮음 |
| CIB Control | 9 | CIB_ExposeOn, CR_DR_Mode_* | **높음** (노출제어) |

### 알려진 이상 및 평가
| 항목 | 평가 |
|------|------|
| AbyzSdk | 자사 개발, 위험 낮음. 소스코드 접근 가능 |
| libxd2 | 상용 라이선스, 검증됨. 샘플코드(VS2019 C++) 제공됨 |
| libxd | 상용, 1G 레거시. libxd2 마이그레이션 예정 |
| CIB_Mgr | 상용, CR/DR 노출 제어. IEC 60601-2-54 관련 안전기능 |

---

## Git 완료 프로토콜 [HARD]

```bash
git add docs/regulatory/ docs/verification/
git commit -m "docs(ra): S06-R2 신규 Detector SDK SBOM + SOUP 등록 (#issue)"
git push origin team/ra
```

---

## Status

| Task | 상태 | 완료 시각 | 비고 |
|------|------|---------|------|
| Task 1: DOC-019 SBOM 업데이트 (P1) | NOT_STARTED | -- | 2개 컴포넌트 추가 |
| Task 2: DOC-033 SOUP 업데이트 (P1) | NOT_STARTED | -- | 2개 항목 추가 |
| Git 완료 프로토콜 | NOT_STARTED | -- | PR URL: -- |
