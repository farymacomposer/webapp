# AGENTS.md

## Scope

This file applies to `src/shared`.

## Shared Role

- Treat `shared` as the home for generic UI primitives, hooks, utilities, constants, assets, and types.
- Start with the narrowest subfolder such as `ui`, `lib`, `const`, `types`, or `assets`.
- Keep this layer framework- and domain-light wherever practical.

## Navigation Rules

- Enter `shared` only when the code is truly reusable across multiple features, widgets, or pages.
- Do not move business-specific or screen-specific behavior into `shared`.
- Preserve stable public APIs for shared primitives when possible, because changes here can affect many consumers.
- For UI primitives, keep styling colocated and prefer extending existing components over duplicating similar building blocks.
- For hooks and utilities, favor explicit inputs and outputs over hidden coupling to page or widget state.

## Verification

- Use stronger verification for shared UI primitives, shared hooks, and utility functions because regressions can fan out widely.
- Prefer focused tests or targeted smoke checks when changing low-level shared behavior.
