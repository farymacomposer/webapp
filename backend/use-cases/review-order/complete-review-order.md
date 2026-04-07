# Use-Case: Выполнить заказ

## Эндпоинт
- Метод: `POST`
- Путь: `/api/ReviewOrder/CompleteReviewOrder`

## Что делает
Завершает заказ, создает сущность разбора (`Review`), записывает рейтинг и время завершения.

## Входные данные
- Body: `CompleteReviewOrderRequest`
  - `ReviewOrderId`
  - `Rating` (от 0 до 26)

## Что можно
- Выполнить заказ в статусе `InProgress`.
- Повторно вызвать для уже `Completed` заказа (идемпотентно, вернется текущий заказ).

## Что нельзя
- Выполнить несуществующий заказ.
- Выполнить заказ в `Preorder`, `Pending` или `Canceled`.
- Передавать рейтинг вне диапазона `0..26`.

## Условия выполнения
- Требуется роль администратора (`AuthorizeAdmins`).
- Пользователь из токена должен существовать.
- Заказ должен быть взят в работу.

## Результат
- `200 OK`
- Тело: `CompleteReviewOrderResponse`
  - `ReviewOrder: ReviewOrderDto`
  - `ReviewId: long`

## На что влияет
- Обновляет заказ (`Status = Completed`, `CompletedAt`).
- Создает запись разбора в БД.
- Публикует событие `ReviewOrderChangedEvent` с типом `OrderCompleted`.
