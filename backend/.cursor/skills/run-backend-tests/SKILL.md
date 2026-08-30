---
name: run-backend-tests
description: >-
  Runs Faryma.Composer backend xunit v3 tests via the native in-process runner
  (dotnet run -- or the test .exe). Use when running, debugging, or verifying
  backend tests; when the user asks to run tests, dotnet test, xunit, Api.Test,
  or Application.Test; or when a run fails with Microsoft.Testing.Platform or
  VSTest target errors.
---

# Запуск backend-тестов

Проекты используют **xunit v3 native in-process runner**, не Microsoft Testing Platform runner. В `.csproj` нет `UseMicrosoftTestingPlatformRunner`.

Не используй `dotnet test`. На .NET 10 SDK он падает:

```text
Testing with VSTest target is no longer supported by Microsoft.Testing.Platform
```

Не чини это через `global.json`, `TestingPlatformDotnetTestSupport` или `UseMicrosoftTestingPlatformRunner`, если пользователь об этом не просил.

## Как запускать

Рабочая директория: `backend`.

Предпочтительно `dotnet run --project` — он собирает проект и передаёт аргументы раннеру после `--`:

```bash
dotnet run --project tests/Faryma.Composer.Api.Test/Faryma.Composer.Api.Test.csproj -- -class Faryma.Composer.Api.Test.ComposerStream.CreateStreamTests
```

```bash
dotnet run --project tests/Faryma.Composer.Application.Test/Faryma.Composer.Application.Test.csproj
```

Эквивалент после уже успешного `dotnet build`: запуск `bin/Debug/net10.0/<Project>.exe` с теми же флагами.

Справка native-раннера: `--help` у `.exe` или `dotnet run --project <csproj> -- --help`.

## Фильтры native-раннера

Только эти флаги. Не используй MTP (`--filter-class`, `--filter-query`) и VSTest (`--filter`, `-filterVSTest`).

| Цель | Флаг |
|------|------|
| Класс (полное имя типа) | `-class Faryma.Composer.Api.Test.ComposerStream.CreateStreamTests` |
| Несколько классов | `-class Foo -class Bar` |
| Метод (тип + метод) | `-method Faryma.Composer.Api.Test.ComposerStream.CreateStreamTests.Composer_can_create_stream_on_today_utc` |
| Пространство имён и потомки | `-namespace Faryma.Composer.Api.Test.ComposerStream` |
| Query-фильтр | `-filter /assembly/namespace/class/method` |

Правила:

- Не смешивай simple-фильтры (`-class` / `-method` / `-namespace`) с `-filter`.
- Wildcard `*` допустим в начале и/или конце simple-фильтра.
- Вложенный класс: `Outer+Inner`.

## Проекты

| Проект | Запускать | Docker / PostgreSQL |
|--------|-----------|---------------------|
| `tests/Faryma.Composer.Api.Test` | Да | Нужен для `DatabaseTestBase` |
| `tests/Faryma.Composer.Application.Test` | Да | Нет |
| `tests/Faryma.Composer.Testing` | Нет | Инфраструктура (`IsTestProject=false`) |

Тесты `Api.Test` на `TestBase` базу не требуют, но запускаются тем же проектом.

## Ошибки, которые не надо «чинить»

- `VSTest target is no longer supported` — отказ `dotnet test`, не падение тестов. Запусти `dotnet run --project`.
- `-p:TestingPlatformDotnetTestSupport=true` тот же отказ не снимает.
- `--filter-class` / `--filter-query` на этом раннере неизвестны. Нужны `-class` / `-filter`.
- Логи EF `duplicate key` / `unique constraint` в сценариях отказа — ожидаемый шум, если итог xunit зелёный.
