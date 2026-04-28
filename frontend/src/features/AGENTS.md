# AGENTS.md

## Scope

This file applies to `src/features`.

## Feature Role

- Treat features as focused user actions or small interaction slices.
- Start with the specific feature folder and inspect its public `index.ts`, `ui`, and any nearby model code.
- Keep features narrow and action-oriented.

## Navigation Rules

- Prefer implementing user intent here when the change is about a button, toggle, modal opener, or other isolated interaction.
- Move into `entities` when the feature needs domain-specific presentation, and into `shared` when it needs generic UI primitives or hooks.
- Do not let features absorb page composition or app bootstrap responsibilities.
- Avoid creating cross-feature coupling unless the task explicitly requires shared orchestration.

## Verification

- Prefer focused verification around the exact interaction being changed.
- Use stronger verification when a feature changes shared state flow, modal behavior, or routing-related actions.
