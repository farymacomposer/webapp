# AGENTS.md

## Scope

This file applies to `src/widgets`.

## Widget Role

- Treat widgets as reusable page sections that compose `features`, `entities`, and `shared`.
- Start with the owning widget folder and inspect local `ui`, `model`, and public `index.ts` exports before widening scope.
- Keep widget logic local to the section it renders.

## Navigation Rules

- Stay inside the affected widget unless the change clearly belongs to a lower layer.
- Move into `features` for user-triggered actions, into `entities` for domain-specific UI, and into `shared` for generic primitives or utilities.
- Preserve public widget exports where possible and avoid coupling one widget tightly to another widget's internals.
- Keep mock data and temporary UI state close to the widget that owns them unless the task requires a broader state model.

## Verification

- Prefer widget-scoped verification first.
- Use stronger verification when a widget is reused across pages, touches routing context, or depends on shared modal, hook, or layout primitives.
