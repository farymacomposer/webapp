# Карта тестовой инфраструктуры backend/tests

Карта существующих классов и методов в `Faryma.Composer.Api.Test` и `Faryma.Composer.Testing`: что выбирать при написании теста, как подготовить окружение и данные, как проверить результат. Ссылки ведут к реализации; сигнатуры и поведение описаны по исходникам.

## 1. Быстрый выбор

| Задача | Точка входа |
| --- | --- |
| Проверить HTTP, права доступа, middleware, обработку исключений без обращения к БД | `TestBase` → `CreateApp()` |
| Проверить endpoint с чтением/записью данных, настоящую выдачу токенов или идемпотентность | `DatabaseTestBase` → `await CreateAppAsync()` |
| Отправить запрос анонимно | `app.CreateAnonymousClient()` |
| Отправить запрос с JWT администратора | `await app.CreateAdminBearerClientAsync(...)` |
| Подставить браузерного пользователя и его роли | `await app.CreateBrowserUserClientAsync(...)` |
| Проверить сам login, cookie или refresh token | `SeedUsersAsync(...)` + анонимный клиент + реальный auth-endpoint |
| Подготовить пользователя, стрим, заказ | `new TestDataBuilder(app.Services)` |
| Зафиксировать время приложения | `app.WithFixedDateTime(now)` |
| Перечитать сохранённое состояние | `app.Services.GetStreamAsync(id)`, `GetOrderAsync(id)`, `CountStreamsAsync()` |
| Выполнить свою подготовку или запрос через scoped-сервисы | `app.Services.RunInScopeAsync(...)` |
| Обработать накопленные события очереди и проверить уведомления | `DrainOrderQueueEventsAsync()` → `GetOrderQueueNotifications()` |
| Проверить код бизнес-ошибки HTTP | `response.AssertApiErrorAsync(code, TestContext.CancellationToken)`; HTTP-статус проверяется отдельно |
| Сравнить даты с точностью до миллисекунд | `DateTimeTestAssertions.AssertSameInstant(expected, actual)` |
| Отправить/прочитать JSON со строковыми enum | `TestJsonSerializerOptions.Web` |

### Где находятся классы

| Проект | Назначение и namespace |
| --- | --- |
| [`tests/Faryma.Composer.Api.Test`](../tests/Faryma.Composer.Api.Test/) | HTTP-тесты и API-специфичная инфраструктура. Основные помощники: `Faryma.Composer.Api.Test.Infrastructure`; auth-помощники: `Faryma.Composer.Api.Test.Infrastructure.Auth`. |
| [`tests/Faryma.Composer.Testing`](../tests/Faryma.Composer.Testing/) | Общая инфраструктура PostgreSQL, подготовки данных, DI и уведомлений. Все классы находятся в `Faryma.Composer.Testing.Infrastructure`. В проекте установлено `IsTestProject=false`. |

`Faryma.Composer.Api.Test` и `Faryma.Composer.Application.Test` используют .NET 10, MSTest 4 и платформу VSTest. `Faryma.Composer.Testing` — обычная библиотека тестовой инфраструктуры без тестового фреймворка (`IsTestProject=false`). API-тесты ссылаются на API приложения и общий проект `Testing`. В API-проекте уже глобально подключены `Microsoft.VisualStudio.TestTools.UnitTesting`, `Microsoft.EntityFrameworkCore` и `Faryma.Composer.Testing.Infrastructure`; пространства имён API-помощников нужно подключать самостоятельно.

## 2. API host, базовые классы и изоляция

Источники: [TestBases.cs][api-fixture], [AssemblyParallelization.cs][parallelization], [CustomWebApplicationFactory.cs][api-factory], [TestConfiguration.cs][api-config].

### Базовые классы

| Класс / метод | Как использовать |
| --- | --- |
| `TestBase.CreateApp()` | `protected`, возвращает `CustomWebApplicationFactory` в режиме без БД. Наследуй тестовый класс от `TestBase`. |
| `TestBase.TestContext` | MSTest устанавливает `TestContext` для каждого теста. Для отмены async-операций используй `TestContext.CancellationToken`. |
| `DatabaseTestBase.CreateAppAsync()` | `protected`, возвращает `Task<CustomWebApplicationFactory>` с новой БД. Конструкторная инъекция fixture не нужна: метод получает общий контейнер через `PostgreSqlTestFixture.GetAsync()`. |
| `PostgreSqlTestFixture` | Лениво запускает один `PostgreSqlFixture` на процесс testhost и освобождает его через `[AssemblyCleanup]`. Если DB-тесты не запускались, Docker не используется. |
| `AssemblyParallelization.cs` | Сохраняет параллелизм классов через `[assembly: Parallelize(... ClassLevel)]`. DB-классы помечены `[DoNotParallelize]`, поэтому выполняются последовательно. |

### `CustomWebApplicationFactory`

