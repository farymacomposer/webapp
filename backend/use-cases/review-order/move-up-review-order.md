# Use-Case: Поднять заказ в очереди

## Эндпоинт
- Метод: `POST`
- Путь: `/api/ReviewOrder/MoveUpReviewOrder`

## Что делает
Добавляет платеж к существующему заказу, чтобы повысить его приоритет в очереди.

## Входные данные
- Заголовок: `Idempotency-Key` (`Guid`), обязателен.
- Body: `MoveUpReviewOrderRequest`
  - `ReviewOrderId`
  - `Nickname` (1..40 символов)
  - `PaymentAmount` (> 0)
  - `TopUpProvider` (`Donationalerts`, `Donatty`, `TwitchChannelPoints`, `Manual`)

## Что можно
- Поднять заказ в статусе `Preorder` или `Pending`.
- Выполнить операцию с созданием пополнения и платежной транзакции.

## Что нельзя
- Поднимать несуществующий заказ.
- Поднимать заказ в статусах `InProgress`, `Completed`, `Canceled`.
- Передавать сумму `<= 0` или неподдерживаемый провайдер пополнения.

## Условия выполнения
- Требуется роль администратора (`AuthorizeAdmins`).
- Работает идемпотентность через фильтр `[Idempotent]` в рамках пользователя, маршрута и `Idempotency-Key`.
- Пользователь из токена должен существовать.

## Результат
- `200 OK`
- Тело: `MoveUpReviewOrderResponse`
  - `ReviewOrder: ReviewOrderDto`
  - `PaymentTransactionId: long`

## На что влияет
- Создает финансовые транзакции пополнения и платежа.
- Меняет позицию заказа в очереди (через бизнес-логику очереди).
- Публикует событие `ReviewOrderChangedEvent` с типом `OrderMovedUp`.
