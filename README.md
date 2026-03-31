# Console-GUI

HnVue - Medical Diagnostic X-Ray Console Software

## Overview

HnVue Console SW는 자사 FPD(Flat Panel Detector)에 번들되는 X-ray 촬영 콘솔 소프트웨어이다.

현재 외부 구매/외주 중인 Console SW(IMFOU feel-DRCS OEM + HnVue 외주 개발)를 **자체 개발로 내재화**하는 것이 본 프로젝트의 목표이다.

### 내재화 단계

| Phase | 범위 | 영상처리 |
|-------|------|---------|
| Phase 1 (12-18개월) | 콘솔 프레임워크 자체 개발, feel-DRCS 핵심 기능 대체 | 외부 SDK 구매 연동 |
| Phase 2 (추가 12개월) | 업계 표준 달성 (Auto Stitching, 다국어, Reject Analysis 등) | 자체 엔진 내재화 |
| Phase 3 (24개월+) | AI/Cloud 등 고급 기능 | 자체 + AI 파트너십 |

### 기술 스택

| 계층 | 기술 |
|------|------|
| UI | WPF (.NET 8 LTS) |
| DICOM | fo-dicom 5.x (MIT) |
| 영상처리 | 외부 SDK (Phase 1) / 자체 (Phase 2) |
| DB | SQLite + EF Core |
| 로깅 | Serilog |
| 테스트 | xUnit + NSubstitute |

## Documentation

All project documentation is organized under `docs/`:

| Category | Path | Description |
|----------|------|-------------|
| Planning | `docs/planning/` | MRD, PRD, FRS, SRS, SAD, SDS |
| Research | `docs/planning/research/` | Market research, strategy, competitive analysis |
| Management | `docs/management/` | DMP, WBS, Development Guidelines |
| Risk | `docs/risk/` | Risk Management Plan, FMEA, Threat Model |
| Testing | `docs/testing/` | Unit/Integration/System Test Plans |
| Regulatory | `docs/regulatory/` | SBOM, DHF, DICOM Conformance |
| Verification | `docs/verification/` | V&V Plans, RTM |

### Key Documents

| Doc ID | Document | Version | Description |
|--------|----------|---------|-------------|
| DOC-001 | MRD | v2.0 | Market Requirements (71 MR, 67 active, P1-P4) |
| DOC-002 | PRD | v1.0 | Product Requirements |
| DOC-004 | FRS | v1.0 | Functional Requirements Specification |
| DOC-005 | SRS | v1.0 | Software Requirements Specification |
| DOC-006 | SAD | v1.0 | Software Architecture Document |
| DOC-007 | SDS | v1.0 | Software Design Specification |
| STRATEGY-001 | Strategy | v2.0 | Company positioning and internalization strategy |

### Research Documents

| Document | Description |
|----------|-------------|
| STRATEGY-001 v2.0 | HnVue Console SW internalization strategy |
| STRATEGY-001 v1.0 | Initial strategy (superseded) |
| FPD Console SW Market Research | FPD console SW buy/build/open-source analysis |
| Market Research (Console) | X-ray console software competitive landscape |
| Market Research (Imaging) | X-ray imaging software market data |

### MRD v2.0 Priority Summary

| Priority | Count | Description |
|----------|:-----:|-------------|
| P1 Regulatory Mandatory | 15 | MFDS certification blockers |
| P2 Safety-Critical | 15 | ISO 14971 risk control |
| P3 Clinically Important | 19 | feel-DRCS baseline replacement |
| P4 Desirable | 13 | Phase 2+ deferrable |
| Excluded | 4 | Permanently removed |
| **Active Total** | **67** | |

## Regulatory Standards

- IEC 62304 (Medical Device Software Lifecycle) - Class B
- IEC 62366-1 (Usability Engineering)
- ISO 14971 (Risk Management)
- FDA 21 CFR 820.30 (Design Controls)
- ISO 13485 (Quality Management)
- DICOM 3.0 / IHE SWF
- HIPAA / GDPR
