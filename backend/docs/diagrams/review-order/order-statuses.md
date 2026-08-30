# Статусы и активность заказа разбора

```mermaid
flowchart TB
    subgraph DOMAIN["Доменный статус · ReviewOrderStatus"]
        direction LR
        CREATE(("Создание"))
        PRE["Preorder<br/>нет ссылки"]
        AWAIT["AwaitingPayment<br/>ссылка есть, оплаты недостаточно"]
        PENDING["Pending<br/>можно взять в работу"]
        INPROGRESS["InProgress"]
        COMPLETED["Completed"]
        CANCELED["Canceled"]

        CREATE -->|"без ссылки"| PRE
        CREATE -->|"ссылка есть,<br/>оплаты недостаточно"| AWAIT
        CREATE -->|"ссылка есть,<br/>оплата достаточна или не нужна"| PENDING

        PRE -->|"добавить ссылку;<br/>оплаты недостаточно"| AWAIT
        PRE -->|"добавить ссылку;<br/>оплата достаточна или не нужна"| PENDING
        PRE -->|"оплата без ссылки:<br/>статус сохраняется"| PRE
        AWAIT -->|"доплата покрыла<br/>требуемую сумму"| PENDING
        PENDING -->|"смена длительности:<br/>возникла доплата"| AWAIT

        PENDING -->|"взять в работу:<br/>не заморожен, есть любой Live-стрим,<br/>слот свободен"| INPROGRESS
        INPROGRESS -->|"выполнить"| COMPLETED

        PRE -->|"отменить"| CANCELED
        AWAIT -->|"отменить"| CANCELED
        PENDING -->|"отменить"| CANCELED
        INPROGRESS -->|"отменить"| CANCELED
    end

    CART["Категория «корзина»<br/>ещё нельзя взять"]:::category
    PRE -.-> CART
    AWAIT -.-> CART

    subgraph FREEZE["Ортогональный флаг · IsFrozen"]
        direction LR
        THAWED["false · не заморожен"]
        FROZEN_FLAG["true · заморожен"]
        THAWED -->|"freeze"| FROZEN_FLAG
        FROZEN_FLAG -->|"unfreeze"| THAWED
        FREEZE_SCOPE["Допустим только при<br/>Preorder / AwaitingPayment / Pending"]
    end

    PRE -. "флаг применим" .-> FREEZE_SCOPE
    AWAIT -. "флаг применим" .-> FREEZE_SCOPE
    PENDING -. "флаг применим" .-> FREEZE_SCOPE

    subgraph ACTIVITY["Вычисляемая активность · OrderActivityStatus"]
        direction TB
        WAITING{"Доменный статус:<br/>Preorder / AwaitingPayment / Pending"}
        ACTIVITY_FROZEN{"IsFrozen?"}
        STREAM_DATE{"CreationStream.EventDate<br/>позже ближайшей даты?"}
        ACTIVE_TYPE{"Тип участвует<br/>в приоритете Active?"}
        ACTIVE["Active"]
        SCHEDULED["Scheduled"]
        FROZEN_ACTIVITY["Frozen"]
        UNSPECIFIED_ACTIVITY["Активность не задана"]
        INPROGRESS_ACTIVITY["InProgress"]
        COMPLETED_ACTIVITY["Completed"]
        REMOVED["Removed"]
        PROCESSING_COMPLETED{"ProcessingStream<br/>завершён?"}

        WAITING --> ACTIVITY_FROZEN
        ACTIVITY_FROZEN -->|"да"| FROZEN_ACTIVITY
        ACTIVITY_FROZEN -->|"нет"| STREAM_DATE
        STREAM_DATE -->|"да"| SCHEDULED
        STREAM_DATE -->|"нет"| ACTIVE_TYPE
        ACTIVE_TYPE -->|"OutOfQueue, Donation или Free"| ACTIVE
        ACTIVE_TYPE -->|"Charity или иной тип"| UNSPECIFIED_ACTIVITY

        ACTIVE -->|"freeze"| FROZEN_ACTIVITY
        SCHEDULED -->|"freeze"| FROZEN_ACTIVITY
        UNSPECIFIED_ACTIVITY -->|"freeze"| FROZEN_ACTIVITY
        FROZEN_ACTIVITY -->|"unfreeze:<br/>новый пересчёт"| STREAM_DATE

        PROCESSING_COMPLETED -->|"нет"| COMPLETED_ACTIVITY
        PROCESSING_COMPLETED -->|"да"| REMOVED
    end

    PRE -.-> WAITING
    AWAIT -.-> WAITING
    PENDING -.-> WAITING
    INPROGRESS ==>|"определяет"| INPROGRESS_ACTIVITY
    COMPLETED ==>|"определяет"| PROCESSING_COMPLETED
    CANCELED ==>|"всегда"| REMOVED

    subgraph CONTEXT["Контекст очереди — не статусы заказа"]
        direction LR
        CREATION_STREAM["CreationStream<br/>стрим создания/назначения<br/>после создания не меняется"]
        NEAREST_STREAM_DATE["Ближайшая дата стрима<br/>пересчитывается при изменениях стримов"]
        PROCESSING_STREAM["ProcessingStream<br/>стрим обработки<br/>задаётся при take"]
        QUEUE_CATEGORY["QueueCategory<br/>категория приоритета Active:<br/>OutOfQueue / Donation / Debt"]
    end

    CREATE -. "назначает" .-> CREATION_STREAM
    CREATION_STREAM -. "даёт EventDate" .-> STREAM_DATE
    NEAREST_STREAM_DATE -. "сравнивается с EventDate" .-> STREAM_DATE
    INPROGRESS -. "фиксирует текущий Live-стрим" .-> PROCESSING_STREAM
    ACTIVE -. "упорядочивается категорией" .-> QUEUE_CATEGORY
    INPROGRESS -. "при take фиксируется категория,<br/>только если заказ был Active" .-> QUEUE_CATEGORY

    classDef domain fill:#e8f1ff,stroke:#3267a8,color:#132238;
    classDef terminal fill:#eceff3,stroke:#59636e,color:#1f252b;
    classDef activity fill:#e8f7ed,stroke:#2f7d4a,color:#173820;
    classDef unspecified fill:#fff,stroke:#777,stroke-dasharray: 5 5,color:#111;
    classDef flag fill:#fff3d6,stroke:#a66b00,color:#4d3200;
    classDef context fill:#f4edff,stroke:#7451a6,color:#302044;
    classDef category fill:#fff8e6,stroke:#9a7418,color:#45350b;

    class PRE,AWAIT,PENDING,INPROGRESS domain;
    class COMPLETED,CANCELED terminal;
    class ACTIVE,SCHEDULED,FROZEN_ACTIVITY,INPROGRESS_ACTIVITY,COMPLETED_ACTIVITY,REMOVED activity;
    class UNSPECIFIED_ACTIVITY unspecified;
    class THAWED,FROZEN_FLAG,FREEZE_SCOPE flag;
    class CREATION_STREAM,NEAREST_STREAM_DATE,PROCESSING_STREAM,QUEUE_CATEGORY context;
```

Легенда:

- **Статус** — сохранённое доменное состояние заказа; `Preorder` и `AwaitingPayment` — разные статусы одной смысловой категории «корзина».
- **Активность** — вычисляемое положение заказа в очереди. `Scheduled`, `Frozen` и `Removed` не являются доменными статусами.
- **Флаг** `IsFrozen` не меняет статус. После разморозки активность вычисляется заново по неизменному стриму создания, текущей ближайшей дате и типу заказа; прежняя позиция не восстанавливается.
- **Категория** задаёт приоритет только внутри активной очереди и не определяет, можно ли взять конкретный `Pending`-заказ.
- **Стрим создания** назначает заказ при создании; **стрим обработки** фиксируется отдельно при взятии в работу.
