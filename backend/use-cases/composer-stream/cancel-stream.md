# Use-Case: Отменить стрим

## Эндпоинт
- Метод: `POST`
- Путь: `/api/ComposerStream/CancelStream`

## Что делает
Переводит стрим из `Planned` в `Canceled`.

## Входные данные
- Body: `CancelStreamRequest`
  - `ComposerStreamId` (`long`) - идентификатор стрима.

## Что можно
- Отменить стрим в статусе `Planned`.
- Повторно вызвать отмену для уже `Canceled` стрима (операция идемпотентна, вернется текущий стрим без ошибок).

## Что нельзя
- Отменить несуществующий стрим.
- Отменить стрим в статусе `Live` или `Completed`.

## Условия выполнения
- Требуется авторизация композитора (`AuthorizeComposer`).
- Должен существовать стрим с указанным `ComposerStreamId`.

## Результат
- `200 OK`
- Тело: `CancelStreamResponse`
  - `ComposerStream: ComposerStreamDto`
- После успешной отмены:
  - `Status = Canceled`

## На что влияет
- Обновляет состояние стрима в БД.
- Публикует событие `ComposerStreamChangedEvent` с типом `StreamCanceled`.

## Текущие ограничения реализации
- Есть TODO: пока не проверяется запрет отмены, если на стрим уже есть заказы.
