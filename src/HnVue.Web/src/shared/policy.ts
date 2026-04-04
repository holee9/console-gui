import type { AlertItem, SafeState, UserRole } from "./contracts";

export type AppRoute = "/console" | "/delivery" | "/admin";

const routePermissions: Record<AppRoute, UserRole[]> = {
  "/console": ["Radiographer", "Radiologist"],
  "/delivery": ["Radiologist", "Admin"],
  "/admin": ["Admin", "Service"]
};

export function listAllowedRoutes(role: UserRole) {
  return (Object.keys(routePermissions) as AppRoute[]).filter((route) =>
    routePermissions[route].includes(role)
  );
}

export function canAccessRoute(role: UserRole, route: AppRoute) {
  return routePermissions[route].includes(role);
}

export function getDefaultRoute(role: UserRole): AppRoute {
  return listAllowedRoutes(role)[0] ?? "/console";
}

export function isBlockingAlert(alert: AlertItem) {
  return alert.severity === "critical" && alert.requiresAck;
}

export function deriveSafeState(alerts: AlertItem[]): SafeState {
  return alerts.some(isBlockingAlert) ? "Blocked" : "Idle";
}