| Член | Поведение |
| --- | --- |
| `Create()` | Статическая точка входа без базового класса. Создаёт фабрику без БД, `UsesDatabase == false`. |
| `CreateAsync(PostgreSqlFixture fixture)` | Создаёт БД `api_test_<GUID>`, подготавливает схему, возвращает фабрику с `UsesDatabase == true`. |
| `CreateAnonymousClient()` | `HttpClient` с `BaseAddress = https://localhost`, `AllowAutoRedirect = false`. Позволяет непосредственно проверять `302`, `401`, `403` и заголовки ответа. |
| `WithFixedDateTime(DateTime now)` | Возвращает **новую производную фабрику** с фиксированным `DateTimeContext`. Используй возвращённое значение для дальнейшего seeding и создания клиентов. |
| `CreateDerivedFactory(Action<IWebHostBuilder> configureWebHost)` | `internal`: доступен внутри сборки API-тестов. Создаёт производную фабрику для `ConfigureTestServices` / `ConfigureAppConfiguration`; сохраняет настройки родителя и добавляет новые после них. |
| `Services` | Унаследованный доступ к DI-контейнеру конкретного host. Используй для `TestDataBuilder` и extension-методов общего проекта. |
| `CreateClient(WebApplicationFactoryClientOptions options)` | Унаследованный метод, когда нужны свои параметры клиента. Пример с HTTP вместо HTTPS — `ApiSmokeTests`. |
| `DisposeAsync()` / `Dispose()` | Освобождают также производные фабрики, созданные через `CreateDerivedFactory`, включая фабрики браузерных клиентов. Корневую фабрику оформляй через `await using`, клиентов — через `using`. |

Особенности окружения:

- Host работает в `Development`; к MVC добавляются тестовые контроллеры из сборки `Api.Test`.
- Все регистрации `IHostedService` удалены. В частности, worker очереди и фоновая очистка записей идемпотентности не запускаются.
- `IOrderQueueNotificationService` заменён на `TestOrderQueueNotificationService` автоматически.
- В режиме с БД выполняется обычный `ApplicationStartupInitializer`: bootstrap администраторов, загрузка настроек и инициализация очереди. Поэтому в БД уже могут быть пользователи, созданные startup, до вызова `SeedUsersAsync()`.
- В режиме без БД startup-initializer заменён на пустой, Data Protection использует ephemeral provider. Регистрации persistence остаются: для вызова репозиториев, seeding или endpoint, работающего с БД, выбирай `CreateAppAsync()`.

**Граница изоляции:** отдельный `CreateAppAsync()` — отдельная БД. Производные фабрики используют ту же БД и конфигурацию, но создают отдельные host и DI-контейнеры. Их singleton-сервисы, включая очередь и накопитель уведомлений, имеют отдельное состояние. Родитель освобождает дочерние host; созданные БД живут до `[AssemblyCleanup]`, который удаляет общий Testcontainers-контейнер.

### Минимальный тест без БД

```csharp
using System.Net;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;

[TestClass]
public sealed class MyAuthorizationTests : TestBase
{
    [TestMethod]
    public async Task Browser_user_without_admin_role_is_forbidden()
    {
        await using CustomWebApplicationFactory app = CreateApp();
        using HttpClient client = await app.CreateBrowserUserClientAsync(
            ct: TestContext.CancellationToken);

        using HttpResponseMessage response = await client.GetAsync(
            "/api/_test/auth/admin", TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
```

### Минимальный тест с БД

```csharp
using System.Net;
using Faryma.Composer.Api.Test.Infrastructure;
using Faryma.Composer.Api.Test.Infrastructure.Auth;

[TestClass]
[DoNotParallelize]
public sealed class MyDatabaseApiTests : DatabaseTestBase
{
    [TestMethod]
    public async Task Admin_can_read_settings()
    {
        await using CustomWebApplicationFactory app = await CreateAppAsync();
        using HttpClient client = await app.CreateAdminBearerClientAsync(
            ct: TestContext.CancellationToken);

        using HttpResponseMessage response = await client.GetAsync(
            "/api/AppSettings/Get", TestContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
```

## 3. Пользователи и аутентификация

Источники: [CustomWebApplicationFactoryAuthExtensions.cs][auth-extensions], [AuthTestModels.cs][auth-models]. Методы расширяют `CustomWebApplicationFactory`; нужен `using Faryma.Composer.Api.Test.Infrastructure.Auth`.

| Метод | Когда нужен и что делает |
| --- | --- |
| `SeedUsersAsync(AuthTestSeedOptions? options = null, CancellationToken ct = default)` → `Task<SeededAuthUsers>` | Требует БД. Создаёт или синхронизирует **обоих** пользователей из `options.Admin` и `options.Browser`. Возвращает их ID, имена, пароли, Twitch-поля и роли. |
| `CreateAdminBearerClientAsync(TestAuthUserSeed? seed = null, CancellationToken ct = default)` → `Task<HttpClient>` | С БД: синхронизирует пользователя, требует непустой тестовый пароль, вызывает настоящий `AuthTokenService.IssueForUser` и устанавливает `Authorization: Bearer ...`. При этом сохраняется и refresh token, хотя helper возвращает только клиент. Без БД: сам подписывает тестовый JWT со случайным UserId и ролями seed. |
| `CreateBrowserUserClientAsync(TestAuthUserSeed? seed = null, CancellationToken ct = default)` → `Task<HttpClient>` | Создаёт производный host с тестовой схемой `TestBrowserUser`. С БД синхронизирует пользователя; без БД создаёт только состояние claims со случайным UserId. Подходит для запросов от уже аутентифицированного пользователя. |

