# Граф переходов статусов ReviewOrder

## Что отражает граф

Этот граф описывает только persisted-статус `ReviewOrder.Status`.
Он не описывает вычисляемую активность позиции заказа в очереди. Для нее используется `../order-queue/order-activity-status-graph.md`.

## Mermaid

```mermaid
---
config:
  layout: elk
  theme: neo-dark
  look: neo
---
stateDiagram
  direction TB
  [*] --> Preorder:Создание заказа без TrackUrl
  [*] --> Pending:Создание заказа с TrackUrl и полным покрытием
  [*] --> AwaitingPayment:Создание заказа с TrackUrl и частичным покрытием
  Preorder --> Pending:AddTrackUrl, покрытие достаточно
  Preorder --> AwaitingPayment:AddTrackUrl, покрытие недостаточно
  AwaitingPayment --> Pending:PayReviewOrder или AddTrackUrl, покрытие достаточно
  Pending --> AwaitingPayment:AddTrackUrl увеличил обязательную стоимость
  Pending --> InProgress:TakeOrderInProgress
  InProgress --> Completed:CompleteReviewOrder
  Preorder --> Canceled:CancelReviewOrder
  Pending --> Canceled:CancelReviewOrder
  AwaitingPayment --> Canceled:CancelReviewOrder
  InProgress --> Canceled:CancelReviewOrder
```

## Правила переходов

- `Unspecified` не является допустимым persisted-статусом заказа и не входит в граф.
- `Completed` и `Canceled` являются терминальными статусами.
- Обратных переходов нет.
- Удаление `TrackUrl` не поддерживается, поэтому переходы в `Preorder` из заказов с треком исключены.
- `FreezeReviewOrder` и `UnfreezeReviewOrder` не изменяют `ReviewOrder.Status`.
- Идемпотентные повторные вызовы считаются `no-op` и на диаграмме не отображаются.
- Тип заказа не влияет на допустимые переходы persisted-статусов.
- `AwaitingPayment` означает, что трек известен, но обязательная стоимость заказа покрыта не полностью.

## Переходы persisted-статуса

| Откуда | Куда | Операция | Условия |
| --- | --- | --- | --- |
| `-` | `Preorder` | `CreateDonationReviewOrder`, `CreateOutOfQueueReviewOrder`, `CreateFreeReviewOrder`, `CreateCharityReviewOrder` | `TrackUrl` не передан; для checkout-типов покрытие есть |
| `-` | `Pending` | `CreateDonationReviewOrder`, `CreateOutOfQueueReviewOrder`, `CreateFreeReviewOrder`, `CreateCharityReviewOrder` | `TrackUrl` передан; для checkout-типов обязательная стоимость покрыта |
| `-` | `AwaitingPayment` | `CreateDonationReviewOrder`, `CreateOutOfQueueReviewOrder`, `CreateFreeReviewOrder` | `TrackUrl` передан, покрытие меньше обязательной стоимости |
| `Preorder` | `Pending` | `AddTrackUrl` | В заказ добавлен трек, покрытие достаточно |
| `Preorder` | `AwaitingPayment` | `AddTrackUrl` | В заказ добавлен трек, покрытие недостаточно |
| `AwaitingPayment` | `Pending` | `PayReviewOrder`, `AddTrackUrl` | Обязательная стоимость стала покрыта |
| `Pending` | `AwaitingPayment` | `AddTrackUrl` | Изменение трека увеличило обязательную стоимость выше покрытия |
| `Pending` | `InProgress` | `TakeOrderInProgress` | Заказ не заморожен и взят в работу |
| `InProgress` | `Completed` | `CompleteReviewOrder` | Заказ находится в работе |
| `Preorder` | `Canceled` | `CancelReviewOrder` | Отмена разрешена |
| `Pending` | `Canceled` | `CancelReviewOrder` | Отмена разрешена |
| `AwaitingPayment` | `Canceled` | `CancelReviewOrder` | Отмена разрешена |
| `InProgress` | `Canceled` | `CancelReviewOrder` | Отмена разрешена |

## Допустимые операции без смены persisted-статуса

| Статус | Операция | Что происходит |
| --- | --- | --- |
| `Pending` | `AddTrackUrl` | Обновляются `TrackUrl`, длительность и обязательная стоимость; статус не меняется, если покрытие остается достаточным |
| `Preorder` | `PayReviewOrder` | Добавляется платеж, статус не меняется до появления трека |
| `Pending` | `PayReviewOrder` | Добавляется платеж, статус не меняется |
| `AwaitingPayment` | `PayReviewOrder` | Добавляется платеж, если покрытия все еще недостаточно, статус не меняется |
| `Preorder` | `FreezeReviewOrder` | Меняется `IsFrozen`, статус не меняется |
| `Pending` | `FreezeReviewOrder` | Меняется `IsFrozen`, статус не меняется |
| `AwaitingPayment` | `FreezeReviewOrder` | Меняется `IsFrozen`, статус не меняется |
| `Preorder` | `UnfreezeReviewOrder` | Меняется `IsFrozen`, статус не меняется |
| `Pending` | `UnfreezeReviewOrder` | Меняется `IsFrozen`, статус не меняется |
| `AwaitingPayment` | `UnfreezeReviewOrder` | Меняется `IsFrozen`, статус не меняется |

## Примечания по событиям

- В событиях создания заказа `ReviewOrderChangedEvent` публикуется с `PreviousStatus = Unspecified`.
- Это значение относится к payload события и не означает, что persisted-статус созданного заказа равен `Unspecified`.
