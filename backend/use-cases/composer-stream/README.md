# Composer Stream Use-Cases

Документы в этой папке описывают use cases для `ComposerStream` на границе `API -> application`.
Если текущая реализация расходится с желаемым доменным правилом, это отмечается отдельно как `Текущая реализация vs целевое поведение`.

## Сценарии

| Use case | Файл | Эндпоинт |
| --- | --- | --- |
| Создать стрим | `create-stream.md` | `POST /api/ComposerStream/CreateStream` |
| Запустить стрим | `start-stream.md` | `POST /api/ComposerStream/StartStream` |
| Завершить стрим | `complete-stream.md` | `POST /api/ComposerStream/CompleteStream` |
| Отменить стрим | `cancel-stream.md` | `POST /api/ComposerStream/CancelStream` |

## Общие замечания

- Read-only сценарии не меняют состояние системы и не публикуют события.
- Для сценариев, где используется `today` или текущее время, текущая реализация опирается на UTC через `DateTimeService`.
- Ошибки валидации запроса обрабатываются на уровне API, а бизнес-ошибки `ComposerStreamException` сейчас возвращаются как HTTP `666`.
