# FarymaComposerApiFeaturesCommonDtoComposerStreamDto

Стрим композитора

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | **number** | Id стрима | [default to undefined]
**eventDate** | **string** | Дата проведения стрима | [default to undefined]
**status** | [**FarymaComposerInfrastructureEnumsComposerStreamStatus**](FarymaComposerInfrastructureEnumsComposerStreamStatus.md) | Статус стрима | [default to undefined]
**type** | [**FarymaComposerInfrastructureEnumsComposerStreamType**](FarymaComposerInfrastructureEnumsComposerStreamType.md) | Тип стрима | [default to undefined]
**wentLiveAt** | **string** | Дата и время начала стрима | [default to undefined]
**completedAt** | **string** | Дата и время завершения стрима | [default to undefined]

## Example

```typescript
import { FarymaComposerApiFeaturesCommonDtoComposerStreamDto } from './api';

const instance: FarymaComposerApiFeaturesCommonDtoComposerStreamDto = {
    id,
    eventDate,
    status,
    type,
    wentLiveAt,
    completedAt,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
