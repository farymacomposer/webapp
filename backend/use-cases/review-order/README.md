# Review Order Use-Cases

Документы в этой папке описывают поведение `ReviewOrder` на границе `API -> application`.

Важно различать два вида состояний:
- `ReviewOrder.Status` - доменный persisted-статус заказа;
- `OrderActivityStatus` - вычисляемое состояние позиции заказа в очереди, описанное в `../order-queue/order-activity-status-graph.md`.

## Сценарии

| Use case | Файл | Эндпоинт |
| --- | --- | --- |
| Создать заказ | `create-review-order.md` | `POST /api/ReviewOrder/CreateReviewOrder` |
| Поднять заказ в очереди | `move-up-review-order.md` | `POST /api/ReviewOrder/MoveUpReviewOrder` |
| Добавить или изменить ссылку на трек | `add-track-url.md` | `POST /api/ReviewOrder/AddTrackUrl` |
| Взять заказ в работу | `take-order-in-progress.md` | `POST /api/ReviewOrder/TakeOrderInProgress` |
| Выполнить заказ | `complete-review-order.md` | `POST /api/ReviewOrder/CompleteReviewOrder` |
| Заморозить заказ | `freeze-review-order.md` | `POST /api/ReviewOrder/FreezeReviewOrder` |
| Разморозить заказ | `unfreeze-review-order.md` | `POST /api/ReviewOrder/UnfreezeReviewOrder` |
| Отменить заказ | `cancel-review-order.md` | `POST /api/ReviewOrder/CancelReviewOrder` |
| Граф статусов заказа | `review-order-status-graph.md` | persisted `ReviewOrder.Status` |

## Связанные графы

- `review-order-status-graph.md` описывает переходы persisted-статуса `ReviewOrder.Status`.
- `../order-queue/order-activity-status-graph.md` описывает вычисляемую активность позиции заказа в очереди.