В режиме с БД bearer-helper выдаёт токен напрямую через сервис; browser-helper подставляет principal через тестовый authentication handler. Для проверки настоящего HTTP login, cookie или Twitch challenge используй анонимный клиент и соответствующий endpoint; готовые примеры перечислены в разделе 9.

### Модели seed

| Тип | Поля и значения по умолчанию |
| --- | --- |
| `TestAuthUserSeed` | Обязательное `UserName`; необязательные `Password`, `TwitchUserId`, `TwitchLogin`; `Roles = []`. При передаче своего seed нужные роли укажи явно. |
| `AuthTestSeedOptions.Admin` | `UserName = "test_composer_admin"`, задан тестовый пароль, `Roles = [AppRoles.Composer]`. |
| `AuthTestSeedOptions.Browser` | `UserName = "test_browser_user"`, заполнены Twitch ID/login; пароль отсутствует, роли пусты. |
| `SeededAuthUser` | Результат: `UserId`, `UserName`, `Password`, `TwitchUserId`, `TwitchLogin`, `Roles`. |
| `SeededAuthUsers` | Пара свойств `Admin` и `Browser`. |

Синхронизация ищет пользователя сначала по `UserName`, затем по непустому `TwitchUserId`. Обновляет имя и Twitch-поля; при заданном пароле устанавливает или сбрасывает его. Роли приводятся **точно к набору seed**: недостающие добавляются, лишние удаляются. Повторное создание клиента с другим набором ролей может изменить того же пользователя в БД.

Пример подготовки пользователей для настоящего login, внутри теста с БД:

```csharp
SeededAuthUsers users = await app.SeedUsersAsync(new AuthTestSeedOptions
{
    Admin = new TestAuthUserSeed
    {
        UserName = "my_composer",
        Password = "TestComposerPass123!",
        Roles = [AppRoles.Composer],
    },
}, ct: TestContext.CancellationToken);

using HttpClient client = app.CreateAnonymousClient();
// users.Admin.UserName / Password передаются в Credentials login-запроса.
// users.Admin.UserId можно использовать как createdByUserId в TestDataBuilder.
```

Для `AppRoles` нужен `using Faryma.Composer.Domain`.

### Внутренние механизмы browser-helper

- [BrowserUserAuthenticationState][browser-state] — `internal record` с UserId, именем, Twitch-полями и ролями. `BrowserUserAuthenticationStateHolder.State` хранит состояние для конкретного производного host.
- [BrowserUserTestAuthHandler][browser-handler] — `internal` handler, `SchemeName = "TestBrowserUser"`. `HandleAuthenticateAsync()` формирует `NameIdentifier`, `Name`, роли и, при наличии, `twitch_user_id` / `preferred_username`. Настройка выполняется через `CreateBrowserUserClientAsync()`.
- Каждый browser-клиент получает свой производный host. `app.Services` у родительской фабрики относится к **родительскому** host. Это существенно при проверке singleton-состояния и уведомлений: данные в БД общие, каналы событий и накопители уведомлений — отдельные.

## 4. Подготовка данных: `TestDataBuilder`

Источник: [TestDataBuilder.cs][data-builder]. Namespace: `Faryma.Composer.Testing.Infrastructure`.

Создание: `TestDataBuilder data = new(app.Services);`. Нужен provider с настроенными persistence, Identity и `DateTimeContext`; фабрика с БД предоставляет такое окружение. Каждый метод открывает собственный async scope и сохраняет данные до возврата результата. Builder создаёт состояние через stores и прямое присваивание свойств; бизнес-переходы и события очереди при этом не выполняются.

Типы `UserEntity` и `ComposerStreamEntity` находятся в `Faryma.Composer.Domain.Entities`, `ReviewOrderEntity` — в `Faryma.Composer.Domain.Entities.TransactionSources`, перечисления — в `Faryma.Composer.Domain.Enums`.

### Пользователь и стрим

```csharp
Task<UserEntity> CreateUserAsync(string? userName = null)

Task<ComposerStreamEntity> CreateStreamAsync(
    Guid? createdByUserId = null,
    DateOnly? eventDate = null,
    ComposerStreamType type = ComposerStreamType.Donation,
    ComposerStreamStatus status = ComposerStreamStatus.Planned,
    DateTime? startedAt = null,
    DateTime? completedAt = null)
```

- `CreateUserAsync` создаёт Identity-пользователя без пароля и ролей. По умолчанию имя уникально (`user-<GUID>`), email строится из имени, `CreatedAt` берётся из `DateTimeContext.Now`. Для credentials/ролей используй auth-seeding из раздела 3.
- `CreateStreamAsync`: заданный `createdByUserId` должен уже существовать; при `null` создаётся новый пользователь. Статус и даты начала/завершения присваиваются из аргументов.
- Даты стримов по умолчанию: `clock.Today`, затем следующие дни. Счётчик общий для всех автоматически создаваемых стримов **внутри одного экземпляра builder**. Явные `eventDate` его не увеличивают. Для связанных данных удобно сохранять один экземпляр builder и передавать ID созданных стримов.

### Заказ: `CreateReviewOrderAsync(...)` → `Task<ReviewOrderEntity>`

Метод поддерживает следующие именованные аргументы:

