import { useDeferredValue, useState } from "react";
import { useAppState } from "../../app/state";
import type { WorkflowStage } from "../../shared/contracts";

const stageOrder: Record<WorkflowStage, number> = {
  Idle: 0,
  PatientSelected: 1,
  ProtocolLoaded: 2,
  ReadyToExpose: 3,
  ImageReview: 4,
  Completed: 5
};

const stageLabels = [
  "Patient",
  "Protocol",
  "Confirm",
  "Expose",
  "Send"
] as const;

export default function ConsolePage() {
  const {
    text,
    worklist,
    protocols,
    selectedStudy,
    selectedProtocol,
    workflowStage,
    systemStatus,
    alerts,
    auditTrail,
    currentDose,
    selectStudy,
    selectProtocol,
    confirmParameters,
    startExposure,
    sendToPacs,
    resetWorkflow,
    acknowledgeAlert
  } = useAppState();
  const [query, setQuery] = useState("");
  const deferredQuery = useDeferredValue(query);

  const filteredStudies = worklist.filter((study) => {
    const target = `${study.patientName} ${study.patientId} ${study.bodyPart} ${study.accessionNumber}`.toLowerCase();
    return target.includes(deferredQuery.trim().toLowerCase());
  });

  const visibleProtocols = protocols.filter((protocol) =>
    selectedStudy ? protocol.bodyPart === selectedStudy.bodyPart : true
  );

  const progress = stageOrder[workflowStage];
  const isBlocked = systemStatus.safeState === "Blocked";
  const canConfirm = workflowStage === "ProtocolLoaded" && !isBlocked;
  const canExpose = workflowStage === "ReadyToExpose" && !isBlocked;
  const canSend = workflowStage === "ImageReview" || workflowStage === "Completed";

  return (
    <div className="page-stack">
      {selectedStudy?.ageGroup === "Pediatric" && (
        <div className="inline-banner warning-banner">
          Pediatric study selected. Keep low-dose presets and the confirmation step visible before exposure.
        </div>
      )}

      <div className="console-grid">
        <div className="panel panel-stack">
          <section className="subpanel">
            <div className="section-header">
              <h2>{text.worklist}</h2>
              <button type="button" className="ghost-button">
                {text.emergencyRegister}
              </button>
            </div>

            <input
              className="text-input"
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder={text.searchPlaceholder}
            />

            <div className="list-stack">
              {filteredStudies.length === 0 ? (
                <p className="empty-state">{text.noMatches}</p>
              ) : (
                filteredStudies.map((study) => (
                  <button
                    key={study.id}
                    type="button"
                    className={`list-card ${selectedStudy?.id === study.id ? "is-active" : ""}`}
                    onClick={() => selectStudy(study.id)}
                  >
                    <div className="card-row">
                      <strong>{study.patientName}</strong>
                      <span className={`badge ${study.urgency === "Emergency" ? "badge-danger" : "badge-neutral"}`}>
                        {study.urgency}
                      </span>
                    </div>
                    <span>{study.patientId}</span>
                    <span>{study.requestedProcedure}</span>
                    <span>{study.room}</span>
                  </button>
                ))
              )}
            </div>
          </section>

          <section className="subpanel">
            <div className="section-header">
              <h2>{text.workflow}</h2>
              <span className="summary-muted">MR-003 / Max 5 clicks</span>
            </div>

            <div className="steps-grid">
              {stageLabels.map((label, index) => (
                <div
                  key={label}
                  className={`step-card ${index < progress ? "is-complete" : ""} ${index === progress ? "is-current" : ""}`}
                >
                  <span className="step-index">{index + 1}</span>
                  <span>{label}</span>
                </div>
              ))}
            </div>

            <div className="key-value-list">
              <div>
                <span>Patient</span>
                <strong>{selectedStudy?.patientName ?? "Pending"}</strong>
              </div>
              <div>
                <span>Protocol</span>
                <strong>{selectedProtocol?.title ?? "Pending"}</strong>
              </div>
              <div>
                <span>Workflow</span>
                <strong>{workflowStage}</strong>
              </div>
            </div>
          </section>
        </div>

        <section className="panel viewer-shell">
          <div className="section-header">
            <h2>{text.viewer}</h2>
            <span className="summary-muted">
              {selectedStudy ? `${selectedStudy.bodyPart} / ${selectedStudy.requestedProcedure}` : "Select a study"}
            </span>
          </div>

          <div className="viewer-canvas">
            <div className="viewer-overlay">
              <span className="viewer-pill">Window / Level</span>
              <span className="viewer-pill">Zoom 100%</span>
              <span className="viewer-pill">Detector ready</span>
            </div>

            <div className="viewer-image">
              <div className="scan-crosshair" />
              <div className="scan-outline" />
            </div>

            <div className="viewer-metadata">
              <div>
                <span>Study</span>
                <strong>{selectedStudy?.accessionNumber ?? "Pending"}</strong>
              </div>
              <div>
                <span>DAP</span>
                <strong>{currentDose.dap.toFixed(2)} mGy·cm²</strong>
              </div>
              <div>
                <span>EI / DI</span>
                <strong>
                  {currentDose.ei} / {currentDose.di}
                </strong>
              </div>
            </div>
          </div>

          <section className="subpanel compact">
            <div className="section-header">
              <h3>{text.alerts}</h3>
              <span className={`status-chip ${isBlocked ? "is-danger" : "is-success"}`}>
                {systemStatus.safeState}
              </span>
            </div>

            <div className="alert-stack">
              {alerts.map((alert) => (
                <article key={alert.id} className={`alert-card ${alert.severity}`}>
                  <div>
                    <strong>{alert.title}</strong>
                    <p>{alert.message}</p>
                  </div>
                  {alert.requiresAck ? (
                    <button type="button" className="secondary-button" onClick={() => acknowledgeAlert(alert.id)}>
                      ACK
                    </button>
                  ) : null}
                </article>
              ))}
            </div>
          </section>
        </section>

        <div className="panel panel-stack">
          <section className="subpanel">
            <div className="section-header">
              <h2>{text.protocol}</h2>
              <span className="summary-muted">{selectedStudy?.bodyPart ?? "APR"}</span>
            </div>

            <div className="list-stack">
              {visibleProtocols.map((protocol) => (
                <button
                  key={protocol.id}
                  type="button"
                  className={`list-card ${selectedProtocol?.id === protocol.id ? "is-active" : ""}`}
                  onClick={() => selectProtocol(protocol.id)}
                >
                  <div className="card-row">
                    <strong>{protocol.title}</strong>
                    {protocol.pediatric ? <span className="badge badge-neutral">Peds</span> : null}
                  </div>
                  <span>
                    {protocol.orientation} / {protocol.detector}
                  </span>
                  <span>
                    {protocol.kvp} kVp · {protocol.mas} mAs · AEC {protocol.aec}
                  </span>
                </button>
              ))}
            </div>
          </section>

          <section className="subpanel">
            <div className="section-header">
              <h2>{text.parameters}</h2>
              <span className="summary-muted">{text.detector}</span>
            </div>

            <div className="key-value-list">
              <div>
                <span>kVp</span>
                <strong>{selectedProtocol?.kvp ?? "--"}</strong>
              </div>
              <div>
                <span>mAs</span>
                <strong>{selectedProtocol?.mas ?? "--"}</strong>
              </div>
              <div>
                <span>AEC</span>
                <strong>{selectedProtocol?.aec ?? "--"}</strong>
              </div>
              <div>
                <span>{text.detector}</span>
                <strong>{systemStatus.detector}</strong>
              </div>
              <div>
                <span>Generator</span>
                <strong>{systemStatus.generator}</strong>
              </div>
              <div>
                <span>{text.dose}</span>
                <strong>{currentDose.effectiveDose.toFixed(2)} mSv</strong>
              </div>
            </div>

            <div className="action-stack">
              <button type="button" className="secondary-button" disabled={!canConfirm} onClick={confirmParameters}>
                {text.confirmParameters}
              </button>
              <button type="button" className="danger-button" disabled={!canExpose} onClick={startExposure}>
                {text.startExposure}
              </button>
              <button type="button" className="primary-button" disabled={!canSend} onClick={sendToPacs}>
                {text.sendToPacs}
              </button>
              <button type="button" className="ghost-button" onClick={resetWorkflow}>
                {text.resetWorkflow}
              </button>
            </div>
          </section>
        </div>

        <section className="panel bottom-panel">
          <div className="section-header">
            <h2>{text.eventLog}</h2>
            <span className="summary-muted">Bottom status bar / audit-style feed</span>
          </div>

          <div className="log-grid">
            {auditTrail.map((event) => (
              <article key={event.id} className="log-item">
                <span className={`badge ${event.level === "critical" ? "badge-danger" : "badge-neutral"}`}>
                  {event.level}
                </span>
                <strong>{event.timestamp}</strong>
                <span>{event.message}</span>
              </article>
            ))}
          </div>
        </section>
      </div>
    </div>
  );
}
