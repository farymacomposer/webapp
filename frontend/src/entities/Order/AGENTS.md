# AGENTS.md

## Scope

This file applies to `src/entities/Order`.

## Entity Role

- Treat `Order` as the current domain presentation slice for queue-related UI.
- Start with the public `index.ts`, then inspect `model/types`, `model/consts`, and the specific `ui/*` component involved.
- Keep entity code focused on domain-shaped data, presentation helpers, and reusable domain UI.

## Navigation Rules

- Enter this slice when a change is specific to order cards, order categories, order cover visuals, or order-related typing.
- Move upward to `widgets` only when the change is about page-section composition rather than order presentation itself.
- Move downward to `shared` only for truly generic primitives that should not remain order-specific.
- Preserve the entity public surface in `index.ts` unless the task explicitly requires an API change.

## Verification

- Prefer targeted verification around the affected order component or type shape.
- Use stronger verification when changes affect multiple order presentation variants or shared order data used by more than one widget.
