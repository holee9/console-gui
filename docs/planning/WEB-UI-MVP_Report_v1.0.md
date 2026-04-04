# Web UI MVP Report
## HnVue Console SW

---

## 목적

이 문서는 `feature/web-ui` 브랜치와 `src/HnVue.Web/` 작업 범위를 정리한 웹 UI MVP 보고서이다.  
웹 버전은 실제 장비 제어용 제품이 아니라, 원래 구현 PC에서 개발 중인 데스크톱 GUI의 정보 구조와 작업 흐름을 브라우저에서 빠르게 검증하기 위한 보조 검증 수단이다.

---

## 재검토한 문서와 반영 내용

### 1. PRD v2.0

참조: `docs/planning/DOC-002_PRD_v2.0.md`

- §5.3 GUI Layout Concept의 5패널 구조를 웹 콘솔 레이아웃의 기준으로 사용
- 상단 툴바, 좌측 Worklist, 중앙 영상 뷰어, 우측 촬영 제어, 하단 상태바를 동일한 정보 구조로 재구성
- MR-003의 5클릭 워크플로우를 웹 MVP 핵심 시나리오로 채택
- MR-045 한/영 UI 전환, MR-072 CD/DVD Burning을 웹 MVP 범위에 포함

### 2. SDS v2.0

참조: `docs/planning/DOC-007_SDS_v2.0.md`

- §3.8 UIFramework 모듈의 View/ViewModel 분해를 웹의 페이지/상태 구조에 대응
- 디자인 토큰의 색상, 포커스 링, 최소 터치 영역, 8px 기반 간격 체계를 CSS 변수로 반영
- 위험 경고, 포커스, 비활성 상태를 웹 컴포넌트 공통 스타일로 정리

### 3. Usability Engineering File v2.0

참조: `docs/testing/DOC-021_UsabilityFile_v2.0.md`

- 5클릭 표준 촬영 시나리오를 콘솔 MVP의 기본 동선으로 사용
- 방사선사 1순위 사용자 프로파일을 UI 우선순위의 기준으로 설정
- 소아 촬영, 위험 경고 ACK, CD 굽기, 한/영 전환을 사용성 검증 항목으로 포함

---

## 웹 MVP 범위

### 포함

- 역할 선택형 로그인 진입
- 메인 촬영 콘솔 5패널 레이아웃
- Worklist 선택, 프로토콜 선택, 파라미터 확인, 촬영 실행, PACS 전송
- 영상 검토용 중앙 뷰어와 선량 요약
- 위험 경고 표시 및 ACK 흐름
- CD/DVD 배포 화면
- 시스템 설정, DICOM, 네트워크, 언어, 터치 모드, 자동 잠금 정책
- 한국어/영어 전환
- `HnVue.Common`을 참고한 모의 계약/데이터 구조

### 제외

- 실제 Generator/Detector 제어
- 실제 DICOM 송수신
- 실제 인증, JWT, RBAC 서버 연동
- 실제 장치 드라이버/하드웨어 테스트
- 실제 필드 배포 패키징

---

## 구현 구조

경로: `src/HnVue.Web/`

- `src/app/`: 앱 셸, 전역 상태, 라우팅
- `src/features/login/`: 역할 선택형 로그인 화면
- `src/features/console/`: 메인 웹 콘솔
- `src/features/delivery/`: CD/DVD 배포 시나리오
- `src/features/admin/`: 관리/설정 시나리오
- `src/shared/`: 계약 타입, 다국어 문자열, 모의 데이터
- `src/styles/`: SDS 토큰 기반 공용 스타일

---

## 웹 대응 원칙

- 데스크톱과 1:1 코드 공유보다, 정보 구조와 업무 흐름의 동일성을 우선한다.
- 웹 UI는 사용성 검증 도구이며, 실제 배포 제품은 여전히 데스크톱 앱이다.
- 공용 모듈이 확정되면 웹의 모의 계약을 실제 API 계약으로 대체할 수 있도록 구조를 단순하게 유지한다.
- `main`은 제품 기준선으로 유지하고, 웹 UI 실험과 빠른 MVP는 `feature/web-ui`에서 지속한다.

---

## 다음 단계

1. `src/HnVue.Web/` 의 모의 계약을 실제 데스크톱 ViewModel 계약과 비교
2. 촬영 콘솔의 세부 인터랙션을 사용자 피드백 기반으로 정리
3. 필요한 경우 `HnVue.Common` 기반 DTO 또는 계약 문서만 선택 반영
4. 이후 원래 구현 PC에서 실제 WPF 화면 설계에 반영
