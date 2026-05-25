# AGENTS.md

## Scope

This file applies to `src/app`.

## App Bootstrap

- Treat `src/app` as the frontend composition root after `src/index.tsx`.
- Start navigation from `App.tsx`, then inspect `providers/*`, route configuration, and shared app styles.
- Enter this layer for startup, routing, provider wiring, global layout, error boundaries, or application-level composition.

## Navigation Rules

- Prefer provider-first navigation for cross-cutting behavior.
- For route changes, inspect both `providers/Router` and `@/shared/const/router.ts` before editing.
- Enter `pages` when the task is screen-specific, and enter `shared` when the task is about reusable primitives rather than app wiring.
- Keep this layer thin: compose behavior here, do not move widget, feature, or entity internals upward into `app`.

## Verification

- Use stronger verification for routing, provider registration, error boundary behavior, and app bootstrap changes.
- If a change can affect multiple screens, prefer at least one broader verification step in addition to any local check.
