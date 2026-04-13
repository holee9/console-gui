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
   - Type: .NET managed library
   - License: Proprietary (자사)
   - Namespace: AbyzSdk.*
   - Dependencies: Microsoft.Extensions.Logging, Microsoft.Extensions.DependencyInjection

2. **HME libxd2** (`sdk/third-party/hme-licence/dll/libxd2.dll`)
   - Vendor: HME (라이선스 취득)
   - Type: Native C DLL
   - Supported models: S4335-WA, S4335-WF, S4343-WA
   - Associated: CIB_Mgr.dll, libxd.dll

---

## 사전 확인

```bash
git checkout team/ra
git pull origin main
```

---

## Task 1 (P1): DOC-019 SBOM CycloneDX 업데이트

파일: `docs/regulatory/DOC-019_SBOM_v*.json` (CycloneDX 1.5 JSON)

신규 컴포넌트 2개 추가:

```json
// AbyzSdk 컴포넌트
{
  "type": "library",
  "name": "AbyzSdk",
  "version": "0.1.0.0",
  "purl": "pkg:nuget/AbyzSdk@0.1.0.0",
  "licenses": [{ "license": { "name": "Proprietary" } }],
  "description": "자사 CsI FPD 디텍터 SDK (.NET managed)",
  "supplier": { "name": "Abyzr Co.,Ltd." }
}

// HME libxd2 컴포넌트
{
  "type": "library",
  "name": "libxd2",
  "version": "2.0",
  "licenses": [{ "license": { "name": "Commercial (HME license)" } }],
  "description": "HME 2G Wireless FPD Detector SDK (Native C)",
  "supplier": { "name": "HME" }
}
```

---

## Task 2 (P1): DOC-033 SOUP 목록 업데이트

파일: `docs/verification/DOC-033_SOUP_*.md`

신규 SOUP 항목:

| 항목 | AbyzSdk | HME libxd2 |
|------|---------|------------|
| 명칭 | AbyzSdk | libxd2 |
| 버전 | 0.1.0.0 | 2.0 |
| 공급사 | Abyzr | HME |
| 용도 | 자사 FPD 연결/획득 | HME FPD 연결/획득 |
| 라이선스 | Proprietary | Commercial |
| 알려진 이상 | 없음 | 없음 |
| 평가 | 자사 개발, 위험 낮음 | 상용 라이선스, 검증됨 |

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
