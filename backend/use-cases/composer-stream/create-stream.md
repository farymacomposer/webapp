# Use-Case: Создать стрим

## Эндпоинт
- Метод: `POST`
- Путь: `/api/ComposerStream/CreateStream`

## Что делает
Создает новый стрим в статусе `Planned` с заданной датой и типом.

## Входные данные
- Body: `CreateStreamRequest`
  - `EventDate` (`DateOnly`) - дата стрима.
  - `Type` (`ComposerStreamType`) - тип стрима (`Donation`, `Debt`, `Charity`).

## Что можно
- Создать стрим на сегодня или будущую дату.
- Создать стрим только с валидным типом (не `Unspecified`).

## Что нельзя
- Создать стрим на прошедшую дату.
- Создать стрим с типом `Unspecified`.
- Создать второй стрим на ту же дату (ограничение уникальности в БД).

## Условия выполнения
- Требуется авторизация композитора (`AuthorizeComposer`).
- Пользователь из токена должен существовать в системе.

## Результат
- `200 OK`
- Тело: `CreateStreamResponse`
  - `ComposerStream: ComposerStreamDto`
- Новый стрим имеет:
  - `Status = Planned`
  - `StartedAt = null`
  - `CompletedAt = null`

## На что влияет
- Создает запись стрима в БД.
- Публикует событие `ComposerStreamChangedEvent` с типом `StreamCreated`.
