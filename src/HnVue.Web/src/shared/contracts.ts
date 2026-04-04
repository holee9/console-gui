export type Locale = "ko" | "en";

export type UserRole = "Radiographer" | "Radiologist" | "Admin" | "Service";

export type SafeState = "Idle" | "Degraded" | "Blocked" | "Emergency";

export type WorkflowStage =
  | "Idle"
  | "PatientSelected"
  | "ProtocolLoaded"
  | "ReadyToExpose"
  | "ImageReview"
  | "Completed";

export type AlertSeverity = "critical" | "warning" | "info";

export interface AuthUser {
  userId: string;
  username: string;
  displayName: string;
  role: UserRole;
}

export interface WorklistStudy {
  id: string;
  accessionNumber: string;
  patientId: string;
  patientName: string;
  studyDate: string;
  bodyPart: string;
  requestedProcedure: string;
  urgency: "Routine" | "Emergency";
  ageGroup: "Adult" | "Pediatric";
  room: string;
}

export interface ProtocolPreset {
  id: string;
  title: string;
  bodyPart: string;
  orientation: string;
  kvp: number;
  mas: number;
  aec: "On" | "Off";
  detector: string;
  pediatric: boolean;
}

export interface DoseSnapshot {
  doseId: string;
  studyInstanceUid: string;
  dap: number;
  ei: number;
  di: number;
  effectiveDose: number;
  bodyPart: string;
  recordedAt: string;
}

export interface SystemStatus {
  detector: "Connected" | "Disconnected" | "Busy" | "Ready";
  generator: "Idle" | "Preparing" | "Ready" | "Exposing" | "Error";
  parameterSync: "Pending" | "Acked" | "Error";
  network: "Online" | "Degraded" | "Offline";
  storage: string;
  safeState: SafeState;
}

export interface AlertItem {
  id: string;
  severity: AlertSeverity;
  title: string;
  message: string;
  requiresAck: boolean;
}

export interface AuditEvent {
  id: string;
  timestamp: string;
  level: AlertSeverity;
  message: string;
}

export interface DeliveryStudy {
  id: string;
  patientName: string;
  studyLabel: string;
  discLabel: string;
  drive: string;
  includeViewer: boolean;
  encryptDisc: boolean;
  status: "Ready" | "Burning" | "Completed";
  progress: number;
}

export interface SystemSettings {
  network: {
    ip: string;
    gateway: string;
    dns: string;
  };
  dicom: {
    pacsAeTitle: string;
    risAeTitle: string;
    port: number;
    pollSeconds: number;
  };
  ui: {
    locale: Locale;
    touchMode: boolean;
    inactivityMinutes: number;
  };
  update: {
    channel: "stable" | "validation";
    autoCheck: boolean;
    signedOnly: boolean;
  };
}
