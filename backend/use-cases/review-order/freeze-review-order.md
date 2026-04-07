# Use-Case: Заморозить заказ

## Эндпоинт
- Метод: `POST`
- Путь: `/api/ReviewOrder/FreezeReviewOrder`

## Что делает
Включает флаг заморозки заказа (`IsFrozen = true`), чтобы временно исключить его из обычной обработки.

## Входные данные
- Body: `FreezeReviewOrderRequest`
  - `ReviewOrderId`

## Что можно
- Заморозить заказ в статусе `Preorder` или `Pending`.
- Повторно вызвать для уже замороженного заказа (идемпотентно, вернется текущий заказ).

## Что нельзя
- Заморозить несуществующий заказ.
- Заморозить заказ в `InProgress`, `Completed` или `Canceled`.

## Условия выполнения
- Требуется роль администратора (`AuthorizeAdmins`).
- Заказ должен существовать и быть в допустимом статусе.

## Результат
- `200 OK`
- Тело: `FreezeReviewOrderResponse`
  - `ReviewOrder: ReviewOrderDto`

## На что влияет
- Обновляет флаг `IsFrozen` в БД.
- Публикует событие `ReviewOrderChangedEvent` с типом `OrderFrozen`.
