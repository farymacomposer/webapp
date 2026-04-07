# Use-Case: Добавить или изменить ссылку на трек

## Эндпоинт
- Метод: `POST`
- Путь: `/api/ReviewOrder/AddTrackUrl`

## Что делает
Добавляет или обновляет `TrackUrl` в заказе.  
Если заказ был в статусе `Preorder`, переводит его в `Pending`.

## Входные данные
- Body: `AddTrackUrlRequest`
  - `ReviewOrderId`
  - `TrackUrl` (обязательный, валидный URL)

## Что можно
- Добавить/изменить ссылку для заказа в `Preorder`, `Pending` или `InProgress`.
- Перевести заказ из `Preorder` в `Pending` при первом добавлении ссылки.

## Что нельзя
- Изменять ссылку у несуществующего заказа.
- Изменять ссылку у заказа в `Completed` или `Canceled`.

## Условия выполнения
- Требуется роль администратора (`AuthorizeAdmins`).
- Заказ должен существовать.

## Результат
- `200 OK`
- Тело: `AddTrackUrlResponse`
  - `ReviewOrder: ReviewOrderDto`

## На что влияет
- Обновляет данные заказа в БД (`TrackUrl`, иногда `Status`).
- Публикует событие `ReviewOrderChangedEvent` с типом `TrackUrlAdded`.
