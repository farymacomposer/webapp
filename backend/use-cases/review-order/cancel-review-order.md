# Use-Case: Отменить заказ

## Эндпоинт

- Метод: `POST`
- Путь: `/api/review-orders/cancel`

## Что делает

Переводит заказ в `Canceled`, снимает его с обработки и очищает внутренние поля, связанные с активной обработкой.

## Слои

- API:
  - требует роль администратора (`AuthorizeAdmins`);
  - принимает `CancelReviewOrderRequest`;
  - требует `CancelReason`.
- Application (`ReviewOrderService`):
  - загружает заказ;
  - допускает отмену только для `Preorder`, `Pending`, `AwaitingPayment` и `InProgress`;
  - выставляет `CanceledAt`, `CancelReason`, `QueueCategory = Unspecified`, `ProcessingStream = null`, `Status = Canceled`, `InProgressAt = null`;
  - сохраняет изменения и публикует событие.

## Входные данные

- Body: `CancelReviewOrderRequest`
  - `ReviewOrderId`.
  - `CancelReason` - обязательная причина отмены.

## Предусловия

- Пользователь должен быть авторизован как администратор.
- Заказ должен существовать.

## Что можно

- Отменить заказ в `Preorder`, `Pending`, `AwaitingPayment` или `InProgress`.
- Повторно вызвать сценарий для уже `Canceled` заказа без побочных эффектов.

## Что нельзя

- Отменить несуществующий заказ.
- Отменить заказ в `Completed`.

## Результат и постусловия

- Успешный ответ: `200 OK`.
- Тело: `CancelReviewOrderResponse`.
- Клиент получает `ReviewOrderDto`.
- Публичный API-контракт не возвращает `QueueCategory`, `ProcessingStream`, `CanceledAt` или `CancelReason`.
- Внутренние postconditions application-слоя после первого успешного вызова:
  - `Status = Canceled`;
  - `QueueCategory = Unspecified`;
  - `ProcessingStream = null`;
  - `InProgressAt = null`;
  - `CanceledAt` заполнено;
  - `CancelReason` сохранена.

## События и идемпотентность

- Повторный вызов для уже `Canceled` заказа идемпотентен:
  - persisted-состояние не меняется;
  - причина отмены не обновляется;
  - событие повторно не публикуется.
- При первом успешном вызове публикуется `ReviewOrderChangedEvent`:
  - `UpdateType = OrderCanceled`;
  - `PreviousStatus =` исходный статус заказа.

## Ошибки

- Некорректный HTTP-запрос не проходит API-валидацию, если `CancelReason` отсутствует.
- Если заказ не найден или находится в недопустимом статусе, сервис выбрасывает `ReviewOrderException`, и API сейчас возвращает HTTP `666`.

## Текущая реализация vs целевое поведение

- Публичный ответ этого сценария ограничен полями `ReviewOrderDto`; внутренние изменения processing/queue-полей не должны описываться как поля HTTP-ответа.
- Внутреннее поле называется `QueueCategory`, а не `CategoryType`.
