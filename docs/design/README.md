# HnVue Design Documentation

3계층 문서 아키텍처 중 Layer 2 (UI 디자인) 문서 저장소입니다.

## 폴더 구조

```
docs/design/
├── README.md              # 이 파일 — 문서 인덱스
├── UI_DESIGN_MASTER_REFERENCE.md  # 디자인 시스템 마스터 레퍼런스
├── spec/                  # UISPEC — 화면별 UI 디자인 명세서
│   ├── UISPEC-001_Login.md
│   ├── UISPEC-002_Worklist.md
│   ├── UISPEC-003_Studylist.md
│   ├── UISPEC-004_Acquisition.md
│   ├── UISPEC-005_AddPatient.md
│   ├── UISPEC-006_Merge.md
│   ├── UISPEC-007_Settings.md
│   ├── UISPEC-008_ImageViewer.md
│   └── UISPEC-009_SystemAdmin.md
├── analysis/              # 구현 현황 분석 보고서
│   ├── ANALYSIS-001_Login.md
│   └── ANALYSIS-002_Worklist.md
├── changelog/             # UI 변경 이력
│   └── UI_CHANGELOG.md
└── archive/               # 참고용 원본 자료
    └── PPT_ANALYSIS.md
```

## 문서 계층 구조

| Layer | 문서 유형 | 위치 | 목적 |
|-------|----------|------|------|
| Layer 1 | MRD, PRD | `docs/planning/` | 비즈니스 요구사항 |
| **Layer 2** | **UISPEC** | **`docs/design/spec/`** | **UI 디자인 명세** |
| Layer 3 | SPEC, Code | `.moai/specs/`, `src/` | 구현 명세 |

## UISPEC 문서 목적

UISPEC(UI Design Specification)은 PPT 디자인 시안과 코드 구현 사이의 브릿지 역할을 합니다.

- PPT 디자인 의도를 정확히 문서화
- MRD/PRD 요구사항과 트레이서빌리티 연결
- 구현 우선순위 및 Gap 명시
- WPF 토큰 매핑 포함

## 관련 문서

- MRD: `docs/planning/DOC-001_MRD_v3.0.md`
- PRD: `docs/planning/DOC-002_PRD_v4.0.md`
- 구현 SPEC: `.moai/specs/SPEC-UI-001/spec.md`
- UI Mockups: `docs/ui_mockups/`
- 원본 PPT: `docs/★HnVUE UI 변경 최종안_251118.pptx`
