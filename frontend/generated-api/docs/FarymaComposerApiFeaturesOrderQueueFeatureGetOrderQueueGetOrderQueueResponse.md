# FarymaComposerApiFeaturesOrderQueueFeatureGetOrderQueueGetOrderQueueResponse

Ответ на запрос получения очереди заказов

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**syncVersion** | **number** | Версия для синхронизации состояния очереди | [default to undefined]
**activeOrders** | [**Array&lt;FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto&gt;**](FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto.md) | Активные заказы | [readonly] [default to undefined]
**inProgressOrder** | [**FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto**](FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto.md) | Заказ в работе | [optional] [default to undefined]
**completedOrders** | [**Array&lt;FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto&gt;**](FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto.md) | Выполненные заказы | [readonly] [default to undefined]
**scheduledOrders** | [**Array&lt;FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto&gt;**](FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto.md) | Запланированные заказы | [readonly] [default to undefined]
**frozenOrders** | [**Array&lt;FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto&gt;**](FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto.md) | Замороженные заказы | [readonly] [default to undefined]

## Example

```typescript
import { FarymaComposerApiFeaturesOrderQueueFeatureGetOrderQueueGetOrderQueueResponse } from './api';

const instance: FarymaComposerApiFeaturesOrderQueueFeatureGetOrderQueueGetOrderQueueResponse = {
    syncVersion,
    activeOrders,
    inProgressOrder,
    completedOrders,
    scheduledOrders,
    frozenOrders,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
