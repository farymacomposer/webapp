# AGENTS.md

## Scope

This file applies to `src/Faryma.Composer.Infrastructure`.

## Infrastructure Layer

- Treat this project as the persistence and infrastructure implementation layer.
- Start navigation from `AppDbContext.cs`, `DependencyInjection/ServiceCollectionExtensions.cs`, `Persistence`, `Options`, stores, queries, entity configurations, and `UnitOfWork.cs`.
- Use `.agent/machine-route.yaml` to find the related feature, contract, and test paths before widening scope.

## Navigation Rules

- Keep EF Core configuration, stores, queries, unit-of-work behavior, and infrastructure options in this project.
- Enter `Application` only when persistence behavior changes application service behavior or contracts.
- Enter `Contracts/Infrastructure` when entity or enum shapes need to change.
- Inspect `docs/database/schema.dbml` when database shape changes are relevant, but do not update documentation unless the task requires it.

## Verification

- Prefer the smallest persistence- or feature-scoped verification that confirms the behavior.
- Run verification commands from the `backend` workspace root.
- For persistence changes that affect application behavior, start with application-scoped tests:
  `dotnet test tests/Faryma.Composer.Application.Test/Faryma.Composer.Application.Test.csproj`
- Use stronger verification when `AppDbContext`, entity configuration, stores, queries, unit-of-work behavior, or dependency registration changes.
