# Use-Case: Отменить заказ

## Эндпоинт
- Метод: `POST`
- Путь: `/api/ReviewOrder/CancelReviewOrder`

## Что делает
Переводит заказ в `Canceled`, снимает его с обработки и очищает поля, связанные с активной обработкой.

## Входные данные
- Body: `CancelReviewOrderRequest`
  - `ReviewOrderId`

## Что можно
- Отменить заказ в `Preorder`, `Pending` или `InProgress`.
- Повторно вызвать для уже `Canceled` заказа (идемпотентно, вернется текущий заказ).

## Что нельзя
- Отменить несуществующий заказ.
- Отменить заказ в `Completed`.

## Условия выполнения
- Требуется роль администратора (`AuthorizeAdmins`).
- Заказ должен существовать.

## Результат
- `200 OK`
- Тело: `CancelReviewOrderResponse`
  - `ReviewOrder: ReviewOrderDto`
- После отмены:
  - `Status = Canceled`
  - `CategoryType = Unspecified`
  - `ProcessingStream = null`
  - `InProgressAt = null`

## На что влияет
- Обновляет состояние заказа в БД.
- Публикует событие `ReviewOrderChangedEvent` с типом `OrderCanceled`.
