# AGENTS.md

## Mission

Help agents work safely and predictably in the `Faryma.Composer` backend solution.

## Project Snapshot

- The repository is a `.NET 10` solution rooted at `Faryma.Composer.sln`.
- The main server runtime lives in `src/Faryma.Composer.Api`.
- The backend follows layered separation across `Api`, `Application`, `Infrastructure`, and `Contracts`.
- Supporting projects include `Faryma.Composer.MigrationsBundle` for EF Core migrations and `Faryma.Composer.Desktop` for the WinUI client that shares contracts with the backend.
- Business behavior is additionally described in `use-cases/*`, but code remains the final source of truth.

## ASP.NET Navigation Notes

- Treat `src/Faryma.Composer.Api` as the ASP.NET Core app bootstrap and start navigation from `Program.cs`, API `DependencyInjection`, `appsettings*.json`, auth, filters, middleware/extensions, and hosted/background services.
- Treat `.agent/machine-route.yaml` as the canonical navigation map for feature folders and key runtime entry points.
- Enter neighboring layers only when the change crosses `Api`, `Application`, `Infrastructure`, or `Contracts` boundaries.
- When a task crosses backend and desktop boundaries, inspect the shared `Contracts` slice first, then open only the affected consumer applications.

## Desktop Navigation Notes

- Treat `src/Faryma.Composer.Desktop` as the desktop app bootstrap and start navigation from `App.xaml`, `App.xaml.cs`, desktop `ServiceCollectionExtensions`, `Navigation`, `Auth`, `Services`, and `Api`.
- Treat `.agent/machine-route.yaml` as the canonical navigation map for feature folders and key runtime entry points.
- Enter `src/Faryma.Composer.Desktop` only for WinUI/UI/client-side tasks, desktop auth/session flow, desktop navigation/dialog/page behavior, desktop API client changes, or when shared `Contracts` changes explicitly affect the desktop app. Do not enter `src/Faryma.Composer.Desktop` or `use-cases/*` unless the user request explicitly involves those areas or the backend change cannot be completed safely without them.
- When a task crosses backend and desktop boundaries, inspect the shared `Contracts` slice first, then open only the affected consumer applications.

## Source Of Truth

Before any substantial task, agents should read:

1. `AGENTS.md`
2. `.agent/machine-route.yaml`

If documents disagree with code, treat code as primary and update the stale document only when the task requires it.

## Working Agreement

- Keep changes incremental and shippable.
- Default to the smallest change that fully satisfies the user's request.
- Use `.agent/machine-route.yaml` as navigation context only; it may contain both directory-level routes and a small number of key entry files. Do not expand scope just because related areas exist.
- Keep work inside the affected feature and layer boundaries unless the change truly requires crossing them.
- Follow the existing project shape: feature-oriented folders in `Api` and `Application`, shared DTOs/contracts in `Contracts`, persistence concerns in `Infrastructure`.
- Do not broaden a task into extra refactors, cleanup, documentation, route updates, or architecture changes unless the user asked for it or the change cannot be completed safely without it.
- When multiple valid scopes exist, choose the smaller one first and ask before expanding.
- Do not create, update, or delete tests without explicit user approval.
- Do not create or update documentation or use-case documents without explicit user approval.
- Preserve a runnable project at all times.
- Ask before introducing major dependencies, persistence changes, or architecture pivots.
- If you notice adjacent issues, mention them briefly after finishing instead of folding them into the same change.

## Agent Workflow

1. Read the current route from `.agent/machine-route.yaml`.
2. Confirm the user's requested scope and use the route as a lookup guide for relevant folders and entry points, not as a reason to broaden the task.
3. Identify the smallest affected slice of the solution.
4. Implement only that slice.
5. Run the smallest meaningful verification first, then expand only if the risk justifies it.
6. Update `AGENTS.md` or `.agent/machine-route.yaml` only when the repository guidance has actually changed.

## Verification

- Prefer targeted verification over full-solution verification when a project- or feature-scoped check is sufficient.
- Start with the affected project, then expand to neighboring projects if the change crosses boundaries.
- For API startup, auth, persistence, or migration changes, use proportionally stronger verification because those areas have wider blast radius.

## Definition Of Done

A task is done when the requested change is complete, the relevant verification has been run, and the guidance files reflect the current repository shape when they were part of the requested scope.
