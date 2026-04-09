# UI 변경 관리 프로세스

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | 2.0 |
| 상태 | Active |
| 기준 | IEC 62366-1, PPT 최종안 기반 설계 운영 |
| 갱신일 | 2026-04-09 |

## 목적

UI 변경이 발생했을 때 디자인팀이 어떤 문서를 먼저 수정하고, 어떤 수준까지 Coordinator에게 전달할지를 정의한다.

## 활성 입력물

변경 관리의 입력물은 아래 셋으로 제한한다.

- 승인된 PPT 원본 또는 승인된 캡처 이미지
- 활성 UISPEC 문서
- 승인된 사용자 지시사항

다음 항목은 활성 입력물에서 제외한다.

- HTML 목업
- Pencil/Figma 산출물
- 현재 XAML 화면 배치

## 변경 처리 순서

1. 원천 확인
   - 변경이 PPT 최종안에 있는지, 사용자의 직접 지시인지 먼저 확인한다.
2. 영향 화면 식별
   - 어떤 독립 창이 영향을 받는지 식별한다.
3. 기준 문서 개정
   - `PPT_ANALYSIS -> UI_DESIGN_MASTER_REFERENCE -> UISPEC -> 파생 리뷰` 순서로 문서를 수정한다.
4. Coordinator handoff 정리
   - 구현에 필요한 의도와 미확정 포인트만 Coordinator에게 전달한다.
5. 변경 이력 기록
   - [UI_CHANGELOG.md](D:/workspace-gitea/Console-GUI/.worktrees/team-design/docs/design/UI_CHANGELOG.md)에 반영한다.

## 안전 우선 화면

다음 화면은 추가 검토가 필요하다.

- Acquisition
- Image
- Login
- Worklist

이 화면들에서 변경이 발생하면 디자인 의도뿐 아니라 환자 식별, 조작 실수 방지, 화면 가시성까지 함께 검토한다.

## 산출물 규칙

- 설계팀 산출물은 Markdown 문서만 활성 자산으로 유지한다.
- 구현 지시는 문서에 남기되 코드 수정은 하지 않는다.
- 실험성 아이디어는 활성 문서가 아니라 리뷰 메모나 Coordinator 질의 항목으로 분리한다.
