# UI-DESIGN-001: X-ray 콘솔 UI 디자인 → 코드 변환 리서치

| 항목 | 내용 |
|------|------|
| **문서 ID** | UI-DESIGN-001 |
| **버전** | v1.0 |
| **작성일** | 2026-04-08 |
| **대상 프로젝트** | HnVue Console SW (WPF .NET 8) |
| **목적** | 실제 판매 중인 X-ray 콘솔 제품 UI를 참고하여 코드로 변환하는 워크플로우 조사 |

---

## 1. 실제 판매 중인 X-ray 콘솔 SW UI 조사

### 1.1 주요 벤더별 콘솔 소프트웨어

| 벤더 | 제품명 | 플랫폼 | UI 특징 |
|------|--------|--------|---------|
| **Fujifilm** | FDX Console | Windows | 다크 테마, 좌측 환자 정보 + 중앙 이미지 뷰어 + 우측 도구 패널. 해부학적 아이콘 기반 메뉴. 커스터마이징 가능한 워크플로우 아이콘 배치 |
| **Carestream** | ImageView Software | Windows | 3-stage 레이아웃 — Left Stage(환자정보), Center Stage(이미지 리뷰), Right Stage(후처리 도구). 싱글 스크린 통합 뷰 |
| **Samsung** | XGEO GC80 SW | Windows | 직관적 GUI + 터치 스크린 지원. THU(30.4cm 터치)와 워크스테이션 동일 UI. 컬러 LED 상태 표시(블루=이동, 그린=준비, 오렌지=촬영, 레드=비상) |
| **Siemens** | Multix Fusion MAX | Windows 10 | DiamondView MAX 후처리. 역할 기반 접근제어(RBAC). 감사 추적 기능. 컬러 터치스크린 |
| **Shimadzu** | RADspeed Pro SR5 | Windows | VISION SUPPORT 카메라 오버레이. 모션 감지 기능. CXDI-DR 연동 |
| **DRTECH** | EConsole 1 | Windows | 워크리스트 → 촬영 → 스터디리스트 3단계 워크플로우. 해부학적 바디 맵 선택. Auto Stitching, Auto Labeling |
| **E-COM/White Mountain** | DROC | Windows | Symphony 이미지 처리. Virtual-man 기반 뷰 선택. AI 내장 이미지 처리. 풀 DICOM 지원 |
| **Visaris** | Avanse | Windows 10 | 5단계 워크플로우(환자선택→프로젝션→촬영→처리→내보내기). 노트북 기반 모바일 DR |

### 1.2 공통 UI 패턴 분석

리서치 결과 확인된 X-ray 콘솔 SW의 공통 UI 패턴:

#### 레이아웃
- **다크 테마** — 방사선실 어두운 환경에서 눈 피로 감소 (의료 영상 SW 표준)
- **3-패널 레이아웃** — 좌측(환자/워크리스트), 중앙(이미지 뷰어/메인 작업 영역), 우측(도구/파라미터)
- **상단 네비게이션 바** — 워크리스트, 촬영(Exam), 스터디리스트 간 탭 전환
- **하단 상태 바** — 디텍터 연결 상태, 제너레이터 상태, 시스템 알림

#### 워크플로우
1. **환자 선택/등록** — MWL 워크리스트 또는 수동 입력
2. **프로젝션 선택** — 해부학적 바디 맵(인체 실루엣) + 촬영 자세(AP/PA/Lat) 선택
3. **촬영** — 제너레이터 파라미터(kVp, mAs) 표시, 촬영 상태 실시간 모니터링
4. **이미지 리뷰/후처리** — W/L 조정, 확대/축소, 회전, 필터, 마커
5. **전송/저장** — DICOM PACS 전송, 프린트, CD/DVD 내보내기

#### 주요 UI 컴포넌트
- 해부학적 바디 맵 (인체 실루엣 + 클릭 영역)
- 썸네일 스트립 (촬영된 이미지 목록)
- 이미지 뷰어 (W/L, 줌, 팬, 회전, 측정 도구)
- 촬영 파라미터 패널 (kVp, mAs, APR 표시)
- 디텍터/제너레이터 상태 아이콘

