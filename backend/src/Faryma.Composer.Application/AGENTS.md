# AGENTS.md

## Scope

This file applies to `src/Faryma.Composer.Application`.

## Application Layer

- Treat this project as the backend application logic layer for services, feature workflows, background workers, priority algorithms, and application workflows.
- Start navigation from `Features/*`, `Common`, and `DependencyInjection/ServiceCollectionExtensions.cs`.
- Use `.agent/machine-route.yaml` to find the matching API, Contracts, Infrastructure, Desktop, or test paths before widening scope.

## Navigation Rules

- Prefer feature-first navigation through `Features/*`.
- Keep orchestration and business behavior in application services; enter `Api` only for endpoint, auth, middleware, or presentation concerns.
- Enter `Infrastructure` only for persistence implementations, EF behavior, stores, queries, or unit-of-work behavior.
- Check `src/Faryma.Composer.Contracts/Application` first when commands, models, or application contract changes may affect callers.

## Verification

- Prefer feature-scoped verification first.
- Run verification commands from the `backend` workspace root.
- For changes in application services, feature logic, background workers, priority algorithms, or application contracts, start with application-scoped tests:
  `dotnet test tests/Faryma.Composer.Application.Test/Faryma.Composer.Application.Test.csproj`
- Expand to API or desktop verification only when the changed contract or behavior crosses those boundaries.
