import type { Locale, UserRole } from "./contracts";

export const messages = {
  ko: {
    appTitle: "HnVue Web Validation Console",
    appSubtitle: "데스크톱 GUI와 동일한 업무 흐름을 웹에서 빠르게 검증하는 MVP",
    navConsole: "촬영 콘솔",
    navDelivery: "CD 배포",
    navAdmin: "시스템 설정",
    loginTitle: "웹 검증 워크스테이션",
    loginDescription:
      "이 화면은 실제 장비 제어가 아닌 사용성 검증용 웹 MVP입니다. 역할을 선택해 데스크톱 콘솔 흐름을 브라우저에서 점검합니다.",
    loginNote:
      "원래 구현 PC는 실제 제품 통합과 하드웨어 검증을 담당하고, 이 워크스테이션은 UI 흐름 검증에 집중합니다.",
    enterWorkspace: "검증 콘솔 진입",
    locale: "언어",
    signOut: "로그아웃",
    currentPatient: "현재 환자",
    systemState: "시스템 상태",
    worklist: "Worklist",
    sequence: "촬영 순서",
    workflow: "5클릭 워크플로우",
    searchPlaceholder: "환자명, ID, 촬영부위 검색",
    noMatches: "검색 결과가 없습니다.",
    viewer: "영상 검토",
    protocol: "프로토콜",
    parameters: "촬영 파라미터",
    detector: "디텍터 상태",
    dose: "선량 요약",
    alerts: "위험 경고",
    eventLog: "운영 로그",
    confirmParameters: "파라미터 확인",
    startExposure: "촬영 실행",
    sendToPacs: "PACS 전송",
    resetWorkflow: "워크플로우 초기화",
    emergencyRegister: "응급 등록",
    deliveryTitle: "환자 배포용 CD/DVD 굽기",
    deliveryDescription:
      "MR-072 시나리오를 기준으로 DICOM Viewer 포함 여부, 미디어 라벨, 진행 상태를 검증합니다.",
    includeViewer: "뷰어 포함",
    encryptDisc: "암호화",
    burnNow: "굽기 시작",
    adminTitle: "시스템 관리 및 유지보수",
    adminDescription:
      "DICOM 설정, 언어, 터치 모드, 자동 잠금, 서명된 업데이트 정책을 웹 시뮬레이션으로 검토합니다.",
    touchMode: "터치 모드",
    inactivityLock: "자동 잠금",
    signedUpdates: "서명된 업데이트만 허용",
    autoCheck: "자동 업데이트 점검",
    reviewedDocs: "문서 반영",
    reviewedDocsDescription:
      "PRD의 5패널 GUI 개념, SDS 디자인 토큰, UEF의 5클릭 시나리오를 웹 MVP 범위에 매핑했습니다."
  },
  en: {
    appTitle: "HnVue Web Validation Console",
    appSubtitle: "Browser-first MVP for validating the desktop operator workflow",
    navConsole: "Console",
    navDelivery: "CD Delivery",
    navAdmin: "Admin",
    loginTitle: "Web Validation Workstation",
    loginDescription:
      "This screen is a usability-validation MVP, not a live hardware console. Pick a role and review the desktop workflow in the browser.",
    loginNote:
      "The original implementation workstation remains responsible for production integration and hardware verification.",
    enterWorkspace: "Enter Validation Console",
    locale: "Language",
    signOut: "Sign out",
    currentPatient: "Current patient",
    systemState: "System state",
    worklist: "Worklist",
    sequence: "Sequence",
    workflow: "5-click workflow",
    searchPlaceholder: "Search patient, ID, or body part",
    noMatches: "No studies matched your search.",
    viewer: "Image review",
    protocol: "Protocol",
    parameters: "Exposure parameters",
    detector: "Detector status",
    dose: "Dose summary",
    alerts: "Risk alerts",
    eventLog: "Event log",
    confirmParameters: "Confirm parameters",
    startExposure: "Start exposure",
    sendToPacs: "Send to PACS",
    resetWorkflow: "Reset workflow",
    emergencyRegister: "Emergency register",
    deliveryTitle: "CD/DVD patient delivery",
    deliveryDescription:
      "Validate the MR-072 flow for viewer packaging, media labeling, and burn progress.",
    includeViewer: "Include viewer",
    encryptDisc: "Encrypt disc",
    burnNow: "Start burn",
    adminTitle: "System administration and service",
    adminDescription:
      "Review DICOM settings, language, touch mode, inactivity locking, and signed update policies in a web simulation.",
    touchMode: "Touch mode",
    inactivityLock: "Inactivity lock",
    signedUpdates: "Signed updates only",
    autoCheck: "Automatic update check",
    reviewedDocs: "Document alignment",
    reviewedDocsDescription:
      "The web MVP maps the PRD 5-panel GUI concept, SDS design tokens, and the UEF 5-click scenario."
  }
} as const;

export function getText(locale: Locale) {
  return messages[locale];
}

export function roleLabel(locale: Locale, role: UserRole) {
  const labels: Record<Locale, Record<UserRole, string>> = {
    ko: {
      Radiographer: "방사선사",
      Radiologist: "영상의학과 전문의",
      Admin: "관리자",
      Service: "서비스 엔지니어"
    },
    en: {
      Radiographer: "Radiographer",
      Radiologist: "Radiologist",
      Admin: "Administrator",
      Service: "Service engineer"
    }
  };

  return labels[locale][role];
}
