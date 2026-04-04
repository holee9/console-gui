import { startTransition, useState } from "react";
import { useNavigate } from "react-router-dom";
import { roleLabel } from "../../shared/copy";
import type { UserRole } from "../../shared/contracts";
import { useAppState } from "../../app/state";

const roleDescriptions: Record<UserRole, { ko: string; en: string }> = {
  Radiographer: {
    ko: "환자 선택, 프로토콜 선택, 촬영, 영상 검토, PACS 전송 흐름을 검증합니다.",
    en: "Validate patient selection, protocol choice, exposure, image review, and PACS transfer."
  },
  Radiologist: {
    ko: "영상 검토, 측정 도구, 재촬영 판단, CD 배포 흐름을 확인합니다.",
    en: "Review image interpretation, measurement tools, repeat-study decisions, and delivery flow."
  },
  Admin: {
    ko: "시스템 설정, 다국어, 보안 정책, 업데이트 정책을 검토합니다.",
    en: "Review system settings, localization, security policy, and update policy."
  },
  Service: {
    ko: "유지보수용 설정과 안전 차단 상태를 검증합니다.",
    en: "Validate service settings and safety-blocking states."
  }
};

export default function LoginPage() {
  const navigate = useNavigate();
  const { locale, text, loginAsRole, toggleLocale } = useAppState();
  const [selectedRole, setSelectedRole] = useState<UserRole>("Radiographer");

  function handleEnterWorkspace() {
    loginAsRole(selectedRole);
    startTransition(() => {
      navigate("/console");
    });
  }

  return (
    <div className="login-page">
      <div className="login-panel">
        <div className="brand-block">
          <span className="eyebrow">feature/web-ui</span>
          <h1>{text.loginTitle}</h1>
          <p>{text.loginDescription}</p>
        </div>

        <div className="role-grid">
          {(["Radiographer", "Radiologist", "Admin", "Service"] as const).map((role) => (
            <button
              key={role}
              type="button"
              className={`role-card ${selectedRole === role ? "is-selected" : ""}`}
              onClick={() => setSelectedRole(role)}
            >
              <span className="role-title">{roleLabel(locale, role)}</span>
              <span className="role-description">{roleDescriptions[role][locale]}</span>
            </button>
          ))}
        </div>

        <div className="login-actions">
          <button type="button" className="secondary-button" onClick={toggleLocale}>
            {text.locale}: {locale.toUpperCase()}
          </button>
          <button type="button" className="primary-button" onClick={handleEnterWorkspace}>
            {text.enterWorkspace}
          </button>
        </div>

        <p className="supporting-copy">{text.loginNote}</p>
      </div>
    </div>
  );
}
