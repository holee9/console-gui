import { createInitialSettings } from "../shared/mock-data";
import type {
  AlertItem,
  DeliveryStudy,
  Locale,
  ProtocolPreset,
  SystemSettings,
  SystemStatus,
  WorkflowStage,
  WorklistStudy
} from "../shared/contracts";
import { deriveSafeState } from "../shared/policy";

export interface ConfirmParametersContext {
  alerts: AlertItem[];
  selectedProtocol: ProtocolPreset | null;
  selectedStudy: WorklistStudy | null;
}

export interface StartExposureContext extends ConfirmParametersContext {
  systemStatus: SystemStatus;
  workflowStage: WorkflowStage;
}

export interface BurnDiscResult {
  completedStudy: DeliveryStudy | null;
  nextQueue: DeliveryStudy[];
}

export interface AcknowledgeAlertResult {
  nextAlerts: AlertItem[];
  nextSystemStatus: SystemStatus;
}

export type SettingsSection = keyof SystemSettings;

export function canConfirmParameters({
  alerts,
  selectedProtocol,
  selectedStudy
}: ConfirmParametersContext): boolean {
  return Boolean(selectedStudy && selectedProtocol) && deriveSafeState(alerts) !== "Blocked";
}

export function canStartExposure({
  alerts,
  selectedProtocol,
  selectedStudy,
  systemStatus,
  workflowStage
}: StartExposureContext): boolean {
  return (
    Boolean(selectedStudy && selectedProtocol) &&
    workflowStage === "ReadyToExpose" &&
    deriveSafeState(alerts) !== "Blocked" &&
    systemStatus.parameterSync === "Acked" &&
    systemStatus.generator === "Ready" &&
    systemStatus.detector === "Ready"
  );
}

export function applyAcknowledgeAlert(
  alerts: AlertItem[],
  systemStatus: SystemStatus,
  alertId: string
): AcknowledgeAlertResult {
  const nextAlerts = alerts.filter((alert) => alert.id !== alertId);
  const nextSafeState = deriveSafeState(nextAlerts);

  return {
    nextAlerts,
    nextSystemStatus: {
      ...systemStatus,
      generator:
        nextSafeState === "Blocked"
          ? "Error"
          : systemStatus.generator === "Error"
            ? "Preparing"
            : systemStatus.generator,
      parameterSync:
        nextSafeState === "Blocked"
          ? "Error"
          : systemStatus.parameterSync === "Error"
            ? "Pending"
            : systemStatus.parameterSync,
      safeState: nextSafeState
    }
  };
}

export function applyBurnDisc(
  deliveryQueue: DeliveryStudy[],
  deliveryId: string
): BurnDiscResult {
  let completedStudy: DeliveryStudy | null = null;

  const nextQueue = deliveryQueue.map((study) => {
    if (study.id !== deliveryId) {
      return study;
    }

    completedStudy = study;

    const nextStudy: DeliveryStudy = {
      ...study,
      status: "Completed",
      progress: 100
    };

    return nextStudy;
  });

  return {
    completedStudy,
    nextQueue
  };
}

export function createSessionSettings(locale: Locale): SystemSettings {
  const nextSettings = createInitialSettings();

  return {
    ...nextSettings,
    ui: {
      ...nextSettings.ui,
      locale
    }
  };
}

export function isValidSettingValue(value: string | number | boolean): boolean {
  return typeof value !== "number" || !Number.isNaN(value);
}
