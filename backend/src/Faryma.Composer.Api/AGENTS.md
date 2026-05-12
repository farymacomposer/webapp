# AGENTS.md

## Scope

This file applies to `src/Faryma.Composer.Api`.

## API Bootstrap

- Treat this project as the ASP.NET Core composition root for the backend.
- Start navigation from `Program.cs`, `appsettings*.json`, `Common/DependencyInjection`, `Common/Startup`, exception handling, auth, filters, middleware and extensions, and hosted or background services.
- Use `.agent/machine-route.yaml` to jump to the relevant feature folder before exploring neighboring layers.

## Navigation Rules

- Prefer feature-first navigation through `Features/*`.
- Enter `Application`, `Infrastructure`, or `Contracts` only when the change crosses the API boundary.
- Check shared contracts first when request and response DTO changes may affect other applications.
- For auth, startup, filters, middleware, and hosted services, inspect both feature code and registration points before editing.

## Verification

- Prefer feature-scoped verification first.
- Use stronger verification for startup, auth, middleware, background services, and anything that changes API registration or bootstrapping behavior.