| Аргумент | Тип / default | Поведение |
| --- | --- | --- |
| `createdByUserId` | `Guid? = null` | Существующий пользователь; при `null` создаётся новый. |
| `creationStreamId` | `long? = null` | Существующий стрим создания; при `null` создаётся новый `Donation` / `Planned`. |
| `processingStreamId` | `long? = null` | Существующий стрим обработки. Без ID для `InProgress` / `Completed` создаётся новый `Donation` / `Live` со `StartedAt = clock.Now`; для остальных статусов — `null`. |
| `nickname` | `string = "nickname"` | Существующий ник переиспользуется; иначе создаются ник и связанный счёт. Одинаковый ник в нескольких заказах связывает их с одним ником/счётом. |
| `type` | `ReviewOrderType.Donation` | Тип заказа. |
| `status` | `ReviewOrderStatus.Pending` | Начальное состояние заказа. |
| `isFrozen` | `bool = false` | Флаг заморозки. |
| `trackUrl` | `string? = null` | Для `Preorder` остаётся `null`; для других статусов `null` заменяется на `https://example.com/track`. Явный URL используется как есть. |
| `trackDurationSeconds` | `int? = null` | При наличии URL default — `60`; при отсутствии URL всегда `null`. |
| `nominalPrice` | `int = 1_000` | Передаётся в цену заказа (`Price`). |
| `payableAmount` | `int = 1_000` | Используется для `Donation` и `Free`; для остальных типов builder передаёт `0`. |
| `totalPaymentAmount` | `long = 0` | При `> 0` создаёт ручное пополнение счёта и оплату заказа на эту сумму через `TransactionStore`. |
| `queueCategory` | `QueueCategory.Unspecified` | Начальная категория очереди. |
| `inProgressAt`, `completedAt`, `canceledAt` | `DateTime? = null` | Временные отметки задаются явно. |
| `cancelReason` | `string? = null` | Причина отмены. |
| `reviewRating` | `int? = null` | При наличии создаёт связанный review. |

Дополнительные правила builder:

- Для `Canceled` он обнуляет `InProgressAt` и `ProcessingStream`, сбрасывает `QueueCategory` в `Unspecified`, независимо от переданных значений этих полей.
- `UserComment` всегда равен `"тестовый комментарий"`.
- Автоматически созданный processing-stream для `Completed` имеет статус `Live`; даты самого заказа автоматически из статуса не вычисляются. Нужное согласованное состояние задаёт тест.
- `trackUrl: null` для обычного заказа означает URL по умолчанию. Если сценарий требует иного состояния, подготовь его отдельным scoped-запросом.
- Возвращённые entities относятся к уже завершённому scope. Изменения их свойств после возврата сами в БД не сохраняются; для изменения используй `RunInScopeAsync`, для проверки после операции — повторное чтение.

### Пример Arrange: заказ с полной оплатой

Фрагмент внутри `DatabaseTestBase`; нужны namespaces API infrastructure/auth и `Faryma.Composer.Domain.Enums`.

```csharp
await using CustomWebApplicationFactory root = await CreateAppAsync();
DateTime now = DateTime.UtcNow;
CustomWebApplicationFactory app = root.WithFixedDateTime(now);
SeededAuthUsers users = await app.SeedUsersAsync(
    ct: TestContext.CancellationToken);
TestDataBuilder data = new(app.Services);

var stream = await data.CreateStreamAsync(
    createdByUserId: users.Admin.UserId,
    status: ComposerStreamStatus.Live,
    startedAt: now);

var order = await data.CreateReviewOrderAsync(
    createdByUserId: users.Admin.UserId,
    creationStreamId: stream.Id,
    nickname: "listener-1",
    nominalPrice: 1_000,
    payableAmount: 1_000,
    totalPaymentAmount: 1_000);

// После проверяемой операции перечитай состояние из БД:
var stored = await app.Services.GetOrderAsync(
    order.Id, TestContext.CancellationToken);
```

## 5. Scoped-сервисы, время и очередь

### Доступ к сохранённому состоянию

Источник: [TestServiceProviderExtensions.cs][provider-extensions]. Все методы расширяют `IServiceProvider`.

| Метод | Назначение / результат |
| --- | --- |
| `RunInScopeAsync<T>(Func<IServiceProvider, Task<T>> action)` | Открывает async scope, передаёт scoped provider, возвращает результат и освобождает scope. |
| `RunInScopeAsync(Func<IServiceProvider, Task> action)` | То же для действия без результата. Сохранение изменений выполняется внутри переданного действия явно. |
| `GetStreamAsync(long streamId, CancellationToken cancellationToken = default)` | `Task<ComposerStreamEntity>` через `ComposerStreamStore.GetStream`; отсутствие записи приводит к `NotFoundException`. |
| `GetOrderAsync(long orderId, CancellationToken cancellationToken = default)` | `Task<ReviewOrderEntity>` через `ReviewOrderStore.GetOrder`; также бросает `NotFoundException`. Загружает связи, предусмотренные store: стримы, транзакции, detailed-review payment с транзакциями, coverage redemption, review. |
| `CountStreamsAsync(CancellationToken cancellationToken = default)` | `Task<int>`: число всех стримов в БД, включая автоматически созданные builder. |

Пример точечной подготовки состояния, когда аргументов builder недостаточно:

