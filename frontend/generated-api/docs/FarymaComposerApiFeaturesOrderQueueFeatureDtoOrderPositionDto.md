# FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto

Представляет позицию заказа в очереди, включая сам заказ и историю перемещений

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**order** | [**FarymaComposerApiFeaturesCommonDtoReviewOrderDto**](FarymaComposerApiFeaturesCommonDtoReviewOrderDto.md) | Заказ разбора трека | [default to undefined]
**previousPosition** | [**FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderQueuePositionDto**](FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderQueuePositionDto.md) | Предыдущая позиция заказа в очереди | [default to undefined]
**currentPosition** | [**FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderQueuePositionDto**](FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderQueuePositionDto.md) | Текущая позиция заказа в очереди | [default to undefined]

## Example

```typescript
import { FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto } from './api';

const instance: FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto = {
    order,
    previousPosition,
    currentPosition,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
