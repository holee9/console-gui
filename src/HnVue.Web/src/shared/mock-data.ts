import type {
  AlertItem,
  AuditEvent,
  AuthUser,
  DeliveryStudy,
  DoseSnapshot,
  ProtocolPreset,
  SystemSettings,
  SystemStatus,
  UserRole,
  WorklistStudy
} from "./contracts";

export const demoUsers: Record<UserRole, AuthUser> = {
  Radiographer: {
    userId: "USR-RT-001",
    username: "rt.kim",
    displayName: "Kim RT",
    role: "Radiographer"
  },
  Radiologist: {
    userId: "USR-RD-002",
    username: "rd.lee",
    displayName: "Lee MD",
    role: "Radiologist"
  },
  Admin: {
    userId: "USR-AD-003",
    username: "admin.park",
    displayName: "Park Admin",
    role: "Admin"
  },
  Service: {
    userId: "USR-SV-004",
    username: "svc.choi",
    displayName: "Choi Service",
    role: "Service"
  }
};

export const worklistStudies: WorklistStudy[] = [
  {
    id: "WL-001",
    accessionNumber: "ACC-260404-001",
    patientId: "P-24001",
    patientName: "Kim^Minseo",
    studyDate: "2026-04-04",
    bodyPart: "CHEST",
    requestedProcedure: "Chest PA",
    urgency: "Routine",
    ageGroup: "Adult",
    room: "XR-1"
  },
  {
    id: "WL-002",
    accessionNumber: "ACC-260404-002",
    patientId: "P-24002",
    patientName: "Han^Yejun",
    studyDate: "2026-04-04",
    bodyPart: "KNEE",
    requestedProcedure: "Knee AP/LAT",
    urgency: "Emergency",
    ageGroup: "Pediatric",
    room: "ER-XR"
  },
  {
    id: "WL-003",
    accessionNumber: "ACC-260404-003",
    patientId: "P-24003",
    patientName: "Seo^Jiyoon",
    studyDate: "2026-04-04",
    bodyPart: "ABDOMEN",
    requestedProcedure: "Abdomen Supine",
    urgency: "Routine",
    ageGroup: "Adult",
    room: "XR-2"
  }
];

export const protocolPresets: ProtocolPreset[] = [
  {
    id: "PR-001",
    title: "Chest PA Standard",
    bodyPart: "CHEST",
    orientation: "PA",
    kvp: 110,
    mas: 2.4,
    aec: "On",
    detector: "FPD-A",
    pediatric: false
  },
  {
    id: "PR-002",
    title: "Chest AP Portable",
    bodyPart: "CHEST",
    orientation: "AP",
    kvp: 95,
    mas: 3.2,
    aec: "Off",
    detector: "FPD-B",
    pediatric: false
  },
  {
    id: "PR-003",
    title: "Pediatric Knee Low Dose",
    bodyPart: "KNEE",
    orientation: "AP/LAT",
    kvp: 58,
    mas: 1.4,
    aec: "Off",
    detector: "FPD-A",
    pediatric: true
  },
  {
    id: "PR-004",
    title: "Abdomen Routine",
    bodyPart: "ABDOMEN",
    orientation: "AP",
    kvp: 78,
    mas: 8.6,
    aec: "On",
    detector: "FPD-C",
    pediatric: false
  }
];

export function createInitialDose(): DoseSnapshot {
  return {
    doseId: "DOSE-PENDING",
    studyInstanceUid: "Pending",
    dap: 0,
    ei: 0,
    di: 0,
    effectiveDose: 0,
    bodyPart: "Pending",
    recordedAt: ""
  };
}

export function createInitialSystemStatus(): SystemStatus {
  return {
    detector: "Connected",
    generator: "Idle",
    parameterSync: "Pending",
    network: "Online",
    storage: "74%",
    safeState: "Idle"
  };
}

export function createInitialAlerts(): AlertItem[] {
  return [];
}

export function createInitialAuditTrail(): AuditEvent[] {
  return [
    {
      id: "EV-001",
      timestamp: "10:42:15",
      level: "info",
      message: "Web validation workspace initialized from feature/web-ui."
    },
    {
      id: "EV-002",
      timestamp: "10:42:18",
      level: "info",
      message: "PRD 5-panel layout and UEF 5-click scenario loaded into the MVP shell."
    }
  ];
}

export function createInitialDeliveryQueue(): DeliveryStudy[] {
  return worklistStudies.map((study, index) => ({
    id: `DEL-${index + 1}`,
    patientName: study.patientName,
    studyLabel: `${study.requestedProcedure} / ${study.accessionNumber}`,
    discLabel: `HNVUE-${study.patientId}`,
    drive: index === 1 ? "USB Writer Bay-2" : "DVD-RW Bay-1",
    includeViewer: true,
    encryptDisc: index === 1,
    status: "Ready",
    progress: 0
  }));
}

export function createInitialSettings(): SystemSettings {
  return {
    network: {
      ip: "192.168.12.24",
      gateway: "192.168.12.1",
      dns: "8.8.8.8"
    },
    dicom: {
      pacsAeTitle: "HNVUE_PACS",
      risAeTitle: "HNVUE_MWL",
      port: 104,
      pollSeconds: 10
    },
    ui: {
      locale: "ko",
      touchMode: true,
      inactivityMinutes: 15
    },
    update: {
      channel: "validation",
      autoCheck: true,
      signedOnly: true
    }
  };
}