```csharp
// using Faryma.Composer.Infrastructure;
// using Microsoft.Extensions.DependencyInjection;
await app.Services.RunInScopeAsync(async scoped =>
{
    AppDbContext db = scoped.GetRequiredService<AppDbContext>();
    var stored = await db.ReviewOrders.SingleAsync(
        x => x.Id == order.Id, TestContext.CancellationToken);
    stored.TrackUrl = null;
    stored.TrackDurationSeconds = null;
    await db.SaveChangesAsync(TestContext.CancellationToken);
});
```

### Подмена сервисов

Источник: [TestServiceCollectionExtensions.cs][collection-extensions]. Методы расширяют `IServiceCollection`, возвращают её для chaining и заменяют предыдущие регистрации соответствующего типа.

| Метод | Использование |
| --- | --- |
| `AddFixedDateTimeContext(DateTime now)` | Регистрирует singleton `new DateTimeContext(now)`. В API-тестах это уже обёрнуто в `WithFixedDateTime(now)`. |
| `AddTestOrderQueueNotificationService()` | Регистрирует singleton `IOrderQueueNotificationService → TestOrderQueueNotificationService`. API-фабрика вызывает автоматически; при самостоятельной сборке provider вызови после production-регистраций. |

Фиксированное время влияет на `DateTimeContext.Now` и `Today`. Прямые обращения к `DateTime.UtcNow` продолжают возвращать системное время; например, auth-seeding использует его для `CreatedAt`, а bearer-helper без БД — для срока JWT. Для сценария «прошло два часа» создавай `app.WithFixedDateTime(now.AddHours(2))` на той же БД; пример — тест истечения ключа идемпотентности.

### Обработка событий и уведомления

Источники: [TestServiceProviderExtensions.cs][provider-extensions], [TestOrderQueueNotificationService.cs][notifications].

| Член | Семантика |
| --- | --- |
| `services.DrainOrderQueueEventsAsync()` | Читает доступные события из `OrderQueueEventChannel` и последовательно вызывает `OrderQueueService.HandleEvent`. Завершается, когда канал пуст. Если channel или service не зарегистрированы, просто возвращается. |
| `services.GetOrderQueueNotifications()` | Получает `IOrderQueueNotificationService` и приводит к `TestOrderQueueNotificationService`. Требует установленную тестовую замену. |
| `TestOrderQueueNotificationService.UpdateCount` | Потокобезопасный счётчик полученных уведомлений. |
| `TestOrderQueueNotificationService.Snapshots` | Копия списка полученных `OrderQueueSnapshot` в порядке поступления. Копируется список, сами объекты snapshot не клонируются. |
| `NotifyQueueUpdated(OrderQueueSnapshot snapshot)` | Точка записи, которую вызывает `OrderQueueService`: добавляет snapshot, возвращает завершённый `Task`. |

Практический порядок проверки очереди:

1. Подготовь данные. Если они записаны builder после запуска host, обнови начальное in-memory состояние очереди перед проверяемой операцией: `await app.Services.GetRequiredService<OrderQueueService>().Initialize()`. Для этого нужен `Faryma.Composer.Application.Features.OrderQueue`; метод находится в production-коде, [OrderQueueService.cs][queue-service].
2. Получи `var notifications = app.Services.GetOrderQueueNotifications()`, запомни `int before = notifications.UpdateCount`, затем дождись завершения проверяемой операции.
3. Вызови `await app.Services.DrainOrderQueueEventsAsync()` и проверь `notifications.Snapshots.Skip(before)` / разницу счётчика.

Используй provider того host, который выполнял операцию. Bearer-клиент работает в host исходной фабрики, поэтому его запросам соответствует `app.Services`. Browser-helper возвращает только `HttpClient` производного host; его provider через родительский `app.Services` недоступен. `DrainOrderQueueEventsAsync()` обрабатывает уже доступные события, а не ожидает будущих и не инициализирует очередь. `TestOrderQueueNotificationService` предоставляет счётчик и snapshots; публичных методов ожидания следующего уведомления или сброса истории у него нет.

## 6. Проверка HTTP-ответов, JSON и дат

| Класс / метод | Как использовать |
| --- | --- |
| [ApiErrorAssertions][api-errors].`AssertApiErrorAsync(this HttpResponseMessage response, string code, CancellationToken cancellationToken = default)` → `Task<string?>` | Разбирает JSON, проверяет `type == "urn:composer:error:" + code`, возвращает `detail`. Передавай код без URN-префикса и `TestContext.CancellationToken`. Статус, title и detail при необходимости проверяй отдельно. |
| [DateTimeTestAssertions][date-assertions].`AssertSameInstant(DateTime? expected, DateTime? actual)` | Если expected — `null`, проверяет actual на `null`; иначе сравнивает оба значения после усечения до миллисекунд. |
| `DateTimeTestAssertions.TruncateToMilliseconds(DateTime value)` → `DateTime` | Собирает дату из компонентов до миллисекунд с `DateTimeKind.Utc`. Передавай уже UTC-значения: преобразования часового пояса здесь нет. |
| [TestJsonSerializerOptions][json-options].`Web` | Общие `JsonSerializerOptions(JsonSerializerDefaults.Web)` с `JsonStringEnumConverter`. Передавай в `PostAsJsonAsync`, `ReadFromJsonAsync` и другие операции сериализации DTO с enum. |

