import { useDeferredValue, useState } from "react";
import { useAppState } from "../../app/state";
import type { ValidationGuidance, WorkflowStage } from "../../shared/contracts";

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

function getValidationGuidance({
  locale,
  selectedStudy,
  selectedProtocol,
  workflowStage,
  workflowClicks,
  isBlocked,
  canExpose
}: {
  locale: "ko" | "en";
  selectedStudy: ReturnType<typeof useAppState>["selectedStudy"];
  selectedProtocol: ReturnType<typeof useAppState>["selectedProtocol"];
  workflowStage: WorkflowStage;
  workflowClicks: number;
  isBlocked: boolean;
  canExpose: boolean;
}): ValidationGuidance {
  const labels =
    locale === "ko"
      ? {
          title: "검증 가이드",
          noStudy: "Worklist에서 환자를 선택해 5클릭 워크플로우를 시작하세요.",
          emergency: "응급이면 상단의 '응급 시작'으로 응급 환자와 권장 프로토콜을 즉시 불러올 수 있습니다.",
          noProtocol: "선택한 환자와 신체 부위에 맞는 프로토콜을 선택하세요.",
          pediatric: "소아 환자는 저선량 프로토콜을 우선 선택하고 확인 단계 전까지 경고 배너를 유지하세요.",
          blocked: "치명 경고가 남아 있어 촬영이 차단되었습니다.",
          blockedRecovery: "ACK가 필요한 경고를 해제하고, 소아 환자라면 소아용 저선량 프리셋으로 다시 선택하세요.",
          confirm: "kVp, mAs, 디텍터 상태를 확인한 뒤 파라미터 확인을 누르세요.",
          ready: "Generator ACK와 Detector Ready가 모두 녹색이면 촬영 실행으로 진행하세요.",
          send: "영상 검토 후 PACS 전송으로 5클릭 시나리오를 완료하세요.",
          done:
            workflowClicks <= 5
              ? `표준 워크플로우를 ${workflowClicks}클릭으로 완료했습니다.`
              : `워크플로우는 완료했지만 ${workflowClicks}클릭으로 목표(5클릭)를 초과했습니다.`
        }
      : {
          title: "Validation guidance",
          noStudy: "Select a patient from the worklist to start the 5-click workflow.",
          emergency: "If the case is urgent, use Emergency start to preload the emergency patient and the recommended protocol.",
          noProtocol: "Choose the protocol that matches the selected patient and body part.",
          pediatric: "For pediatric studies, keep the low-dose preset and safety banner visible before confirmation.",
          blocked: "A critical alert is still blocking exposure.",
          blockedRecovery: "Acknowledge the blocking alert and switch back to a pediatric-safe preset before continuing.",
          confirm: "Review kVp, mAs, and detector readiness, then confirm the parameters.",
          ready: "Once generator ACK and detector readiness are both green, you can start exposure.",
          send: "Review the image and send it to PACS to complete the 5-click scenario.",
          done:
            workflowClicks <= 5
              ? `The standard workflow finished within ${workflowClicks} clicks.`
              : `The workflow finished in ${workflowClicks} clicks, which exceeds the 5-click target.`
        };

  if (!selectedStudy) {
    return {
      title: labels.title,
      detail: labels.noStudy,
      recovery: labels.emergency
    };
  }

  if (!selectedProtocol) {
    return {
      title: labels.title,
      detail: labels.noProtocol,
      recovery: selectedStudy.ageGroup === "Pediatric" ? labels.pediatric : labels.emergency
    };
  }

  if (isBlocked) {
    return {
      title: labels.title,
      detail: labels.blocked,
      recovery: labels.blockedRecovery
    };
  }

  if (workflowStage === "ProtocolLoaded") {
    return {
      title: labels.title,
      detail: labels.confirm,
      recovery: labels.pediatric
    };
  }

  if (workflowStage === "ReadyToExpose") {
    return {
      title: labels.title,
      detail: canExpose ? labels.ready : labels.blocked,
      recovery: canExpose ? labels.send : labels.blockedRecovery
    };
  }

  if (workflowStage === "ImageReview") {
    return {
      title: labels.title,
      detail: labels.send,
      recovery: labels.done
    };
  }

  return {
    title: labels.title,
    detail: labels.done,
    recovery: labels.emergency
  };
}

export default function ConsolePage() {
  const {
    locale,
    text,
    worklist,
    protocols,
    selectedStudy,
    selectedProtocol,
    workflowStage,
    workflowClicks,
    systemStatus,
    alerts,
    auditTrail,
    currentDose,
    selectStudy,
    selectProtocol,
    launchEmergencyWorkflow,
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
  const canConfirm =
    workflowStage === "ProtocolLoaded" &&
    Boolean(selectedStudy) &&
    Boolean(selectedProtocol) &&
    !isBlocked;
  const canExpose =
    workflowStage === "ReadyToExpose" &&
    !isBlocked &&
    systemStatus.parameterSync === "Acked" &&
    systemStatus.generator === "Ready" &&
    systemStatus.detector === "Ready";
  const canSend = workflowStage === "ImageReview";
  const guidance = getValidationGuidance({
    locale,
    selectedStudy,
    selectedProtocol,
    workflowStage,
    workflowClicks,
    isBlocked,
    canExpose
  });

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
              <button type="button" className="ghost-button" onClick={launchEmergencyWorkflow}>
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

            <div className="metrics-grid">
              <article className={`metric-card ${workflowClicks <= 5 ? "is-success" : "is-warning"}`}>
                <span className="summary-label">{locale === "ko" ? "클릭 수" : "Clicks"}</span>
                <strong>
                  {workflowClicks} / 5
                </strong>
                <span className="summary-muted">
                  {workflowClicks <= 5
                    ? locale === "ko"
                      ? "목표 범위 내"
                      : "On target"
                    : locale === "ko"
                      ? "목표 초과"
                      : "Over target"}
                </span>
              </article>
              <article className="metric-card">
                <span className="summary-label">{guidance.title}</span>
                <strong>{guidance.detail}</strong>
                <span className="summary-muted">{guidance.recovery}</span>
              </article>
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
              <span className="viewer-pill">{systemStatus.detector}</span>
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
              {alerts.length === 0 ? (
                <p className="empty-state">
                  {locale === "ko"
                    ? "현재 차단 경고가 없습니다. 다음 단계 안내를 따라 진행하세요."
                    : "No blocking alerts are active. Follow the next-step guidance panel."}
                </p>
              ) : null}
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
                <span>{text.parameterSync}</span>
                <strong>
                  {systemStatus.parameterSync === "Acked" ? text.ackReceived : text.pendingAck}
                </strong>
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
