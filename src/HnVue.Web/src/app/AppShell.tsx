import { NavLink } from "react-router-dom";
import { roleLabel } from "../shared/copy";
import { useAppState } from "./state";
import type { PropsWithChildren } from "react";

const statusToneClass = {
  Idle: "is-success",
  Degraded: "is-warning",
  Blocked: "is-danger",
  Emergency: "is-danger",
  Online: "is-success",
  Connected: "is-success",
  Ready: "is-success",
  Busy: "is-warning",
  Preparing: "is-warning",
  Exposing: "is-danger",
  Offline: "is-danger",
  Disconnected: "is-danger",
  Error: "is-danger"
} as const;

export default function AppShell({ children }: PropsWithChildren) {
  const { locale, text, user, logout, selectedStudy, systemStatus, toggleLocale } = useAppState();

  return (
    <div className="app-shell">
      <header className="top-shell">
        <div className="brand-block">
          <span className="eyebrow">HnVue / Web Validation</span>
          <h1>{text.appTitle}</h1>
          <p>{text.appSubtitle}</p>
        </div>

        <nav className="shell-nav" aria-label="Primary">
          <NavLink to="/console" className="nav-pill">
            {text.navConsole}
          </NavLink>
          <NavLink to="/delivery" className="nav-pill">
            {text.navDelivery}
          </NavLink>
          <NavLink to="/admin" className="nav-pill">
            {text.navAdmin}
          </NavLink>
        </nav>

        <div className="header-actions">
          <button type="button" className="secondary-button" onClick={toggleLocale}>
            {text.locale}: {locale.toUpperCase()}
          </button>
          <button type="button" className="ghost-button" onClick={logout}>
            {text.signOut}
          </button>
        </div>
      </header>

      <section className="shell-summary">
        <article className="summary-card">
          <span className="summary-label">{text.currentPatient}</span>
          <strong>{selectedStudy?.patientName ?? "Unassigned"}</strong>
          <span className="summary-muted">
            {selectedStudy ? `${selectedStudy.bodyPart} / ${selectedStudy.accessionNumber}` : "No worklist selection"}
          </span>
        </article>

        <article className="summary-card">
          <span className="summary-label">{text.systemState}</span>
          <div className="status-row">
            <span className={`status-chip ${statusToneClass[systemStatus.safeState]}`}>
              Safe: {systemStatus.safeState}
            </span>
            <span className={`status-chip ${statusToneClass[systemStatus.generator]}`}>
              GEN: {systemStatus.generator}
            </span>
            <span className={`status-chip ${statusToneClass[systemStatus.detector]}`}>
              DET: {systemStatus.detector}
            </span>
            <span className={`status-chip ${statusToneClass[systemStatus.network]}`}>
              NET: {systemStatus.network}
            </span>
          </div>
        </article>

        <article className="summary-card">
          <span className="summary-label">Role</span>
          <strong>{user ? roleLabel(locale, user.role) : "Guest"}</strong>
          <span className="summary-muted">{user?.username ?? "Validation mode"}</span>
        </article>
      </section>

      <main className="shell-main">{children}</main>
    </div>
  );
}
