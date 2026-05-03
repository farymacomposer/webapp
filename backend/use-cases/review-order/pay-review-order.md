# Use-Case: Оплатить заказ

## Эндпоинт

- Метод: `POST`
- Путь: `/api/review-orders/pay`

## Что делает

Добавляет денежный платеж к существующему заказу. Платеж может закрыть недостающее обязательное покрытие, включая обязательную доплату за длительность трека сверх пяти минут, или быть переплатой, которая повышает донатный приоритет.

## Слои

- API:
  - требует роль администратора (`AuthorizeAdmins`);
  - требует заголовок `Idempotency-Key`;
  - принимает `PayReviewOrderRequest`;
  - валидирует `ReviewOrderId`, `Nickname`, `PaymentAmount` и `TopUpProvider`.
- Application (`ReviewOrderService`):
  - находит пользователя из токена;
  - загружает заказ;
  - допускает операцию для `Preorder`, `Pending` и `AwaitingPayment`;
  - создает пополнение и платеж на сам `ReviewOrder`;
  - пересчитывает checkout-статус по текущему покрытию обязательной стоимости;
  - сохраняет изменения и публикует событие.

## Входные данные

- Заголовок: `Idempotency-Key` (`Guid`), обязателен.
- Body: `PayReviewOrderRequest`
  - `ReviewOrderId` - больше `0`.
  - `Nickname` - ник плательщика, от 1 до 40 символов.
  - `PaymentAmount` - строго больше `0`.
  - `TopUpProvider` - один из `Donationalerts`, `Donatty`, `TwitchChannelPoints`, `Manual`.

## Предусловия

- Пользователь должен быть авторизован как администратор.
- Пользователь из токена должен существовать.
- Заказ должен существовать и быть в статусе `Preorder`, `Pending` или `AwaitingPayment`.

## Что можно

- Оплатить заказ частями.
- Оплатить заказ от ника, отличного от автора заказа.
- Перевести `AwaitingPayment -> Pending`, если после платежа обязательная стоимость покрыта.
- Оставить `Preorder` в статусе `Preorder`, если трек еще не указан.
- Добавить переплату к уже покрытому заказу; она остается денежным платежом и влияет на донатный приоритет.
- Закрыть обязательную доплату за длительность через платеж на сам `ReviewOrder`, без отдельной платной услуги.

## Что нельзя

- Оплатить несуществующий заказ.
- Оплатить заказ в статусах `InProgress`, `Completed` или `Canceled`.
- Передавать сумму `<= 0`.
- Передавать неподдерживаемый провайдер пополнения.

## Результат и постусловия

- Успешный ответ: `200 OK`.
- Тело: `PayReviewOrderResponse`.
- Клиент получает:
  - `ReviewOrder: ReviewOrderDto`;
  - `PaymentTransactionId: long`.
- Создается пара транзакций `AccountTopUp` и `Payment` на аккаунте ника плательщика.
- `ReviewOrder.MainNickname` и `ReviewOrder.UserNicknames` не меняются из-за платежа другого ника.
- `ReviewOrderDto` возвращает checkout-суммы: `RequiredAmount`, `CoveredAmount`, `PaidAmount`, `PaidPriorityAmount`; legacy `TotalAmount` совпадает с `PaidPriorityAmount`.

## События и идемпотентность

- Идемпотентность обеспечивается фильтром `[Idempotent]` в рамках пользователя, маршрута и `Idempotency-Key`.
- При успешном вызове публикуется `ReviewOrderChangedEvent`:
  - `UpdateType = OrderMovedUp`;
  - `PreviousStatus =` исходный статус заказа.

## Ошибки

- Некорректный запрос не проходит API-валидацию:
  - `ReviewOrderId <= 0`;
  - сумма `<= 0`;
  - неподдерживаемый `TopUpProvider`;
  - невалидный nickname.
- Если пользователь не найден или заказ находится в недопустимом статусе, сервис выбрасывает `ReviewOrderException`, и API сейчас возвращает HTTP `666`.