---

## 2. Design-to-Code 도구/에이전트 조사

### 2.1 도구 비교표

| 도구 | 유형 | 입력 | 출력 | WPF XAML 지원 | 성숙도 | 비용 |
|------|------|------|------|:---:|------|------|
| **Figma MCP Server** | AI 에이전트 연동 | Figma 디자인 파일 | React, HTML/CSS, 코드 생성 | ✗ (HTML/React 중심) | Beta (2026.03~) | 베타 기간 무료 |
| **Figma + Xamlify** | Figma → XAML | Figma 파일 ID | .NET MAUI XAML | △ (MAUI, WPF 아님) | 개인 도구 수준 | Azure OpenAI 비용 |
| **Uno Platform Figma Plugin** | Figma → XAML | Figma 디자인 | Uno XAML (WinUI 기반) | △ (Uno ≈ WinUI) | 상용 제품 | Uno 라이선스 |
| **screenshot-to-code** | 오픈소스 | 스크린샷 이미지 | HTML + Tailwind CSS | ✗ | 활발한 오픈소스 | OpenAI API 비용 |
| **v0.dev (Vercel)** | SaaS | 스크린샷/프롬프트 | React + Tailwind | ✗ | 상용 SaaS | 구독 |
| **UI2Code.ai** | SaaS | UI 스크린샷 | Flutter, Swift, Kotlin, HTML | ✗ | 상용 SaaS | 구독 |
| **Screen2Code** | macOS 앱 | UI 스크린샷 | SwiftUI, React Native, Flutter | ✗ | 앱 수준 | 구독 |
| **GPT-4o Vision (직접 프롬프팅)** | API | 스크린샷 + 프롬프트 | 모든 형식 (XAML 포함) | ⚠️ 가능하나 품질 불안정 | API 직접 사용 | 토큰 비용 |
| **Claude Vision (직접 프롬프팅)** | API | 스크린샷 + 프롬프트 | 모든 형식 (XAML 포함) | ⚠️ 가능하나 품질 불안정 | API 직접 사용 | 토큰 비용 |
| **HTML/CSS to WPF XAML Converter** | Claude Code Skill | HTML/CSS | WPF XAML | ✅ (변환 전용) | MCP Market 스킬 | Claude 사용료 |
| **Syncfusion AI Coding Assistant** | MCP 어시스턴트 | 프롬프트 | WPF XAML + C# | ✅ (Syncfusion 컴포넌트) | 상용 MCP | Syncfusion 라이선스 |

### 2.2 핵심 발견

#### WPF XAML 직접 지원 도구는 거의 없다

대부분의 Design-to-Code 도구는 **웹 (HTML/CSS/React)** 또는 **모바일 (SwiftUI/Flutter)** 을 타겟으로 합니다. WPF XAML을 직접 출력하는 성숙한 도구는 현재 존재하지 않습니다.

#### Figma MCP Server (2026.03 출시)

