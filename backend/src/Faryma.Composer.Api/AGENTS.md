# AGENTS.md

## Scope

This file applies to `src/Faryma.Composer.Api`.

## API Bootstrap

- Treat this project as the ASP.NET Core composition root for the backend.
- Start navigation from `Program.cs`, `appsettings*.json`, `Common/DependencyInjection`, `Common/Startup`, exception handling, auth, filters, middleware and extensions, API-hosted services, and registration points.
- Use `.agent/machine-route.yaml` to jump to the relevant API feature folder before exploring neighboring layers.

## Navigation Rules

- Prefer API feature-first navigation through `Features/*`, controllers, hubs, API mappers, filters, and endpoint-facing services.
- Enter `Application`, `Infrastructure`, or `Contracts` only when the change crosses the API boundary.
- Check `src/Faryma.Composer.Contracts/Api` first when request and response DTO changes may affect other applications.
- For auth, startup, filters, middleware, and hosted services, inspect both feature code and registration points before editing.

## Verification

- Prefer feature-scoped verification first.
- Run verification commands from the `backend` workspace root.
- For API behavior, auth, startup, middleware, filters, exception handling, or API contract changes, start with API-scoped verification:
  `dotnet test tests/Faryma.Composer.Api.Test/Faryma.Composer.Api.Test.csproj`
- Use stronger verification for startup, auth, middleware, background services, and anything that changes API registration or bootstrapping behavior.
- If the change also affects application behavior behind the API, add the relevant application tests.
