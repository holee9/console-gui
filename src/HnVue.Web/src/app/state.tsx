import {
  createContext,
  useContext,
  useRef,
  useState,
  type PropsWithChildren,
  type ReactElement
} from "react";
import {
  applyAcknowledgeAlert,
  applyBurnDisc,
  canConfirmParameters,
  canStartExposure,
  createSessionSettings,
  isValidSettingValue,
  type SettingsSection
} from "./state-logic";
import { getText } from "../shared/copy";
import {
  createInitialAlerts,
  createInitialAuditTrail,
  createInitialDeliveryQueue,
  createInitialDose,
  createInitialSettings,
  createInitialSystemStatus,
  demoUsers,
  protocolPresets,
  worklistStudies
} from "../shared/mock-data";
import type {
  AlertItem,
  AuditEvent,
  AuthUser,
  DeliveryStudy,
  DoseSnapshot,
  Locale,
  ProtocolPreset,
  SystemSettings,
  SystemStatus,
  UserRole,
  WorkflowStage,
  WorklistStudy
} from "../shared/contracts";
import { deriveSafeState } from "../shared/policy";

interface AppState {
  locale: Locale;
  text: ReturnType<typeof getText>;
  user: AuthUser | null;
  worklist: WorklistStudy[];
  protocols: ProtocolPreset[];
  selectedStudy: WorklistStudy | null;
  selectedProtocol: ProtocolPreset | null;
  workflowStage: WorkflowStage;
  workflowClicks: number;
  systemStatus: SystemStatus;
  alerts: AlertItem[];
  auditTrail: AuditEvent[];
  deliveryQueue: DeliveryStudy[];
  currentDose: DoseSnapshot;
  settings: SystemSettings;
  loginAsRole: (role: UserRole) => void;
  logout: () => void;
  toggleLocale: () => void;
  selectStudy: (studyId: string) => void;
  selectProtocol: (protocolId: string) => void;
  launchEmergencyWorkflow: () => void;
  confirmParameters: () => void;
  startExposure: () => void;
  sendToPacs: () => void;
  resetWorkflow: () => void;
  acknowledgeAlert: (alertId: string) => void;
  burnDisc: (deliveryId: string) => void;
  toggleDeliveryFlag: (deliveryId: string, key: "includeViewer" | "encryptDisc") => void;
  updateSetting: <S extends SettingsSection, K extends keyof SystemSettings[S]>(
    section: S,
    key: K,
    value: SystemSettings[S][K]
  ) => void;
}

const AppContext = createContext<AppState | null>(null);

function nowTime() {
  return new Date().toLocaleTimeString("en-GB", {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit"
  });
}

function createPediatricGuardAlert(): AlertItem {
  return {
    id: "AL-PEDS",
    severity: "warning",
    title: "Pediatric dose guard",
    message: "Pediatric workflow selected. Confirm the low-dose preset before exposure.",
    requiresAck: false
  };
}

function createProtocolMismatchAlert(protocolId: string): AlertItem {
  return {
    id: `AL-MISMATCH-${protocolId}`,
    severity: "critical",
    title: "Protocol mismatch",
    message: "The selected patient is pediatric but the preset is not flagged for pediatric use.",
    requiresAck: true
  };
}

function deriveAlerts(
  study: WorklistStudy | null,
  protocol: ProtocolPreset | null
) {
  if (!study) {
    return createInitialAlerts();
  }

  if (study.ageGroup !== "Pediatric") {
    return createInitialAlerts();
  }

  if (!protocol) {
    return [createPediatricGuardAlert()];
  }

  return protocol.pediatric
    ? [createPediatricGuardAlert()]
    : [createProtocolMismatchAlert(protocol.id), createPediatricGuardAlert()];
}

function resetWorkflowArtifacts() {
  return {
    selectedStudyId: null,
    selectedProtocolId: null,
    workflowStage: "Idle" as WorkflowStage,
    workflowClicks: 0,
    alerts: createInitialAlerts(),
    currentDose: createInitialDose(),
    systemStatus: createInitialSystemStatus()
  };
}

