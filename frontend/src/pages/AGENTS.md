# AGENTS.md

## Scope

This file applies to `src/pages`.

## Page Role

- Treat pages as screen-level composition points.
- Start with the specific page folder, then trace into the widgets it renders.
- Keep page logic focused on local screen state, page layout, and widget composition.

## Navigation Rules

- Prefer editing the page first when the request is tied to a specific route or screen.
- Move into `widgets` for reusable screen sections, into `features` for user actions, and into `entities` for domain presentation.
- Avoid placing generic UI primitives or shared hooks directly in `pages`; those belong in `shared`.
- Avoid pulling app-wide concerns into a page when the behavior belongs in `app` providers or routing.

## Verification

- Prefer page-scoped verification for screen composition changes.
- Use stronger verification when a page change also affects routing, shared widget behavior, or cross-screen layout expectations.