Пример проверки бизнес-ошибки, внутри теста с анонимным клиентом:

```csharp
using HttpResponseMessage response = await client.GetAsync(
    "/api/_test/exceptions/app", TestContext.CancellationToken);

Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
string? detail = await response.AssertApiErrorAsync("test_api", TestContext.CancellationToken);
Assert.AreEqual("Тестовая ошибка API", detail);
```

`AssertApiErrorAsync` рассчитан на указанный формат бизнес-ошибки. Например, ответ на `AuthenticationException` в существующих тестах содержит поле `Message`, а не `type` / `detail`; его проверяют отдельно.

Пример передачи JSON options (`System.Net.Http.Json`):

```csharp
var body = await response.Content.ReadFromJsonAsync<TestRequestContextResponse>(
    TestJsonSerializerOptions.Web,
    TestContext.CancellationToken);
```

## 7. Низкоуровневые помощники PostgreSQL

Используй их при подготовке собственного тестового окружения; `CustomWebApplicationFactory.CreateAsync(fixture)` уже выполняет этот путь за API-тест.

| Класс / метод | Назначение |
| --- | --- |
| [PostgreSqlFixture][postgres-fixture] | Содержит Testcontainers-контейнер `postgres:18-alpine`. `InitializeAsync()` запускает, `DisposeAsync()` освобождает контейнер. Сам класс не зависит от тестового фреймворка. |
| [PostgreSqlTestFixture][api-fixture].`GetAsync()` | Лениво запускает и возвращает общий контейнер для DB-тестов текущего testhost; `[AssemblyCleanup]` освобождает его после прогона. |
| `PostgreSqlFixture.Host`, `Port`, `Username`, `Password` | Параметры доступа к контейнеру; `Port` — динамически опубликованный порт. |
| `PostgreSqlFixture.CreateDatabaseAsync(string prefix)` → `Task<string>` | Создаёт БД `<prefix>_<GUID>` и возвращает её имя. Fixture должна быть запущена; prefix должен быть непустым. Результат `ExecScriptAsync` проверяется по `ExitCode`; схему этот метод не создаёт. |
| [PostgreSqlTestConfiguration][postgres-config].`CreatePostgreSqlSettings(PostgreSqlFixture fixture, string databaseName)` | Возвращает `IReadOnlyDictionary<string, string?>` с `POSTGRES:HOST`, `PORT`, `DATABASE`, `USERNAME`, `PASSWORD`, `POOLING=false`. Подключается через `AddInMemoryCollection`. |
| [PostgreSqlSchemaInitializer][postgres-schema].`EnsureCreatedAsync(IConfiguration configuration)` | Собирает временный provider через `AddPersistence`, получает `IDbContextFactory<AppDbContext>` и вызывает `context.Database.EnsureCreatedAsync()`. Это создание схемы по модели EF; миграции этим методом не прогоняются. |
| `TestConfiguration.CreateNoDatabase()` / `CreatePostgreSql(fixture, databaseName)` | `internal` API-специфичная конфигурация. Добавляет JWT, Twitch, bootstrap-credentials; для режима без БД содержит фиктивные PostgreSQL-настройки. Константы `JwtIssuer`, `JwtAudience`, `JwtSecretKey` согласованы с bearer-helper. |

Минимальная цепочка вне API-фабрики:

```csharp
// using Microsoft.Extensions.Configuration;
PostgreSqlFixture fixture = await PostgreSqlTestFixture.GetAsync();
string databaseName = await fixture.CreateDatabaseAsync("custom_test");
ConfigurationManager configuration = new();
configuration.AddInMemoryCollection(
    PostgreSqlTestConfiguration.CreatePostgreSqlSettings(fixture, databaseName));
await PostgreSqlSchemaInitializer.EnsureCreatedAsync(configuration);
```

Эта цепочка готовит БД и схему. Provider с application/Identity-сервисами для последующего `TestDataBuilder` собирается отдельно.

## 8. Тестовые контроллеры-пробы

Все перечисленные контроллеры подключаются API-фабрикой автоматически через `AddApplicationPart`. Они позволяют проверять инфраструктуру HTTP на коротком маршруте с предсказуемым поведением.

| Контроллер / методы | HTTP-маршруты и назначение |
| --- | --- |
| [TestAuthProbeController][auth-probe].`Authenticated()` / `Admin()` | `GET /api/_test/auth/authenticated` — `[Authorize]`; `GET /api/_test/auth/admin` — `[AuthorizeAdmins]`. При успехе возвращают имя пользователя в JSON-ответе. Удобны для матрицы `401` / `403` / `200` без БД. |
| `TestAuthProbeController.RateLimitedLogin()` | `POST /api/_test/auth/rate-limited-login`, политика `auth-login`; action возвращает `401`, при исчерпанном лимите запрос блокируется с `429`. |
| [TestRequestContextController][context-probe].`Get()` | `GET /api/_test/request-context`, `[AllowAnonymous]`. Возвращает `TestRequestContextResponse(Guid? UserId, Guid? ClaimsUserId, DateTime Now)` для сверки `CurrentUserContext`, claims и времени. |
| [TestExceptionProbeController][exception-probe].`ThrowAppException()` | `GET /api/_test/exceptions/app`: бросает внутренний `TestApiException`; ожидаемый ответ — `409`, код `test_api`. |
| `ThrowAuthenticationException()` / `ThrowUnhandledException()` | `GET /api/_test/exceptions/authentication` → `401`; `GET /api/_test/exceptions/unhandled` → `500`. Форматы проверены в `ApiExceptionHandlerTests`. |
| [TestIdempotencyController][idempotency-probe].`Execute(...)` | `POST /api/_test/idempotency`, `[Authorize]` + `[Idempotent]`. Нужна БД. Возвращает сценарий, payload и счётчик выполнений. |
| `TestIdempotencyController.UnsupportedResult()` | `POST /api/_test/idempotency/unsupported-result`: action возвращает `NoContent`, что позволяет проверить отказ фильтра идемпотентности на неподдерживаемый результат (`500`). |

