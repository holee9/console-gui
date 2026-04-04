import { Navigate, Route, Routes } from "react-router-dom";
import AppShell from "./AppShell";
import { useAppState } from "./state";
import LoginPage from "../features/login/LoginPage";
import ConsolePage from "../features/console/ConsolePage";
import DeliveryPage from "../features/delivery/DeliveryPage";
import AdminPage from "../features/admin/AdminPage";

function ProtectedContent() {
  const { user } = useAppState();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  return (
    <AppShell>
      <Routes>
        <Route path="/console" element={<ConsolePage />} />
        <Route path="/delivery" element={<DeliveryPage />} />
        <Route path="/admin" element={<AdminPage />} />
        <Route path="*" element={<Navigate to="/console" replace />} />
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
