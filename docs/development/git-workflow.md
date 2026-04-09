# HnVue Git 저장소 및 브랜치

> 원본: README.md "Git 저장소 및 브랜치" 섹션에서 분리 (2026-04-09)

## 저장소 정보

| 저장소 | 역할 | 주소 |
|--------|------|------|
| **Gitea (origin)** | 사내 주 저장소 | `http://10.11.1.40:7001/DR_RnD/Console-GUI.git` |
| **GitHub (github)** | 외부 미러 | `https://github.com/holee9/console-gui.git` |

- 자동 동기화: Gitea -> GitHub (10분 간격)
- Gitea가 기준 저장소

## 브랜치 전략

| 브랜치 | 용도 | 설명 |
|--------|------|------|
| `main` | 릴리스 기준선 | 프로덕션 배포 준비, 모든 테스트 통과 |
| `team/{team-name}` | 팀별 개발 | 6개 팀 워크트리 (coordinator, qa, ra, team-a, team-b, team-design) |
| `feat/wave*-*` | Wave별 개발 | 병렬 구현 (Wave 1, 2, 3, 4) |
| `feature/web-ui` | 웹 UI 검증 | 향후 웹 인터페이스 추가 |

## Git Clone

```bash
# HTTPS (외부)
git clone https://github.com/holee9/console-gui.git

# SSH (사내)
git clone git@gitea.abyzr.local:DR_RnD/Console-GUI.git

# HTTP (사내 로컬)
git clone http://10.11.1.40:7001/DR_RnD/Console-GUI.git
```

---

문서 최종 업데이트: 2026-04-09
