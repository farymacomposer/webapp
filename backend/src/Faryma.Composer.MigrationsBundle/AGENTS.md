# AGENTS.md

## Scope

This guidance applies to the `Faryma.Composer.MigrationsBundle` project and all files under it, including `Migrations/*`.

## Migration Ownership

- Do not create, edit, rename, delete, or regenerate Entity Framework migration files.
- Do not edit `AppDbContextModelSnapshot.cs`.
- Do not include migration creation or migration editing in implementation plans.
- Database migrations are always created manually by the maintainer.

## Agent Workflow

- If a code change appears to require a schema migration, stop before changing migration files and tell the user that a manual migration is required.
- You may inspect existing migrations to understand the current database shape, but treat them as read-only.
