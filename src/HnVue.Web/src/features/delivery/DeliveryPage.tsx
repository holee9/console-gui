import { useState } from "react";
import { useAppState } from "../../app/state";

export default function DeliveryPage() {
  const { locale, text, deliveryQueue, burnDisc, toggleDeliveryFlag, auditTrail } = useAppState();
  const [pendingBurnId, setPendingBurnId] = useState<string | null>(null);
  const pendingStudy = deliveryQueue.find((study) => study.id === pendingBurnId) ?? null;

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

              <button type="button" className="primary-button" onClick={() => setPendingBurnId(study.id)}>
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
              <li>Patient / study confirmation before PHI export</li>
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

      {pendingStudy ? (
        <div className="modal-backdrop" role="dialog" aria-modal="true" aria-labelledby="burn-confirm-title">
          <div className="modal-card">
            <div className="section-header">
              <h3 id="burn-confirm-title">
                {locale === "ko" ? "환자 배포 전 최종 확인" : "Final confirmation before media export"}
              </h3>
              <span className="summary-muted">{pendingStudy.discLabel}</span>
            </div>

            <div className="key-value-list">
              <div>
                <span>{locale === "ko" ? "환자" : "Patient"}</span>
                <strong>{pendingStudy.patientName}</strong>
              </div>
              <div>
                <span>{locale === "ko" ? "스터디" : "Study"}</span>
                <strong>{pendingStudy.studyLabel}</strong>
              </div>
              <div>
                <span>{locale === "ko" ? "뷰어 포함" : "Include viewer"}</span>
                <strong>{pendingStudy.includeViewer ? "Yes" : "No"}</strong>
              </div>
              <div>
                <span>{locale === "ko" ? "암호화" : "Encryption"}</span>
                <strong>{pendingStudy.encryptDisc ? "Enabled" : "Disabled"}</strong>
              </div>
            </div>

            <p className="supporting-copy">
              {locale === "ko"
                ? "PHI 오배포를 막기 위해 환자와 스터디 라벨을 다시 확인한 뒤 굽기를 시작합니다."
                : "Review the patient and study label one more time before exporting PHI to disc."}
            </p>

            <div className="modal-actions">
              <button type="button" className="ghost-button" onClick={() => setPendingBurnId(null)}>
                {locale === "ko" ? "취소" : "Cancel"}
              </button>
              <button
                type="button"
                className="primary-button"
                onClick={() => {
                  burnDisc(pendingStudy.id);
                  setPendingBurnId(null);
                }}
              >
                {locale === "ko" ? "확인 후 굽기" : "Confirm and burn"}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  );
}