[Figma 블로그](https://www.figma.com/blog/the-figma-canvas-is-now-open-to-agents/)에 따르면 2026년 3월에 AI 에이전트가 Figma 캔버스에서 직접 디자인할 수 있는 MCP 서버가 베타 출시되었습니다. 주요 기능:

- `generate_figma_design` — 웹 UI → Figma 레이어 변환
- `use_figma` — JavaScript Plugin API로 Figma 파일 직접 수정
- **Skills** — Markdown 기반 규칙으로 에이전트 행동 제어
- **Self-healing iteration** — 스크린샷 비교 → 자동 수정 루프

⚠️ **한계**: 출력은 Figma 레이어이므로, 여전히 Figma → WPF XAML 변환이 별도로 필요합니다.

#### Xamlify (Figma → .NET MAUI XAML)

[Hector Perez의 Xamlify 프로젝트](https://www.youtube.com/watch?v=999SyP5gKaQ)는 Figma API로 노드 정보를 추출한 뒤, Azure OpenAI를 통해 XAML 코드를 생성합니다. .NET MAUI 대상이지만, WPF XAML로 프롬프트를 수정하면 유사한 접근이 가능합니다. ⚠️ 다만 개인 프로젝트 수준이며 안정성은 보장되지 않습니다.

---

## 3. HnVue에 적용 가능한 워크플로우 분석

### 3.1 워크플로우 옵션 비교

| 워크플로우 | 단계 | 장점 | 단점 | 실현 가능성 |
|-----------|------|------|------|:---:|
| **A. 스크린샷 → LLM → XAML 직접** | 제품 스크린샷 → GPT-4o/Claude Vision → WPF XAML | 가장 빠름, 도구 최소 | XAML 품질 불안정, 반복 수정 필요 | ⚠️ 중 |
| **B. 스크린샷 → Figma → LLM → XAML** | 스크린샷 → Figma 디자인 → MCP → HTML → LLM → XAML | Figma에서 편집 가능, 디자인 시스템 활용 | 변환 체인이 길다, Figma 숙련 필요 | ⚠️ 중 |
| **C. 스크린샷 → HTML → WPF XAML 변환** | 스크린샷 → screenshot-to-code(HTML) → HTML-to-XAML 변환 | 중간 HTML 검증 가능 | 2단계 변환 품질 손실 | ⚠️ 중 |
| **D. 스크린샷 → 수동 Figma 디자인 → 수동 XAML** | 전통적 디자인 프로세스 | 품질 최고, 완전 제어 | 시간 소요 큼 (2인 팀에 비현실적) | ✗ 낮음 |
| **E. Perplexity 에이전트 활용 (권장)** | 스크린샷 수집 → 분석 → WPF XAML 코드 직접 생성 | 단일 환경, 즉시 빌드 검증, 반복 수정 가능 | LLM의 XAML 품질에 의존 | ✅ 높음 |

### 3.2 권장 워크플로우: Option E — Perplexity 에이전트 직접 활용

```
실제 제품 UI 스크린샷/영상 수집
        │
        ▼
  UI 패턴 분석 (레이아웃, 컬러, 컴포넌트 식별)
        │
        ▼
  화면별 WPF XAML + ViewModel 코드 생성
  (MahApps.Metro 다크 테마 + CommunityToolkit.Mvvm)
        │
        ▼
  feature/web-ui 브랜치에서 빌드 & 검증
        │
        ▼
  반복 수정 (스크린샷 비교 → 조정)
```

**이유:**

1. **도구 체인 최소화** — 외부 도구 없이 이 환경 안에서 스크린샷 분석 → XAML 생성 → 커밋까지 완결
2. **기존 기술 스택 활용** — 프로젝트에 이미 MahApps.Metro, CommunityToolkit.Mvvm, LiveCharts가 포함됨
3. **의료기기 특수성** — 의료기기 UI는 IEC 62366-1 사용적합성 요구사항이 있어 Figma 자동 변환보다는 의도적 설계가 더 적합
4. **2인 팀 현실성** — Figma 디자인 스킬 확보 비용 대비 직접 XAML 작성이 효율적

### 3.3 화면별 구현 우선순위

HnVue Console SW의 주요 화면을 경쟁 제품 패턴 기반으로 정리:

| 우선순위 | 화면 | 참고 제품 UI 패턴 | 핵심 컴포넌트 |
|:---:|------|------|------|
| 1 | **로그인** | 전 제품 공통 — 심플 로그인 | 사용자ID/PW, 역할 선택 |
| 2 | **워크리스트 (메인)** | Fujifilm FDX, Carestream ImageView | 환자 테이블, MWL 쿼리, 필터, 상태 아이콘 |
| 3 | **촬영 준비 (Exam Setup)** | DRTECH EConsole, E-COM DROC | 해부학적 바디 맵, 프로젝션 선택, APR 파라미터 |
| 4 | **촬영 실행 (Exposure)** | Samsung XGEO, Carestream | 제너레이터 상태, kVp/mAs 표시, 촬영 버튼, 안전 인터록 |
| 5 | **이미지 리뷰** | Fujifilm Dynamic Visualization, Carestream | 이미지 뷰어, W/L, 확대, 회전, 측정, Accept/Reject |
| 6 | **시스템 관리** | Siemens Multix Fusion | DICOM 설정, 사용자 관리, 감사 로그 |

---

## 4. 종합 판단

### 4.1 핵심 결론

| 질문 | 답변 |
|------|------|
| Figma → WPF XAML 자동 변환이 가능한가? | **실용적 수준에서는 아직 불가능.** Figma MCP는 HTML/React 중심이며, WPF XAML 직접 출력 도구는 미성숙 |
| 스크린샷 → WPF 코드 자동 변환이 가능한가? | **부분적 가능.** GPT-4o/Claude Vision으로 XAML 생성은 가능하나, 복잡한 의료 UI의 경우 수동 조정이 상당히 필요 |
| 가장 현실적인 접근은? | **경쟁 제품 UI 스크린샷을 레퍼런스로 삼아, 화면별로 WPF XAML을 직접 작성** (LLM 어시스트) |
| Pencil 도구는 유용한가? | **제한적.** Pencil은 와이어프레이밍 도구로, 코드 생성 기능이 없음. Figma가 더 적합하나 WPF 변환 문제는 동일 |

### 4.2 추천 실행 계획

1. **레퍼런스 UI 수집** — Fujifilm FDX, Carestream ImageView, DRTECH EConsole 영상/스크린샷 수집
2. **디자인 가이드 작성** — 다크 테마 팔레트, 레이아웃 그리드, 컴포넌트 라이브러리 정의
3. **화면별 XAML 구현** — Perplexity 에이전트로 화면별 View + ViewModel 코드 생성
4. **반복 검증** — 각 화면 완성 후 레퍼런스와 시각적 비교, 수정

### 4.3 리스크

| 리스크 | 영향 | 완화 방안 |
|--------|------|----------|
| LLM 생성 XAML의 레이아웃 정확도 | 중 | 화면을 작은 단위로 분할하여 생성, 단계별 검증 |
| 의료기기 UI 규제 (IEC 62366-1) | 높 | 사용적합성 파일(DOC-021)의 필수 UI 요구사항 먼저 정리 후 구현 |
| MahApps.Metro 테마 호환성 | 낮 | 이미 프로젝트에 포함되어 있으므로 호환성 검증됨 |
| 저작권 이슈 (타사 UI 복제) | 중 | UI 레이아웃/패턴은 저작권 대상이 아님. 아이콘/그래픽 에셋은 자체 제작 필수 |

---

## 5. 참고 자료

- Fujifilm FDX Console: https://healthcaresolutions-us.fujifilm.com/products/diagnostic-imaging/digital-radiography/image-processing-dector-technologies/fdx-console/
- Carestream ImageView: https://www.youtube.com/watch?v=I_9YG9alx-s
- Samsung XGEO GC80: https://www.idsa.org/awards-recognition/idea/idea-gallery/samsung-xgeo-gc80/
- DRTECH EConsole: https://www.drtech.co.kr/en/sub/product/view.php?s_cate=1110&idx=58
- E-COM DROC: https://whitemountainimaging.com/products/digital/e-com/ecom-droc/
- Visaris Avanse: https://visaris.com/development-of-the-new-mobile-x-ray-system/
- Siemens Multix Fusion MAX: https://www.deltamedicalsystems.com/wp-content/uploads/2021/02/Multix-Fusion-Max-Brochure-11-2019.pdf
- Shimadzu RADspeed Pro SR5: https://www.shimadzu-medical.eu/SR5-press-release
- Figma MCP Server (2026.03): https://www.figma.com/blog/the-figma-canvas-is-now-open-to-agents/
- Xamlify (Figma→MAUI XAML): https://www.youtube.com/watch?v=999SyP5gKaQ
- screenshot-to-code (오픈소스): https://screenshottocode.com/
- Adaptix Medical Imaging UI 디자인 사례: https://fruto.design/case-studies/medical-imaging-ui-radiology-tech-startup
- HTML/CSS to WPF XAML Converter (MCP Skill): https://mcpmarket.com/tools/skills/html-css-to-wpf-xaml-converter
- Syncfusion WPF AI Coding Assistant: https://www.syncfusion.com/blogs/post/syncfusion-ai-coding-assistant-wpf-winui-winforms
