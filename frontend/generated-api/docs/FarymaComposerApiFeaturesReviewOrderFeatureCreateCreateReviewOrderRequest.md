# FarymaComposerApiFeaturesReviewOrderFeatureCreateCreateReviewOrderRequest

Запрос создания заказа на разбор

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**nickname** | **string** | Псевдоним пользователя | [default to undefined]
**orderType** | [**FarymaComposerInfrastructureEnumsReviewOrderType**](FarymaComposerInfrastructureEnumsReviewOrderType.md) | Тип заказа разбора трека | [default to undefined]
**trackUrl** | **string** | Ссылка на трек | [optional] [default to undefined]
**paymentAmount** | **number** | Сумма платежа | [optional] [default to undefined]
**userComment** | **string** | Комментарий пользователя | [optional] [default to undefined]

## Example

```typescript
import { FarymaComposerApiFeaturesReviewOrderFeatureCreateCreateReviewOrderRequest } from './api';

const instance: FarymaComposerApiFeaturesReviewOrderFeatureCreateCreateReviewOrderRequest = {
    nickname,
    orderType,
    trackUrl,
    paymentAmount,
    userComment,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
