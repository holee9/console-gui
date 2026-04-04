import {
  createContext,
  useContext,
  useState,
  type PropsWithChildren
} from "react";
import { getText } from "../shared/copy";
import {
  demoUsers,
  initialAlerts,
  initialAuditTrail,
  initialDeliveryQueue,
  initialDose,
  initialSettings,
  initialSystemStatus,
  protocolPresets,
  worklistStudies
} from "../shared/mock-data";
import type {
  AlertItem,
  AuditEvent,
  AuthUser,
  DeliveryStudy,
  Locale,
  ProtocolPreset,
  SystemSettings,
  SystemStatus,
  UserRole,
  WorkflowStage,
  WorklistStudy
} from "../shared/contracts";

type SettingsSection = keyof SystemSettings;
type SettingsValue = string | number | boolean;

interface AppState {
  locale: Locale;
  text: ReturnType<typeof getText>;
  user: AuthUser | null;
  worklist: WorklistStudy[];
  protocols: ProtocolPreset[];
  selectedStudy: WorklistStudy | null;
  selectedProtocol: ProtocolPreset | null;
  workflowStage: WorkflowStage;
  systemStatus: SystemStatus;
  alerts: AlertItem[];
  auditTrail: AuditEvent[];
  deliveryQueue: DeliveryStudy[];
  currentDose: typeof initialDose;
  settings: SystemSettings;
  loginAsRole: (role: UserRole) => void;
  logout: () => void;
  toggleLocale: () => void;
  selectStudy: (studyId: string) => void;
  selectProtocol: (protocolId: string) => void;
  confirmParameters: () => void;
  startExposure: () => void;
  sendToPacs: () => void;
  resetWorkflow: () => void;
  acknowledgeAlert: (alertId: string) => void;
  burnDisc: (deliveryId: string) => void;
  toggleDeliveryFlag: (deliveryId: string, key: "includeViewer" | "encryptDisc") => void;
  updateSetting: (section: SettingsSection, key: string, value: SettingsValue) => void;
}

const AppContext = createContext<AppState | null>(null);

function nowTime() {
  return new Date().toLocaleTimeString("en-GB", {
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit"
  });
}

