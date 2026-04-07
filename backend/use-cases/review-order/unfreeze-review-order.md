# Use-Case: Разморозить заказ

## Эндпоинт
- Метод: `POST`
- Путь: `/api/ReviewOrder/UnfreezeReviewOrder`

## Что делает
Выключает флаг заморозки заказа (`IsFrozen = false`), возвращая его к обычной обработке очереди.

## Входные данные
- Body: `UnfreezeReviewOrderRequest`
  - `ReviewOrderId`

## Что можно
- Разморозить заказ в статусе `Preorder` или `Pending`.
- Повторно вызвать для уже размороженного заказа (идемпотентно, вернется текущий заказ).

## Что нельзя
- Разморозить несуществующий заказ.
- Разморозить заказ в `InProgress`, `Completed` или `Canceled`.

## Условия выполнения
- Требуется роль администратора (`AuthorizeAdmins`).
- Заказ должен существовать и быть в допустимом статусе.

## Результат
- `200 OK`
- Тело: `UnfreezeReviewOrderResponse`
  - `ReviewOrder: ReviewOrderDto`

## На что влияет
- Обновляет флаг `IsFrozen` в БД.
- Публикует событие `ReviewOrderChangedEvent` с типом `OrderUnfrozen`.
