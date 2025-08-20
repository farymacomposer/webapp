# FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderQueuePositionDto

Позиция заказа в очереди, включая его индекс, статус активности и категорию

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**queueIndex** | **number** | Позиция заказа в очереди | [default to undefined]
**activityStatus** | [**FarymaComposerCoreFeaturesOrderQueueFeatureEnumsOrderActivityStatus**](FarymaComposerCoreFeaturesOrderQueueFeatureEnumsOrderActivityStatus.md) | Статус активности заказа | [default to undefined]
**categoryType** | [**FarymaComposerInfrastructureEnumsOrderCategoryType**](FarymaComposerInfrastructureEnumsOrderCategoryType.md) | Тип категории заказа | [default to undefined]
**categoryDebtNumber** | **number** | Номер категории, если заказ относится к долговой категории | [default to undefined]

## Example

```typescript
import { FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderQueuePositionDto } from './api';

const instance: FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderQueuePositionDto = {
    queueIndex,
    activityStatus,
    categoryType,
    categoryDebtNumber,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
