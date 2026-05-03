# AGENTS.md

## Scope

This file applies to `src/Faryma.Composer.Contracts`.

## Contracts Layer

- Treat this project as the shared contract surface for API DTOs, application commands and models, infrastructure entities, and shared enums.
- Start navigation from the boundary that matches the change: `Api`, `Application`, or `Infrastructure`.
- Use `.agent/machine-route.yaml` to find all consumers before changing contracts that cross application boundaries.

## Navigation Rules

- Keep API request, response, DTO, and async message shapes under `Api`.
- Keep application commands and application-facing models under `Application`.
- Keep persistence entities and infrastructure enums under `Infrastructure`.
- When changing shared contracts, inspect affected consumers before editing and preserve compatibility for shipped request, response, persisted, and message shapes.

## Verification

- Run verification commands from the `backend` workspace root.
- For API contract changes, start with API-scoped verification:
  `dotnet test tests/Faryma.Composer.Api.Test/Faryma.Composer.Api.Test.csproj`
- For application contract changes, start with application-scoped verification:
  `dotnet test tests/Faryma.Composer.Application.Test/Faryma.Composer.Application.Test.csproj`
- For infrastructure entity or enum changes, also inspect the related persistence configuration, stores, queries, and migration impact before declaring the change done.
