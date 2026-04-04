import assert from "node:assert/strict";
import test from "node:test";
import {
  applyAcknowledgeAlert,
  applyBurnDisc,
  canConfirmParameters,
  canStartExposure,
  createSessionSettings,
  isValidSettingValue
} from "../src/app/state-logic.ts";
import {
  createInitialSystemStatus,
  protocolPresets,
  worklistStudies
} from "../src/shared/mock-data.ts";

const adultStudy = worklistStudies[0];
const pediatricStudy = worklistStudies[1];
const chestProtocol = protocolPresets[0];

test("burnDisc returns the completed study and updates delivery progress for audit logging", () => {
  const result = applyBurnDisc(
    [
      {
        id: "DEL-1",
        patientName: "Kim^Minseo",
        studyLabel: "Chest PA / ACC-260404-001",
        discLabel: "HNVUE-P-24001",
        drive: "DVD-RW Bay-1",
        includeViewer: true,
        encryptDisc: false,
        status: "Ready",
        progress: 0
      }
    ],
    "DEL-1"
  );

  assert.equal(result.completedStudy?.patientName, "Kim^Minseo");
  assert.equal(result.nextQueue[0]?.status, "Completed");
  assert.equal(result.nextQueue[0]?.progress, 100);
});

test("acknowledgeAlert keeps the system blocked until every critical alert is cleared", () => {
  const alerts = [
    {
      id: "AL-1",
      severity: "critical" as const,
      title: "Mismatch",
      message: "Critical mismatch",
      requiresAck: true
    },
    {
      id: "AL-2",
      severity: "critical" as const,
      title: "Mismatch",
      message: "Critical mismatch",
      requiresAck: true
    }
  ];
  const blockedStatus = {
    ...createInitialSystemStatus(),
    generator: "Error" as const,
    parameterSync: "Error" as const,
    safeState: "Blocked" as const
  };

  const afterFirstAck = applyAcknowledgeAlert(alerts, blockedStatus, "AL-1");
  assert.equal(afterFirstAck.nextAlerts.length, 1);
  assert.equal(afterFirstAck.nextSystemStatus.safeState, "Blocked");
  assert.equal(afterFirstAck.nextSystemStatus.generator, "Error");
  assert.equal(afterFirstAck.nextSystemStatus.parameterSync, "Error");

  const afterSecondAck = applyAcknowledgeAlert(afterFirstAck.nextAlerts, blockedStatus, "AL-2");
  assert.equal(afterSecondAck.nextAlerts.length, 0);
  assert.equal(afterSecondAck.nextSystemStatus.safeState, "Idle");
  assert.equal(afterSecondAck.nextSystemStatus.generator, "Preparing");
  assert.equal(afterSecondAck.nextSystemStatus.parameterSync, "Pending");
});

test("confirmParameters only passes when patient and protocol exist with no blocking alerts", () => {
  assert.equal(
    canConfirmParameters({
      alerts: [],
      selectedStudy: adultStudy,
      selectedProtocol: chestProtocol
    }),
    true
  );

  assert.equal(
    canConfirmParameters({
      alerts: [
        {
          id: "AL-LOCK",
          severity: "critical",
          title: "Mismatch",
          message: "Critical mismatch",
          requiresAck: true
        }
      ],
      selectedStudy: pediatricStudy,
      selectedProtocol: chestProtocol
    }),
    false
  );
});

test("startExposure requires stage, ACK, generator, detector, and alert clearance", () => {
  const readyStatus = {
    ...createInitialSystemStatus(),
    generator: "Ready" as const,
    detector: "Ready" as const,
    parameterSync: "Acked" as const
  };

  assert.equal(
    canStartExposure({
      alerts: [],
      selectedStudy: adultStudy,
      selectedProtocol: chestProtocol,
      systemStatus: readyStatus,
      workflowStage: "ReadyToExpose"
    }),
    true
  );

  assert.equal(
    canStartExposure({
      alerts: [],
      selectedStudy: adultStudy,
      selectedProtocol: chestProtocol,
      systemStatus: {
        ...readyStatus,
        detector: "Connected"
      },
      workflowStage: "ReadyToExpose"
    }),
    false
  );

  assert.equal(
    canStartExposure({
      alerts: [],
      selectedStudy: adultStudy,
      selectedProtocol: chestProtocol,
      systemStatus: {
        ...readyStatus,
        parameterSync: "Pending"
      },
      workflowStage: "ReadyToExpose"
    }),
    false
  );
});

test("settings helpers preserve locale and reject NaN writes", () => {
  assert.equal(createSessionSettings("en").ui.locale, "en");
  assert.equal(isValidSettingValue(Number.NaN), false);
  assert.equal(isValidSettingValue(104), true);
});