### Управление сценарием идемпотентности

- `TestIdempotencyRequest(string Scenario, string Value, int DelayMilliseconds = 0)` — тело запроса; задержка помогает создать конкурентные запросы.
- `TestIdempotencyResponse(string Scenario, string Value, int Executions)` — ответ action.
- `TestIdempotencyController.Reset(string scenario)` — очищает счётчик и флаг ошибки для сценария.
- `FailNext(string scenario)` — следующее дошедшее до action выполнение этого сценария бросит `TestIdempotencyException` до увеличения счётчика.
- `GetExecutionCount(string scenario)` — число выполнений action; повторно выданный сохранённый ответ его не увеличивает.

Счётчики статические и общие для разных host: используй уникальный `Scenario`, например `Guid.NewGuid().ToString("N")`. Заголовок ключа берётся из `Faryma.Composer.Domain.Globals.IdempotencyKey`.

Пример одного запроса с уже созданным аутентифицированным `client` фабрики с БД:

```csharp
string scenario = Guid.NewGuid().ToString("N");
TestIdempotencyController.Reset(scenario);
Guid key = Guid.NewGuid();

using HttpRequestMessage request = new(HttpMethod.Post, "/api/_test/idempotency");
request.Headers.Add(Globals.IdempotencyKey, key.ToString("D"));
request.Content = JsonContent.Create(new TestIdempotencyRequest(scenario, "payload"));

using HttpResponseMessage response = await client.SendAsync(
    request, TestContext.CancellationToken);
```

Для replay создай новый `HttpRequestMessage` с тем же ключом, телом и пользователем. Для проверки восстановления после ошибки вызови `FailNext(scenario)` перед первым запросом. Полные последовательные и конкурентные сценарии находятся в `IdempotentAttributeTests`.

## 9. Готовые примеры в тестах

Это навигация по существующим сценариям. Методы и вложенные типы с `private` доступны только внутри своего тестового класса; используй их реализацию как образец, а общие helpers — через API выше.

| Файл | Что брать за образец |
| --- | --- |
| [Smoke/ApiSmokeTests.cs][smoke-tests] | Запуск без БД, собственные `WebApplicationFactoryClientOptions`, чтение OpenAPI. |
| [Features/Auth/AuthHelpersTests.cs][auth-helper-tests] | Анонимный, bearer и тестовый browser-клиенты, пользовательская роль в `TestAuthUserSeed`. |
| [Features/Auth/AdminAccessTests.cs][admin-tests] | Матрица доступа к admin-endpoint: `401`, `403`, `200`. |
| [Features/Auth/BrowserLoginTests.cs][browser-login-tests] | Настоящий Twitch OIDC challenge. Локальный `CreateBrowserLoginClient` задаёт `OpenIdConnectOptions.Configuration` через `CreateDerivedFactory` для проверки redirect без запроса discovery к Twitch. |
| [Features/Auth/BrowserAdminLoginTests.cs][browser-admin-tests] | Настоящий cookie-login, отказ по роли/паролю, desktop JWT login. Локальные `SeedAdmin`, `Login`, `GetBrowserAuthCookie` показывают seeding, тело credentials и извлечение `faryma_browser_auth` из `Set-Cookie`. |
| [Features/Auth/RefreshTokenTests.cs][refresh-tests] | Выдача, ротация, replay, истечение refresh token. Локальные `Login`, `Refresh`, `ReadTokens` используют настоящие endpoints. Вложенный `RefreshTokenReadBarrierInterceptor.Arm()` синхронизирует два чтения `refresh_tokens`, чтобы воспроизвести гонку; подключение — через производную фабрику и EF interceptor. |
| [Features/Auth/AuthRateLimitForwardedHeadersTests.cs][rate-limit-tests] | `ConfigureAppConfiguration` для trusted proxies/networks, заголовки `X-Forwarded-For` / `X-Forwarded-Proto`. Вложенный `RemoteIpAddressStartupFilter.Configure(...)` подставляет исходный IP соединения. |
| [Common/RequestContextMiddlewareTests.cs][context-tests] | Сверка UserId из request context и claims; локальный `ReadResponseAsync` читает `TestRequestContextResponse`. |
| [Common/ApiExceptionHandlerTests.cs][exception-tests] | Реальные форматы ответов на бизнес-, authentication- и необработанное исключения. |
| [Common/IdempotentAttributeTests.cs][idempotency-tests] | Replay, конкуренция, конфликт payload, повтор после ошибки, истечение ключа с `WithFixedDateTime`. Локальные `NewScenario`, `SendAsync`, `SendUnsupportedResultAsync`, `ReadResponseAsync` показывают протокол запросов. |

