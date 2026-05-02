# AGENTS.md

## Mission

Help agents work safely and predictably in the `Faryma.Composer` backend solution.

## Repository Shape

- The repository is a `.NET 10` solution rooted at `Faryma.Composer.slnx`.
- The main server runtime lives in `src/Faryma.Composer.Api`.
- The backend is organized across `Api`, `Application`, `Infrastructure`, and `Contracts`.
- Supporting projects include `Faryma.Composer.MigrationsBundle`, `Faryma.Composer.Desktop`, and the `Api`, `Application`, and shared testing projects under `tests`.
- Business behavior may be described in `use-cases/*`, but code remains the source of truth.

## Guidance Precedence

- Code is primary. If guidance disagrees with code, follow code and update the stale guidance only when the task requires it.
- This root file defines repository-wide expectations.
- Nested `AGENTS.md` files provide local instructions for their directories and take precedence within that scope.
- Treat `.agent/machine-route.yaml` as the canonical navigation map for feature paths and entry points, not as a substitute for reading code.

## Navigation Summary

- Start from the smallest relevant scope instead of pre-reading the entire repository.
- For API work, begin with the local guidance under `src/Faryma.Composer.Api`.
- For desktop work, begin with the local guidance under `src/Faryma.Composer.Desktop`.
- When a task crosses backend and desktop boundaries, inspect the shared `Contracts` slice first, then open only the affected consumer applications.
- Use `.agent/machine-route.yaml` to locate relevant features, neighboring layers, and key bootstrapping files.

## Working Agreement

- Keep changes incremental, shippable, and limited to the requested outcome.
- Default to the smallest change that fully satisfies the user's request.
- Stay inside the affected feature and layer boundaries unless the change truly requires crossing them.
- Do not broaden a task into extra refactors, cleanup, route updates, or architecture changes unless the user asked for it or the change cannot be completed safely without it.
- When multiple valid scopes exist, choose the smaller one first and ask before expanding.
- Follow the existing project shape: feature-oriented folders in `Api` and `Application`, shared DTOs and contracts in `Contracts`, and persistence concerns in `Infrastructure`.
- Ask before introducing major dependencies, persistence changes, migration changes, or architecture pivots.
- Do not create or update documentation or use-case documents unless the user explicitly asked for that work.
- If you notice adjacent issues, mention them briefly after finishing instead of folding them into the same change.

## Agent Workflow

1. Confirm the user's requested scope.
2. Open the nearest relevant guidance file and use `.agent/machine-route.yaml` to locate the smallest affected slice.
3. Read the code required to complete the task safely.
4. Implement only the affected slice.
5. Run proportionate verification and perform an explicit validation pass before declaring the task done.
6. Update guidance files only when repository guidance has actually changed.

## Verification

- Prefer targeted verification over full-solution verification when a project- or feature-scoped check is sufficient.
- Start with the affected project or feature, then expand only if the change crosses boundaries or the risk justifies it.
- Agents may add or update targeted tests in the affected slice when that is the smallest reliable way to verify the requested change.
- Ask before adding large new test suites, broad test rewrites, or unrelated coverage expansion.
- For API startup, auth, persistence, migration, or shared contract changes, use proportionally stronger verification.
- Preserve a runnable repository state at all times.

## Definition Of Done

A task is done when the requested change is complete, the relevant verification has been run, the result has been explicitly validated, and the guidance files reflect the current repository shape when they were part of the requested scope.
