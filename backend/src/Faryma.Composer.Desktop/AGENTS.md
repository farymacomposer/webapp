# AGENTS.md

## Scope

This file applies to `src/Faryma.Composer.Desktop`.

## Desktop Entry Conditions

- Enter this project only for WinUI or client-side work, desktop auth or session flow, navigation or dialog behavior, desktop API client changes, or when shared `Contracts` changes explicitly affect the desktop app.
- Do not enter `use-cases/*` from desktop work unless the user asked for it or the backend or desktop change cannot be completed safely without it.

## Desktop Bootstrap

- Treat this project as the desktop app bootstrap.
- Start navigation from `App.xaml`, `App.xaml.cs`, `ServiceCollectionExtensions.cs`, `Navigation`, `Auth`, `Services`, `Api`, `UI`, `ViewModels`, and `Validation`.
- Use `.agent/machine-route.yaml` to find the relevant shared feature paths before widening scope.

## Navigation Rules

- Keep work inside the affected desktop feature unless a shared contract or API client boundary forces a broader change.
- Inspect `Contracts` first when the task crosses desktop and backend boundaries.
- Open backend projects only when the desktop change depends on API behavior, shared DTOs, or server-side contracts.

## Verification

- Prefer the smallest desktop-scoped verification that confirms the requested behavior.
- Use stronger verification when auth, app startup, service registration, or API client behavior changes.
- Build this WinUI project with Visual Studio MSBuild, not `dotnet build`, so XAML compilation runs in the supported environment.
