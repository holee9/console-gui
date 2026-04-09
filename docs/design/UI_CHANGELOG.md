# UI Design Changelog

## Version History

| Version | Date | Description |
|---------|------|-------------|
| 2.0.0 | 2026-04-09 | PPT `변경 2안` 기준 refresh, legacy docs archive, HTML mockup retirement |

## [2.0.0] - 2026-04-09

### Changed

- Active UI baseline을 `docs/★HnVUE UI 변경 최종안_251118.pptx` 중심으로 재정의했다.
- `MainWindow 3열 셸` 해석을 폐기하고 독립 창 구조로 문서를 재작성했다.
- Worklist, Studylist, Acquisition, Merge, Setting, Add Patient/Procedure, Login, Image의 활성 사양을 다시 세웠다.

### Archived

- 기존 design docs, UISPEC docs, workflow docs, change-management docs
- 기존 HTML/Pencil/mockup 자산
- archive snapshot: `docs/archive/2026-04-09_ui-design_pre-ppt251118`

### Deprecated

- HTML mockup 기반 설계 검토
- Pencil/Figma deliverable-first workflow
- 현재 구현 구조를 설계 truth로 사용하는 문서 작성 방식

### Notes

- `Image` 창은 user clarification과 viewer grammar에 따라 독립 창으로 유지한다.
- `SystemAdmin`은 본 PPT refresh 범위 밖으로 두고 active baseline에서 제외한다.
