# Use-Case: Завершить стрим

## Эндпоинт
- Метод: `POST`
- Путь: `/api/ComposerStream/CompleteStream`

## Что делает
Переводит стрим из `Live` в `Completed` и фиксирует время завершения (`CompletedAt = UtcNow`).

## Входные данные
- Body: `CompleteStreamRequest`
  - `ComposerStreamId` (`long`) - идентификатор стрима.

## Что можно
- Завершить стрим в статусе `Live`.
- Повторно вызвать завершение для уже `Completed` стрима (операция идемпотентна, вернется текущий стрим без ошибок).

## Что нельзя
- Завершить несуществующий стрим.
- Завершить стрим в статусе `Planned` или `Canceled`.
- Завершить стрим, пока есть заказ в работе (`ReviewOrder` в статусе in progress).

## Условия выполнения
- Требуется авторизация композитора (`AuthorizeComposer`).
- Должен существовать стрим с указанным `ComposerStreamId`.
- Не должно быть активного заказа "в работе".

## Результат
- `200 OK`
- Тело: `CompleteStreamResponse`
  - `ComposerStream: ComposerStreamDto`
- После успешного завершения:
  - `Status = Completed`
  - `CompletedAt` заполнено текущим UTC-временем

## На что влияет
- Обновляет состояние стрима в БД.
- Публикует событие `ComposerStreamChangedEvent` с типом `StreamCompleted`.