export function AppProvider({ children }: PropsWithChildren): ReactElement {
  const [locale, setLocale] = useState<Locale>("ko");
  const [user, setUser] = useState<AuthUser | null>(null);
  const [workflowStage, setWorkflowStage] = useState<WorkflowStage>("Idle");
  const [workflowClicks, setWorkflowClicks] = useState(0);
  const [selectedStudyId, setSelectedStudyId] = useState<string | null>(null);
  const [selectedProtocolId, setSelectedProtocolId] = useState<string | null>(null);
  const [systemStatus, setSystemStatus] = useState<SystemStatus>(() => createInitialSystemStatus());
  const [alerts, setAlerts] = useState<AlertItem[]>(() => createInitialAlerts());
  const [auditTrail, setAuditTrail] = useState<AuditEvent[]>(() => createInitialAuditTrail());
  const [deliveryQueue, setDeliveryQueue] = useState<DeliveryStudy[]>(() => createInitialDeliveryQueue());
  const [currentDose, setCurrentDose] = useState<DoseSnapshot>(() => createInitialDose());
  const [settings, setSettings] = useState<SystemSettings>(() => createInitialSettings());
  const auditCounter = useRef(createInitialAuditTrail().length);

  const selectedStudy = worklistStudies.find((study) => study.id === selectedStudyId) ?? null;
  const selectedProtocol =
    protocolPresets.find((protocol) => protocol.id === selectedProtocolId) ?? null;

  function appendAudit(level: AuditEvent["level"], message: string) {
    auditCounter.current += 1;
    setAuditTrail((current) => [
      {
        id: `EV-${String(auditCounter.current).padStart(3, "0")}`,
        timestamp: nowTime(),
        level,
        message
      },
      ...current
    ].slice(0, 12));
  }

  function resetSessionState(nextUser: AuthUser | null) {
    const session = resetWorkflowArtifacts();

    setUser(nextUser);
    setSelectedStudyId(session.selectedStudyId);
    setSelectedProtocolId(session.selectedProtocolId);
    setWorkflowStage(session.workflowStage);
    setWorkflowClicks(session.workflowClicks);
    setAlerts(session.alerts);
    setCurrentDose(session.currentDose);
    setSystemStatus(session.systemStatus);
    setDeliveryQueue(createInitialDeliveryQueue());
    setSettings((current) => createSessionSettings(current.ui.locale));
    setAuditTrail(createInitialAuditTrail());
    auditCounter.current = createInitialAuditTrail().length;
  }

  function loginAsRole(role: UserRole) {
    resetSessionState(demoUsers[role]);
    appendAudit("info", `${role} role entered the validation workspace.`);
  }

  function logout() {
    resetSessionState(null);
  }

  function toggleLocale() {
    setLocale((current) => {
      const next = current === "ko" ? "en" : "ko";
      setSettings((value) => ({
        ...value,
        ui: {
          ...value.ui,
          locale: next
        }
      }));
      return next;
    });
  }

  function selectStudy(studyId: string) {
    const study = worklistStudies.find((item) => item.id === studyId);

    if (!study) {
      return;
    }

    const nextAlerts = deriveAlerts(study, null);

    setSelectedStudyId(studyId);
    setSelectedProtocolId(null);
    setWorkflowStage("PatientSelected");
    setWorkflowClicks(1);
    setAlerts(nextAlerts);
    setCurrentDose(createInitialDose());
    setSystemStatus((current) => ({
      ...current,
      detector: "Connected",
      generator: "Idle",
      parameterSync: "Pending",
      safeState: deriveSafeState(nextAlerts)
    }));
    appendAudit("info", `Selected patient ${study.patientName} (${study.accessionNumber}).`);
  }

  function selectProtocol(protocolId: string) {
    const protocol = protocolPresets.find((item) => item.id === protocolId);

    if (!selectedStudy || !protocol) {
      appendAudit("warning", "Protocol selection requires a patient context.");
      return;
    }

    const nextAlerts = deriveAlerts(selectedStudy, protocol);

    setSelectedProtocolId(protocolId);
    setWorkflowStage("ProtocolLoaded");
    setWorkflowClicks((current) => current + 1);
    setAlerts(nextAlerts);
    setSystemStatus((current) => ({
      ...current,
      generator: nextAlerts.some((alert) => alert.severity === "critical") ? "Error" : "Preparing",
      parameterSync: nextAlerts.some((alert) => alert.severity === "critical") ? "Error" : "Pending",
      safeState: deriveSafeState(nextAlerts)
    }));
    appendAudit("info", `Loaded protocol ${protocol.title}.`);
  }

  function launchEmergencyWorkflow() {
    const emergencyStudy =
      worklistStudies.find((study) => study.urgency === "Emergency") ?? worklistStudies[0] ?? null;

    if (!emergencyStudy) {
      appendAudit("warning", "No emergency-ready study exists in the mock worklist.");
      return;
    }

    const emergencyProtocol =
      protocolPresets.find(
        (protocol) =>
          protocol.bodyPart === emergencyStudy.bodyPart &&
          (emergencyStudy.ageGroup !== "Pediatric" || protocol.pediatric)
      ) ??
      protocolPresets.find((protocol) => protocol.bodyPart === emergencyStudy.bodyPart) ??
      null;
    const nextAlerts = deriveAlerts(emergencyStudy, emergencyProtocol);
    const hasCriticalAlert = nextAlerts.some((alert) => alert.severity === "critical");

    setSelectedStudyId(emergencyStudy.id);
    setSelectedProtocolId(emergencyProtocol?.id ?? null);
    setWorkflowStage(emergencyProtocol ? "ProtocolLoaded" : "PatientSelected");
    setWorkflowClicks(1);
    setAlerts(nextAlerts);
    setCurrentDose(createInitialDose());
    setSystemStatus((current) => ({
      ...current,
      detector: "Connected",
      generator: emergencyProtocol ? (hasCriticalAlert ? "Error" : "Preparing") : "Idle",
      parameterSync: emergencyProtocol ? (hasCriticalAlert ? "Error" : "Pending") : "Pending",
      safeState: deriveSafeState(nextAlerts)
    }));

    appendAudit(
      "warning",
      `Emergency start loaded ${emergencyStudy.patientName} with ${emergencyProtocol?.title ?? "no protocol selected yet"}.`
    );
  }

  function confirmParameters() {
    if (!selectedStudy || !selectedProtocol) {
      appendAudit("warning", "Parameter confirmation requires both a patient and a protocol.");
      return;
    }

    if (!canConfirmParameters({ alerts, selectedStudy, selectedProtocol })) {
      appendAudit("warning", "Parameter confirmation paused until critical alerts are acknowledged.");
      return;
    }

    setWorkflowStage("ReadyToExpose");
    setWorkflowClicks((current) => current + 1);
    setSystemStatus((current) => ({
      ...current,
      parameterSync: "Acked",
      generator: "Ready",
      detector: "Ready"
    }));
    appendAudit("info", "Exposure parameters confirmed after generator ACK simulation.");
  }

  function startExposure() {
    if (!selectedStudy || !selectedProtocol) {
      appendAudit("warning", "Exposure blocked because patient or protocol was not selected.");
      return;
    }

    if (!canStartExposure({ alerts, selectedStudy, selectedProtocol, systemStatus, workflowStage })) {
      appendAudit(
        "warning",
        "Exposure blocked because generator ACK, detector readiness, or critical-alert clearance is incomplete."
      );
      return;
    }

    setSystemStatus((current) => ({
      ...current,
      generator: "Exposing",
      detector: "Busy"
    }));
    setWorkflowClicks((current) => current + 1);

    setCurrentDose({
      doseId: `DOSE-${selectedStudy.id}`,
      studyInstanceUid: `1.2.410.1000.${selectedStudy.accessionNumber}`,
      dap: Number((selectedProtocol.mas / 3.6).toFixed(2)),
      ei: Math.round(360 + selectedProtocol.kvp),
      di: selectedStudy.ageGroup === "Pediatric" ? -1.2 : -0.2,
      effectiveDose: Number((selectedProtocol.mas / 20).toFixed(2)),
      bodyPart: selectedStudy.bodyPart,
      recordedAt: new Date().toISOString()
    });

    setWorkflowStage("ImageReview");
    appendAudit(
      "info",
      `Exposure simulated for ${selectedStudy.patientName} with ${selectedProtocol.title}.`
    );

    setSystemStatus((current) => ({
      ...current,
      parameterSync: "Pending",
      generator: "Ready",
      detector: "Connected"
    }));
  }

  function sendToPacs() {
    if (workflowStage !== "ImageReview" && workflowStage !== "Completed") {
      appendAudit("warning", "PACS transfer is available after image review.");
      return;
    }

    setWorkflowClicks((current) => current + 1);
    setWorkflowStage("Completed");
    appendAudit("info", "Study marked as reviewed and queued for PACS transfer.");
  }

  function resetWorkflow() {
    const session = resetWorkflowArtifacts();
    setSelectedStudyId(session.selectedStudyId);
    setSelectedProtocolId(session.selectedProtocolId);
    setWorkflowStage(session.workflowStage);
    setWorkflowClicks(session.workflowClicks);
    setAlerts(session.alerts);
    setCurrentDose(session.currentDose);
    setSystemStatus(session.systemStatus);
    appendAudit("info", "Workflow reset for the next patient.");
  }

  function acknowledgeAlert(alertId: string) {
    const resolved = applyAcknowledgeAlert(alerts, systemStatus, alertId);

    setAlerts(resolved.nextAlerts);
    setSystemStatus(resolved.nextSystemStatus);
    appendAudit("info", `Acknowledged alert ${alertId}.`);
  }

  function burnDisc(deliveryId: string) {
    const result = applyBurnDisc(deliveryQueue, deliveryId);

    setDeliveryQueue(result.nextQueue);

    if (result.completedStudy) {
      appendAudit(
        "info",
        `Disc package completed for ${result.completedStudy.patientName} after PHI review and read-back verify.`
      );
    }
  }

  function toggleDeliveryFlag(
    deliveryId: string,
    key: "includeViewer" | "encryptDisc"
  ) {
    setDeliveryQueue((current) =>
      current.map((study) =>
        study.id === deliveryId
          ? {
              ...study,
              [key]: !study[key]
            }
          : study
      )
    );
  }

  function updateSetting<S extends SettingsSection, K extends keyof SystemSettings[S]>(
    section: S,
    key: K,
    value: SystemSettings[S][K]
  ) {
    if (!isValidSettingValue(value as string | number | boolean)) {
      appendAudit(
        "warning",
        `Ignored invalid value for ${String(section)}.${String(key)} in the settings panel.`
      );
      return;
    }

    setSettings((current) => ({
      ...current,
      [section]: {
        ...current[section],
        [key]: value
      }
    }));

    appendAudit("info", `Updated ${String(section)}.${String(key)} in the validation settings panel.`);
  }

  return (
    <AppContext.Provider
      value={{
        locale,
        text: getText(locale),
        user,
        worklist: worklistStudies,
        protocols: protocolPresets,
        selectedStudy,
        selectedProtocol,
        workflowStage,
        workflowClicks,
        systemStatus,
        alerts,
        auditTrail,
        deliveryQueue,
        currentDose,
        settings,
        loginAsRole,
        logout,
        toggleLocale,
        selectStudy,
        selectProtocol,
        launchEmergencyWorkflow,
        confirmParameters,
        startExposure,
        sendToPacs,
        resetWorkflow,
        acknowledgeAlert,
        burnDisc,
        toggleDeliveryFlag,
        updateSetting
      }}
    >
      {children}
    </AppContext.Provider>
  );
}

export function useAppState(): AppState {
  const context = useContext(AppContext);

  if (!context) {
    throw new Error("useAppState must be used inside AppProvider.");
  }

  return context;
}
