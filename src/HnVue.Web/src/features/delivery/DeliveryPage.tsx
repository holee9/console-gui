import { useAppState } from "../../app/state";

export default function DeliveryPage() {
  const { text, deliveryQueue, burnDisc, toggleDeliveryFlag, auditTrail } = useAppState();

  return (
    <div className="page-stack">
      <section className="page-intro panel">
        <h2>{text.deliveryTitle}</h2>
        <p>{text.deliveryDescription}</p>
      </section>

      <div className="two-column-layout">
        <section className="panel panel-stack">
          {deliveryQueue.map((study) => (
            <article key={study.id} className="subpanel">
              <div className="section-header">
                <div>
                  <h3>{study.patientName}</h3>
                  <span className="summary-muted">{study.studyLabel}</span>
                </div>
                <span className={`status-chip ${study.status === "Completed" ? "is-success" : "is-warning"}`}>
                  {study.status}
                </span>
              </div>

              <div className="key-value-list">
                <div>
                  <span>Disc label</span>
                  <strong>{study.discLabel}</strong>
                </div>
                <div>
                  <span>Drive</span>
                  <strong>{study.drive}</strong>
                </div>
              </div>

              <div className="toggle-row">
                <label className="toggle-card">
                  <input
                    type="checkbox"
                    checked={study.includeViewer}
                    onChange={() => toggleDeliveryFlag(study.id, "includeViewer")}
                  />
                  <span>{text.includeViewer}</span>
                </label>

                <label className="toggle-card">
                  <input
                    type="checkbox"
                    checked={study.encryptDisc}
                    onChange={() => toggleDeliveryFlag(study.id, "encryptDisc")}
                  />
                  <span>{text.encryptDisc}</span>
                </label>
              </div>

              <div className="progress-track" aria-label="Burn progress">
                <div className="progress-fill" style={{ width: `${study.progress}%` }} />
              </div>

              <button type="button" className="primary-button" onClick={() => burnDisc(study.id)}>
                {text.burnNow}
              </button>
            </article>
          ))}
        </section>

        <aside className="panel panel-stack">
          <section className="subpanel">
            <h3>Validation checks</h3>
            <ul className="plain-list">
              <li>Viewer packaging visibility</li>
              <li>Drive / media label comprehension</li>
              <li>Burn progress readability</li>
              <li>RBAC awareness for patient media export</li>
            </ul>
          </section>

          <section className="subpanel">
            <h3>{text.eventLog}</h3>
            <div className="log-grid compact-log">
              {auditTrail.slice(0, 6).map((event) => (
                <article key={event.id} className="log-item">
                  <strong>{event.timestamp}</strong>
                  <span>{event.message}</span>
                </article>
              ))}
            </div>
          </section>
        </aside>
      </div>
    </div>
  );
}
