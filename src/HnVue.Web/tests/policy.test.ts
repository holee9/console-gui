import test from "node:test";
import assert from "node:assert/strict";
import { createInitialAlerts, createInitialSystemStatus } from "../src/shared/mock-data.ts";
import {
  canAccessRoute,
  deriveSafeState,
  getDefaultRoute,
  listAllowedRoutes
} from "../src/shared/policy.ts";

test("RBAC routes follow the documented validation matrix", () => {
  assert.deepEqual(listAllowedRoutes("Radiographer"), ["/console"]);
  assert.deepEqual(listAllowedRoutes("Radiologist"), ["/console", "/delivery"]);
  assert.deepEqual(listAllowedRoutes("Admin"), ["/delivery", "/admin"]);
  assert.deepEqual(listAllowedRoutes("Service"), ["/admin"]);

  assert.equal(canAccessRoute("Radiographer", "/admin"), false);
  assert.equal(canAccessRoute("Admin", "/delivery"), true);
  assert.equal(getDefaultRoute("Service"), "/admin");
});

test("safe state remains blocked while any critical ACK alert is unresolved", () => {
  const alerts = [
    {
      id: "AL-MISMATCH-1",
      severity: "critical" as const,
      title: "Protocol mismatch",
      message: "Mismatch",
      requiresAck: true
    },
    {
      id: "AL-MISMATCH-2",
      severity: "critical" as const,
      title: "Protocol mismatch",
      message: "Mismatch",
      requiresAck: true
    }
  ];

  assert.equal(deriveSafeState(alerts), "Blocked");
  assert.equal(deriveSafeState(alerts.slice(1)), "Blocked");
  assert.equal(deriveSafeState(createInitialAlerts()), "Idle");
});

test("initial validation state starts clean instead of preselecting a blocked workflow", () => {
  const status = createInitialSystemStatus();

  assert.equal(status.generator, "Idle");
  assert.equal(status.detector, "Connected");
  assert.equal(status.parameterSync, "Pending");
  assert.equal(status.safeState, "Idle");
});