export function AppProvider({ children }: PropsWithChildren) {
  const [locale, setLocale] = useState<Locale>("ko");
  const [user, setUser] = useState<AuthUser | null>(null);
  const [workflowStage, setWorkflowStage] = useState<WorkflowStage>("Idle");
  const [selectedStudyId, setSelectedStudyId] = useState<string | null>(worklistStudies[0]?.id ?? null);
  const [selectedProtocolId, setSelectedProtocolId] = useState<string | null>(protocolPresets[0]?.id ?? null);
  const [systemStatus, setSystemStatus] = useState<SystemStatus>(initialSystemStatus);
  const [alerts, setAlerts] = useState<AlertItem[]>(initialAlerts);
  const [auditTrail, setAuditTrail] = useState<AuditEvent[]>(initialAuditTrail);
  const [deliveryQueue, setDeliveryQueue] = useState<DeliveryStudy[]>(initialDeliveryQueue);
  const [currentDose, setCurrentDose] = useState(initialDose);
  const [settings, setSettings] = useState<SystemSettings>(initialSettings);

  const selectedStudy = worklistStudies.find((study) => study.id === selectedStudyId) ?? null;
  const selectedProtocol =
    protocolPresets.find((protocol) => protocol.id === selectedProtocolId) ?? null;

  function appendAudit(level: AuditEvent["level"], message: string) {
    setAuditTrail((current) => [
      {
        id: `EV-${current.length + 1}`,
        timestamp: nowTime(),
        level,
        message
      },
      ...current
    ].slice(0, 12));
  }

  function loginAsRole(role: UserRole) {
    setUser(demoUsers[role]);
    appendAudit("info", `${role} role entered the validation workspace.`);
  }

  function logout() {
    setUser(null);
    setWorkflowStage("Idle");
    appendAudit("info", "User left the validation workspace.");
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
    setSelectedStudyId(studyId);
    setWorkflowStage("PatientSelected");
    const study = worklistStudies.find((item) => item.id === studyId);

    if (study) {
      appendAudit("info", `Selected patient ${study.patientName} (${study.accessionNumber}).`);

      if (study.ageGroup === "Pediatric") {
        setAlerts((current) => {
          const exists = current.some((alert) => alert.id === "AL-PEDS");

          if (exists) {
            return current;
          }

          return [
            {
              id: "AL-PEDS",
              severity: "warning",
              title: "Pediatric dose guard",
              message: "Pediatric workflow selected. Confirm the low-dose preset before exposure.",
              requiresAck: false
            },
            ...current
          ];
        });
      }
    }
  }

  function selectProtocol(protocolId: string) {
    const protocol = protocolPresets.find((item) => item.id === protocolId);
    setSelectedProtocolId(protocolId);
    setWorkflowStage("ProtocolLoaded");

    if (protocol) {
      appendAudit("info", `Loaded protocol ${protocol.title}.`);

      if (selectedStudy?.ageGroup === "Pediatric" && !protocol.pediatric) {
        setAlerts((current) => [
          {
            id: `AL-MISMATCH-${protocolId}`,
            severity: "critical",
            title: "Protocol mismatch",
            message: "The selected patient is pediatric but the preset is not flagged for pediatric use.",
            requiresAck: true
          },
          ...current
        ]);
        setSystemStatus((current) => ({
          ...current,
          safeState: "Blocked"
        }));
      } else {
        setSystemStatus((current) => ({
          ...current,
          safeState: "Idle"
        }));
      }
    }
  }

  function confirmParameters() {
    setWorkflowStage("ReadyToExpose");
    setSystemStatus((current) => ({
      ...current,
      generator: "Ready",
      detector: "Ready"
    }));
    appendAudit("info", "Exposure parameters confirmed.");
  }

  function startExposure() {
    if (!selectedStudy || !selectedProtocol) {
      appendAudit("warning", "Exposure blocked because patient or protocol was not selected.");
      return;
    }

    setSystemStatus((current) => ({
      ...current,
      generator: "Exposing",
      detector: "Busy"
    }));

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
      generator: "Ready",
      detector: "Connected"
    }));
  }

  function sendToPacs() {
    setWorkflowStage("Completed");
    appendAudit("info", "Study marked as reviewed and queued for PACS transfer.");
  }

  function resetWorkflow() {
    setWorkflowStage("Idle");
    setSystemStatus(initialSystemStatus);
    appendAudit("info", "Workflow reset for the next patient.");
  }

  function acknowledgeAlert(alertId: string) {
    setAlerts((current) => current.filter((alert) => alert.id !== alertId));
    setSystemStatus((current) => ({
      ...current,
      safeState: "Idle"
    }));
    appendAudit("info", `Acknowledged alert ${alertId}.`);
  }

  function burnDisc(deliveryId: string) {
    setDeliveryQueue((current) =>
      current.map((study) =>
        study.id === deliveryId
          ? {
              ...study,
              status: "Completed",
              progress: 100
            }
          : study
      )
    );

    const study = deliveryQueue.find((item) => item.id === deliveryId);

    if (study) {
      appendAudit("info", `Disc package completed for ${study.patientName}.`);
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

  function updateSetting(section: SettingsSection, key: string, value: SettingsValue) {
    setSettings((current) => ({
      ...current,
      [section]: {
        ...current[section],
        [key]: value
      }
    }));

    appendAudit("info", `Updated ${section}.${key} in the validation settings panel.`);
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

export function useAppState() {
  const context = useContext(AppContext);

  if (!context) {
    throw new Error("useAppState must be used inside AppProvider.");
  }

  return context;
}
