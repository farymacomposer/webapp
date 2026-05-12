# AGENTS.md

## Mission

Help agents work safely and predictably in the `Faryma.Composer` frontend.

## Repository Shape

- This repository is a `Vite + React + TypeScript` frontend rooted at the current folder.
- The main application shape follows `app`, `pages`, `widgets`, `features`, `entities`, and `shared`.
- App bootstrap starts in `src/index.tsx`, then flows through `src/app/App.tsx`, app providers, and route configuration.
- Most current UI behavior is local and mock-driven. Treat widget- and entity-level code as the source of truth before assuming a backend integration exists.

## Guidance Precedence

- Code is primary. If guidance conflicts with code, follow the code and update stale guidance only when the task requires it.
- This root file defines repository-wide expectations.
- Nested `AGENTS.md` files in `src/app`, `src/pages`, `src/widgets`, `src/features`, `src/entities/Order`, and `src/shared` take precedence inside their directories.
- Treat `.agent/machine-route.yaml` as the canonical navigation map for layers, feature paths, and entry points. Use it to narrow scope before reading code broadly.

## Navigation Summary

- Start from the smallest relevant slice instead of reading the whole frontend.
- Open the nearest local `AGENTS.md` before widening scope beyond the affected layer.
- For startup, providers, routing, or global layout work, begin with `src/index.tsx`, `src/app/App.tsx`, `src/app/providers`, and `src/shared/const/router.ts`.
- For page behavior, open the relevant page first, then inspect the widgets it composes.
- For widget behavior, trace dependencies downward through `features`, `entities`, and `shared`.
- Prefer public module entry points such as local `index.ts` files before reaching into deep internals.
- Use `.agent/machine-route.yaml` to locate related slices when a change crosses page, widget, feature, entity, and shared boundaries.

## Working Agreement

- Keep changes incremental, shippable, and limited to the requested outcome.
- Default to the smallest change that fully satisfies the user's request.
- Preserve the current repository shape: `pages` compose `widgets`, `widgets` compose `features`, `entities`, and `shared`, and `shared` stays generic.
- Avoid introducing upward imports that cut across the existing layer direction unless the user explicitly asks for a structural change.
- Do not broaden a task into unrelated cleanup, renaming, formatting sweeps, or architecture refactors unless the change cannot be completed safely without them.
- Ask before introducing major dependencies, changing project tooling, altering global providers, or replacing mock-driven behavior with new data flows.
- Keep styles colocated with the owning component and follow the existing module-based SCSS pattern.
- If you notice adjacent issues, mention them briefly after finishing instead of folding them into the same change.

## Agent Workflow

1. Confirm the requested scope.
2. Open `.agent/machine-route.yaml` and locate the smallest affected slice.
3. Read the local page, widget, feature, entity, or shared code required to complete the task safely.
4. Implement only the affected slice and preserve existing public exports where possible.
5. Run proportionate verification and explicitly validate the result before declaring the task done.
6. Update `AGENTS.md` or `.agent/machine-route.yaml` only when repository guidance or navigation has materially changed.

## Verification

- Prefer targeted verification over broad verification when a smaller check is enough.
- Use the existing root scripts in `package.json`: `npm run build`, `npm run lint`, and `npm run test`.
- If dependencies are not installed or the environment is not ready, report that clearly instead of guessing at the result.
- Add or update focused tests only when they materially reduce regression risk for the requested change.
- Use stronger verification for routing, provider setup, shared hooks, shared UI primitives, and other changes that can affect multiple screens.
- Preserve a runnable repository state at all times.

## Definition Of Done

A task is done when the requested frontend change is complete, the relevant verification has been run or its blocker has been made explicit, the result has been validated, and guidance files reflect the current repository shape when they were part of the requested scope.
