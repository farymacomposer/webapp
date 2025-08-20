# FarymaComposerApiFeaturesCommonDtoReviewOrderDto

Заказ разбора трека

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | **number** | Id заказа | [default to undefined]
**createdAt** | **string** | Дата и время создания заказа | [default to undefined]
**inProgressAt** | **string** | Дата и время взятия заказа в работу | [default to undefined]
**completedAt** | **string** | Дата и время выполнения заказа | [default to undefined]
**type** | [**FarymaComposerInfrastructureEnumsReviewOrderType**](FarymaComposerInfrastructureEnumsReviewOrderType.md) | Тип заказа разбора трека | [default to undefined]
**categoryType** | [**FarymaComposerInfrastructureEnumsOrderCategoryType**](FarymaComposerInfrastructureEnumsOrderCategoryType.md) | Тип категории заказа | [default to undefined]
**status** | [**FarymaComposerInfrastructureEnumsReviewOrderStatus**](FarymaComposerInfrastructureEnumsReviewOrderStatus.md) | Статус заказа разбора трека | [default to undefined]
**isFrozen** | **boolean** | Заказ заморожен | [default to undefined]
**trackUrl** | **string** | Ссылка на трек | [default to undefined]
**userComment** | **string** | Комментарий пользователя | [default to undefined]
**mainNickname** | **string** | Основной ник пользователя, из всех пользователей, кто причастен к созданию заказа | [default to undefined]
**totalAmount** | **number** | Общая стоимость заказа (номинал + платежи) | [default to undefined]
**creationStream** | [**FarymaComposerApiFeaturesCommonDtoComposerStreamDto**](FarymaComposerApiFeaturesCommonDtoComposerStreamDto.md) | Связанный cтрим композитора, где создан заказ | [default to undefined]

## Example

```typescript
import { FarymaComposerApiFeaturesCommonDtoReviewOrderDto } from './api';

const instance: FarymaComposerApiFeaturesCommonDtoReviewOrderDto = {
    id,
    createdAt,
    inProgressAt,
    completedAt,
    type,
    categoryType,
    status,
    isFrozen,
    trackUrl,
    userComment,
    mainNickname,
    totalAmount,
    creationStream,
};
```

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)
