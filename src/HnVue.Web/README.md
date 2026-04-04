# HnVue.Web

Web validation workspace for the HnVue console UI.

## Purpose

- Validate the desktop GUI information architecture and operator workflow in a browser.
- Keep hardware integration and release packaging on the desktop application track.
- Iterate quickly on usability before the production WPF implementation catches up.

## Commands

```bash
pnpm install
pnpm dev
pnpm exec playwright install chromium
pnpm test:e2e
pnpm build
```

## Scope

- Login and role entry
- 5-click acquisition console shell
- Image review and dose summary
- CD/DVD delivery flow
- System administration and language settings
- Mock contracts aligned with `HnVue.Common`
- Playwright-based E2E validation for the web MVP

## Review Status

- PR `#2` reviewer feedback (`#3` to `#6`) was addressed before merge.
- The merged branch includes the `burnDisc` audit fix, numeric `NaN` guards, `state.tsx` quality cleanup, ESLint, and core state-action tests.
- Validation baseline: `pnpm lint`, `pnpm typecheck`, `pnpm test`, `pnpm build`, `pnpm test:e2e`
