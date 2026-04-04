import { useAppState } from "../../app/state";

export default function AdminPage() {
  const { text, settings, updateSetting } = useAppState();

  return (
    <div className="page-stack">
      <section className="page-intro panel">
        <h2>{text.adminTitle}</h2>
        <p>{text.adminDescription}</p>
      </section>

      <div className="admin-grid">
        <section className="panel panel-stack">
          <article className="subpanel">
            <div className="section-header">
              <h3>DICOM connectivity</h3>
              <span className="summary-muted">MWL / PACS / Print prep</span>
            </div>
            <div className="form-grid">
              <label>
                <span>PACS AE</span>
                <input
                  className="text-input"
                  value={settings.dicom.pacsAeTitle}
                  onChange={(event) => updateSetting("dicom", "pacsAeTitle", event.target.value)}
                />
              </label>
              <label>
                <span>RIS AE</span>
                <input
                  className="text-input"
                  value={settings.dicom.risAeTitle}
                  onChange={(event) => updateSetting("dicom", "risAeTitle", event.target.value)}
                />
              </label>
              <label>
                <span>Port</span>
                <input
                  className="text-input"
                  type="number"
                  value={settings.dicom.port}
                  onChange={(event) => updateSetting("dicom", "port", Number(event.target.value))}
                />
              </label>
              <label>
                <span>MWL poll (sec)</span>
                <input
                  className="text-input"
                  type="number"
                  value={settings.dicom.pollSeconds}
                  onChange={(event) => updateSetting("dicom", "pollSeconds", Number(event.target.value))}
                />
              </label>
            </div>
          </article>

          <article className="subpanel">
            <div className="section-header">
              <h3>Network</h3>
              <span className="summary-muted">Validation only</span>
            </div>
            <div className="form-grid">
              <label>
                <span>IP</span>
                <input
                  className="text-input"
                  value={settings.network.ip}
                  onChange={(event) => updateSetting("network", "ip", event.target.value)}
                />
              </label>
              <label>
                <span>Gateway</span>
                <input
                  className="text-input"
                  value={settings.network.gateway}
                  onChange={(event) => updateSetting("network", "gateway", event.target.value)}
                />
              </label>
              <label>
                <span>DNS</span>
                <input
                  className="text-input"
                  value={settings.network.dns}
                  onChange={(event) => updateSetting("network", "dns", event.target.value)}
                />
              </label>
            </div>
          </article>
        </section>

        <section className="panel panel-stack">
          <article className="subpanel">
            <div className="section-header">
              <h3>{text.locale}</h3>
              <span className="summary-muted">MR-045</span>
            </div>

            <div className="toggle-row">
              <label className="toggle-card">
                <input
                  type="checkbox"
                  checked={settings.ui.touchMode}
                  onChange={(event) => updateSetting("ui", "touchMode", event.target.checked)}
                />
                <span>{text.touchMode}</span>
              </label>

              <label className="toggle-card">
                <input
                  type="checkbox"
                  checked={settings.update.signedOnly}
                  onChange={(event) => updateSetting("update", "signedOnly", event.target.checked)}
                />
                <span>{text.signedUpdates}</span>
              </label>

              <label className="toggle-card">
                <input
                  type="checkbox"
                  checked={settings.update.autoCheck}
                  onChange={(event) => updateSetting("update", "autoCheck", event.target.checked)}
                />
                <span>{text.autoCheck}</span>
              </label>
            </div>

            <label>
              <span>{text.inactivityLock} (minutes)</span>
              <input
                className="text-input"
                type="number"
                value={settings.ui.inactivityMinutes}
                onChange={(event) => updateSetting("ui", "inactivityMinutes", Number(event.target.value))}
              />
            </label>
          </article>

          <article className="subpanel">
            <div className="section-header">
              <h3>{text.reviewedDocs}</h3>
            </div>
            <p>{text.reviewedDocsDescription}</p>
            <ul className="plain-list">
              <li>PRD 5.3 GUI layout concept: top / left / center / right / bottom composition</li>
              <li>SDS 3.8 design tokens: color, spacing, focus, minimum touch target</li>
              <li>UEF 4-6: 5-click workflow, CD burning, KR/EN switching, pediatric safety prompts</li>
            </ul>
          </article>
        </section>
      </div>
    </div>
  );
}