## 10. Запуск

Из каталога `backend`, при установленном .NET 10 SDK и доступном Docker для PostgreSQL-сценариев:

```powershell
dotnet test tests/Faryma.Composer.Application.Test/Faryma.Composer.Application.Test.csproj
dotnet test tests/Faryma.Composer.Api.Test/Faryma.Composer.Api.Test.csproj
```

Для одного класса через VSTest-фильтр MSTest:

```powershell
dotnet test tests/Faryma.Composer.Api.Test/Faryma.Composer.Api.Test.csproj --filter "ClassName=Faryma.Composer.Api.Test.Common.IdempotentAttributeTests"
```

Итоговая строка VSTest сообщает количество пройденных, не пройденных, пропущенных и всего обнаруженных тестов. Общий проект `Faryma.Composer.Testing` предоставляет инфраструктуру и не является тестовым проектом; запускать сценарии следует из использующего её тестового проекта.

[api-fixture]: ../tests/Faryma.Composer.Api.Test/Infrastructure/TestBases.cs
[parallelization]: ../tests/Faryma.Composer.Api.Test/Infrastructure/AssemblyParallelization.cs
[api-factory]: ../tests/Faryma.Composer.Api.Test/Infrastructure/CustomWebApplicationFactory.cs
[api-config]: ../tests/Faryma.Composer.Api.Test/Infrastructure/TestConfiguration.cs
[auth-extensions]: ../tests/Faryma.Composer.Api.Test/Infrastructure/Auth/CustomWebApplicationFactoryAuthExtensions.cs
[auth-models]: ../tests/Faryma.Composer.Api.Test/Infrastructure/Auth/AuthTestModels.cs
[browser-state]: ../tests/Faryma.Composer.Api.Test/Infrastructure/Auth/BrowserUserAuthenticationState.cs
[browser-handler]: ../tests/Faryma.Composer.Api.Test/Infrastructure/Auth/BrowserUserTestAuthHandler.cs
[data-builder]: ../tests/Faryma.Composer.Testing/Infrastructure/TestDataBuilder.cs
[provider-extensions]: ../tests/Faryma.Composer.Testing/Infrastructure/TestServiceProviderExtensions.cs
[collection-extensions]: ../tests/Faryma.Composer.Testing/Infrastructure/TestServiceCollectionExtensions.cs
[notifications]: ../tests/Faryma.Composer.Testing/Infrastructure/TestOrderQueueNotificationService.cs
[queue-service]: ../src/Faryma.Composer.Application/Features/OrderQueue/OrderQueueService.cs
[api-errors]: ../tests/Faryma.Composer.Api.Test/Infrastructure/ApiErrorAssertions.cs
[date-assertions]: ../tests/Faryma.Composer.Api.Test/Infrastructure/DateTimeTestAssertions.cs
[json-options]: ../tests/Faryma.Composer.Api.Test/Infrastructure/TestJsonSerializerOptions.cs
[postgres-fixture]: ../tests/Faryma.Composer.Testing/Infrastructure/PostgreSqlFixture.cs
[postgres-config]: ../tests/Faryma.Composer.Testing/Infrastructure/PostgreSqlTestConfiguration.cs
[postgres-schema]: ../tests/Faryma.Composer.Testing/Infrastructure/PostgreSqlSchemaInitializer.cs
[auth-probe]: ../tests/Faryma.Composer.Api.Test/Infrastructure/TestAuthProbeController.cs
[context-probe]: ../tests/Faryma.Composer.Api.Test/Infrastructure/TestRequestContextController.cs
[exception-probe]: ../tests/Faryma.Composer.Api.Test/Infrastructure/TestExceptionProbeController.cs
[idempotency-probe]: ../tests/Faryma.Composer.Api.Test/Infrastructure/TestIdempotencyController.cs
[smoke-tests]: ../tests/Faryma.Composer.Api.Test/Smoke/ApiSmokeTests.cs
[auth-helper-tests]: ../tests/Faryma.Composer.Api.Test/Features/Auth/AuthHelpersTests.cs
[admin-tests]: ../tests/Faryma.Composer.Api.Test/Features/Auth/AdminAccessTests.cs
[browser-login-tests]: ../tests/Faryma.Composer.Api.Test/Features/Auth/BrowserLoginTests.cs
[browser-admin-tests]: ../tests/Faryma.Composer.Api.Test/Features/Auth/BrowserAdminLoginTests.cs
[refresh-tests]: ../tests/Faryma.Composer.Api.Test/Features/Auth/RefreshTokenTests.cs
[rate-limit-tests]: ../tests/Faryma.Composer.Api.Test/Features/Auth/AuthRateLimitForwardedHeadersTests.cs
[context-tests]: ../tests/Faryma.Composer.Api.Test/Common/RequestContextMiddlewareTests.cs
[exception-tests]: ../tests/Faryma.Composer.Api.Test/Common/ApiExceptionHandlerTests.cs
[idempotency-tests]: ../tests/Faryma.Composer.Api.Test/Common/IdempotentAttributeTests.cs
