import { Navigate, Route, Routes } from "react-router-dom";
import type { ReactElement } from "react";
import AppShell from "./AppShell";
import { useAppState } from "./state";
import LoginPage from "../features/login/LoginPage";
import ConsolePage from "../features/console/ConsolePage";
import DeliveryPage from "../features/delivery/DeliveryPage";
import AdminPage from "../features/admin/AdminPage";
import { canAccessRoute, getDefaultRoute, type AppRoute } from "../shared/policy";

function AuthorizedRoute({
  route,
  element
}: {
  route: AppRoute;
  element: ReactElement;
}) {
  const { user } = useAppState();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (!canAccessRoute(user.role, route)) {
    return <Navigate to={getDefaultRoute(user.role)} replace />;
  }

  return element;
}

function ProtectedContent() {
  const { user } = useAppState();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  const defaultRoute = getDefaultRoute(user.role);

  return (
    <AppShell>
      <Routes>
        <Route
          path="/console"
          element={<AuthorizedRoute route="/console" element={<ConsolePage />} />}
        />
        <Route
          path="/delivery"
          element={<AuthorizedRoute route="/delivery" element={<DeliveryPage />} />}
        />
        <Route
          path="/admin"
          element={<AuthorizedRoute route="/admin" element={<AdminPage />} />}
        />
        <Route path="*" element={<Navigate to={defaultRoute} replace />} />
      </Routes>
    </AppShell>
  );
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/*" element={<ProtectedContent />} />
    </Routes>
  );
}
