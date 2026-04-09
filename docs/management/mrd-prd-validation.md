# MRD/PRD 교차검증 및 글로벌 딥리서치 (2026-04-06)

> 원본: README.md "MRD/PRD 교차검증" 섹션에서 분리 (2026-04-09)

## 교차검증 요약

구현 모듈 14개(153개 소스 파일)와 MRD v3.0(72개 MR), PRD v2.0을 교차검증하고, 글로벌 36사 상용제품 딥리서치를 수행하여 문서를 개정하였습니다.

| 항목 | 결과 |
|------|------|
| Phase 1 구현 커버리지 (Tier 1+2) | **90%** (27/30 구현 완료) |
| Tier 3 선행 구현 | 4건 (MPPS, Storage Commitment, Scatter Correction, Auto-Trim) |
| 미구현 잔여 (Tier 2) | 3건 (다국어 .resx, 프로토콜 50개 시딩, 실기 HW 연동) |
| 조사 벤더 총합 | **36사** (기존 20 + 신규 16) |
| 신규 MR 추가 | **20건** (MR-073 ~ MR-092) |

## MRD Tier 분포 (v3.0 -> v4.0)

| Tier | v3.0 | v4.0 | 변동 | 의미 |
|------|:----:|:----:|:----:|------|
| Tier 1 (인허가 필수) | 13 | 13 | - | Phase 1 필수 |
| Tier 2 (시장 진입 필수) | 17 | 17 | - | Phase 1 필수 |
| Tier 3 (추후업그레이드) | 25 | **35** | +10 | Phase 2+ |
| Tier 4 (있으면좋음) | 13 | **23** | +10 | Phase 3+ |
| **총계** | **72** | **92** | **+20** | |

> Phase 1 범위(Tier 1+2 = 30개)는 변동 없음. 신규 20건은 모두 Tier 3/4로 배정.

## 신규 MR 항목 요약 (MR-073 ~ MR-092)

### Tier 3 추후업그레이드 (10건)

| MR ID | 요구사항 | 참조 벤더 |
|-------|---------|----------|
| MR-073 | 자동 이미지 회전 (Auto-Rotate) | Agfa SmartRotate |
| MR-074 | DICOM Presentation State (GSPS) | DICOM 표준 |
| MR-075 | Barcode/QR 환자 식별 | GE Auto Protocol |
| MR-076 | DICOMweb (WADO-RS/STOW-RS) | Philips IM 15 |
| MR-077 | Dual-Energy Subtraction | Carestream Eclipse |
| MR-078 | 선량 크립 모니터링 | Qaelum DOSE |
| MR-079 | HL7v2 ADT/ORM 직접 수신 | 중소병원 요구 |
| MR-080 | 디텍터 공유 (Detector Sharing) | Siemens MAXswap |
| MR-081 | Multi-Frequency Image Processing | Carestream Eclipse |
| MR-082 | DICOM GSDF 디스플레이 교정 | DICOM Part 14 |

### Tier 4 있으면좋음 (10건)

| MR ID | 요구사항 | 참조 벤더 |
|-------|---------|----------|
| MR-083 | 훈련/시뮬레이션 모드 | DRGEM RadTrainer |
| MR-084 | Dynamic Digital Radiography (DDR) | Konica Minolta |
| MR-085 | On-Device AI CADe | GE Critical Care Suite |
| MR-086 | Zero-Footprint Web Viewer | OHIF Viewer |
| MR-087 | Cobb Angle 자동 측정 | dicomPACS DX-R |
| MR-088 | 원격 서비스/예지 정비 | Siemens Remote |
| MR-089 | EU AI Act 고위험 AI 준수 | EU 2024/1689 |
| MR-090 | NIS2 인시던트 보고 | EU NIS2 |
| MR-091 | Pre-Exposure 모션 감지 | Shimadzu VISION |
| MR-092 | 에너지 절감 모드 | Green Radiology |

## 관련 문서

| 문서 | 경로 | 설명 |
|------|------|------|
| CVR-002 | `docs/planning/CVR-002_MRD_PRD_CrossVerification_v1.0.md` | 교차검증 + 갭 분석 + 36사 딥리서치 종합 보고서 |
| MRD v4.0 | `docs/planning/DOC-001_MRD_v3.0.md` | 시장 요구사항 문서 (v3.0 -> v4.0, 92개 MR) |
| PRD v3.0 | `docs/planning/DOC-002_PRD_v2.0.md` | 제품 요구사항 문서 (v2.0 -> v3.0, 17개 PR 추가) |

---

문서 최종 업데이트: 2026-04-09
