# Активность и приоритет заказов на разбор

```mermaid
flowchart TB
    Q["Заказы текущего снимка очереди<br/>и обновления уже отслеживаемых заказов"] --> R{"Canceled<br/>или стрим обработки Completed?"}

    subgraph activity["Вычисляемая активность"]
        R -->|да| REM["Removed"]
        R -->|нет| C{"Доменный статус<br/>Completed?"}
        C -->|да| COM["Completed"]
        C -->|нет| P{"Доменный статус<br/>InProgress?"}
        P -->|да| INP["InProgress"]
        P -->|нет| F{"IsFrozen?"}
        F -->|да| FRO["Frozen"]
        F -->|нет| S{"Дата стрима создания<br/>позже ближайшей даты?"}
        S -->|да| SCH["Scheduled"]
        S -->|нет| AT{"Тип участвует<br/>в приоритете Active?"}
        AT -->|OutOfQueue, Donation или Free| ACT["Active"]
        AT -->|Charity или иной тип| NA["Активность не задана<br/>категория не задана"]
    end

    ACT --> T{"Тип заказа"}

    subgraph categories["Категории приоритета — только внутри Active"]
        T -->|OutOfQueue| OQ["OutOfQueue<br/>по CreatedAt ↑"]
        T -->|Donation или Free| D{"Дата стрима создания"}
        D -->|ближайшая дата| DON["Donation<br/>PaidPriorityAmount ↓,<br/>затем CreatedAt ↑"]
        D -->|прошлая дата| DB["Debt[DebtIndex]<br/>отдельная корзина на каждую дату;<br/>ближайшая прошлая = 0,<br/>следующая = 1, …"]
    end

    OQ --> ALG
    DON --> ALG
    DB --> ALG

    subgraph priority["Построение порядка Active"]
        ALG["Сохранённое состояние:<br/>последняя категория, общий последний ник,<br/>последний ник каждой корзины,<br/>последняя Debt-дата"] --> SEL["Выбор следующего:<br/>OutOfQueue → Donation → Debt"]
        SEL --> NICK{"Есть альтернатива<br/>по нику?"}
        NICK -->|да| ALT["Не повторять общий последний ник<br/>и, где применимо, последний ник корзины"]
        NICK -->|нет| FALL["Разрешить единственный оставшийся ник"]
        ALT --> RR
        FALL --> RR
        RR["Для Debt: round-robin по всем<br/>прошлым датам; пустые корзины пропускаются"] --> TAKE["Извлечь заказ,<br/>обновить состояние"]
        TAKE --> MORE{"Остались заказы<br/>в категориях?"}
        MORE -->|да| SEL
        MORE -->|нет| POS["Индексы Active<br/>0, 1, 2, …"]
    end

    MP{"Ручное взятие:<br/>статус Pending, не заморожен,<br/>есть любой Live-стрим и слот свободен?"}
    MP -->|да, любой тип и позиция| WORK["Перевести в InProgress"]
    WORK --> HC{"В момент взятия была<br/>категория Active?"}
    HC -->|да| SHIFT["Записать категорию;<br/>продолжить приоритет от неё"]
    HC -->|нет: например Charity<br/>или Scheduled| KEEP["Категорию не записывать;<br/>приоритет Active не сдвигать"]

    classDef activityNode fill:#e8f1ff,stroke:#4472c4,color:#111;
    classDef categoryNode fill:#fff2cc,stroke:#bf9000,color:#111;
    classDef terminalNode fill:#eeeeee,stroke:#666,color:#111;
    classDef unspecifiedNode fill:#fff,stroke:#777,stroke-dasharray: 5 5,color:#111;
    class ACT,SCH,FRO,INP activityNode;
    class OQ,DON,DB categoryNode;
    class REM,COM terminalNode;
    class NA unspecifiedNode;
```

**Легенда и границы.** Очередь здесь — текущий снимок, а не доменный статус. При начальной загрузке в него входят `Preorder`, `AwaitingPayment`, `Pending`, `InProgress` и только те `Completed`, чей стрим обработки ещё `Live`. `Canceled` и заказы с завершённым стримом обработки временно получают `Removed`, если они уже отслеживались, после чего удаляются из снимка. `ReviewOrderStatus`, `ReviewOrderType`, вычисляемая активность и `QueueCategory` — разные измерения; `Free` использует те же корзины `Donation`/`Debt`, что и `Donation`. `Charity` не получает ни `Active`, ни категорию. Схема показывает только вычисление активности и приоритета `Active`: она не запрещает вручную взять любой незамороженный `Pending`, включая `Charity` и `Scheduled`.
