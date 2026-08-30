# Назначение стрима при создании заказа

```mermaid
flowchart TD
    Start([Администратор создаёт заказ]) --> OrderType{Тип создаваемого заказа}

    OrderType -->|Вне очереди<br/>OutOfQueue| AnyNearest["Найти ближайший доступный стрим<br/>любого типа"]
    OrderType -->|Донатный<br/>Donation| HasHistory{У ника есть хотя бы<br/>один заказ?}
    OrderType -->|Бесплатный<br/>Free| HasHistory
    OrderType -->|Благотворительный<br/>Charity| LiveCharity["Найти запущенный благотворительный стрим<br/>Live + Charity"]

    HasHistory -->|Нет| AnyNearest
    HasHistory -->|Да, включая отменённый| DonationNearest["Найти ближайший доступный<br/>донатный стрим (Donation)"]

    AnyNearest --> AnyFound{Стрим найден?}
    DonationNearest --> DonationFound{Стрим найден?}
    LiveCharity --> CharityFound{Стрим найден?}

    AnyFound -->|Да| Assigned["Назначить найденный стрим<br/>стримом создания заказа"]
    DonationFound -->|Да| Assigned
    CharityFound -->|Да| Assigned

    AnyFound -->|Нет| Rejected([Заказ не создаётся])
    DonationFound -->|Нет| Rejected
    CharityFound -->|Нет| Rejected

    Assigned --> Created([Заказ создан])

    AnyNearest -.-> DebtNote["Выбор «любого типа» может привести<br/>к долговому стриму (Debt)"]
    DebtNote -.-> DebtBoundary["Debt не является типом заказа,<br/>создаваемого этим сценарием"]
```

**Легенда и границы.** Ближайший доступный стрим — запущенный либо запланированный на сегодня или будущее, с самой ранней датой. Для `Charity` подходит только уже запущенный стрим `Live + Charity`; история ника не учитывается. Диаграмма показывает только выбор **стрима создания**: стрим, на котором заказ позднее возьмут в работу и обработают, может отличаться.
