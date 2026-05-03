# Review Order Use-Cases

Документы в этой папке описывают поведение `ReviewOrder` на границе `API -> application`.

Важно различать два вида состояний:
- `ReviewOrder.Status` - доменный persisted-статус заказа;
- `OrderActivityStatus` - вычисляемое состояние позиции заказа в очереди, описанное в `../order-queue/order-activity-status-graph.md`.

## Сценарии

| Use case | Файл | Эндпоинт |
| --- | --- | --- |
| Создать донатный заказ | `create-donation-review-order.md` | `POST /api/review-orders/create/donation` |
| Создать внеочередной заказ | `create-out-of-queue-review-order.md` | `POST /api/review-orders/create/out-of-queue` |
| Создать бесплатный заказ | `create-free-review-order.md` | `POST /api/review-orders/create/free` |
| Создать благотворительный заказ | `create-charity-review-order.md` | `POST /api/review-orders/create/charity` |
| Оплатить заказ | `pay-review-order.md` | `POST /api/review-orders/pay` |
| Оплатить подробный разбор заказа | `pay-detailed-review-order.md` | `POST /api/review-orders/pay-detailed-review` |
| Добавить или изменить ссылку на трек | `add-track-url.md` | `POST /api/review-orders/track-url` |
| Взять заказ в работу | `take-order-in-progress.md` | `POST /api/review-orders/take-in-progress` |
| Выполнить заказ | `complete-review-order.md` | `POST /api/review-orders/complete` |
| Заморозить заказ | `freeze-review-order.md` | `POST /api/review-orders/freeze` |
| Разморозить заказ | `unfreeze-review-order.md` | `POST /api/review-orders/unfreeze` |
| Отменить заказ | `cancel-review-order.md` | `POST /api/review-orders/cancel` |
| Граф статусов заказа | `review-order-status-graph.md` | persisted `ReviewOrder.Status` |

Отдельный сценарий `pay-extra-time-review-order.md` оставлен как архивное описание: обязательная доплата за длительность теперь входит в стоимость заказа и покрывается через обычную оплату заказа.

## Связанные графы

- `review-order-status-graph.md` описывает переходы persisted-статуса `ReviewOrder.Status`.
- `../order-queue/order-activity-status-graph.md` описывает вычисляемую активность позиции заказа в очереди.
