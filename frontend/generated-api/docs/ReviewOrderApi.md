# ReviewOrderApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiReviewOrderAddTrackUrlPost**](#apirevieworderaddtrackurlpost) | **POST** /api/ReviewOrder/AddTrackUrl | Добавляет или изменяет ссылку на трек|
|[**apiReviewOrderCancelReviewOrderPost**](#apireviewordercancelrevieworderpost) | **POST** /api/ReviewOrder/CancelReviewOrder | Отменяет заказ|
|[**apiReviewOrderCompleteReviewOrderPost**](#apireviewordercompleterevieworderpost) | **POST** /api/ReviewOrder/CompleteReviewOrder | Выполнение заказа|
|[**apiReviewOrderCreateReviewOrderPost**](#apireviewordercreaterevieworderpost) | **POST** /api/ReviewOrder/CreateReviewOrder | Создает заказ|
|[**apiReviewOrderFreezeReviewOrderPost**](#apirevieworderfreezerevieworderpost) | **POST** /api/ReviewOrder/FreezeReviewOrder | Замораживает заказ|
|[**apiReviewOrderTakeOrderInProgressPost**](#apireviewordertakeorderinprogresspost) | **POST** /api/ReviewOrder/TakeOrderInProgress | Взятие заказа в работу|
|[**apiReviewOrderUnfreezeReviewOrderPost**](#apirevieworderunfreezerevieworderpost) | **POST** /api/ReviewOrder/UnfreezeReviewOrder | Размораживает заказ|
|[**apiReviewOrderUpReviewOrderPost**](#apirevieworderuprevieworderpost) | **POST** /api/ReviewOrder/UpReviewOrder | Поднимает заказ в очереди|

# **apiReviewOrderAddTrackUrlPost**
> FarymaComposerApiFeaturesReviewOrderFeatureAddTrackUrlAddTrackUrlResponse apiReviewOrderAddTrackUrlPost()


### Example

```typescript
import {
    ReviewOrderApi,
    Configuration,
    FarymaComposerApiFeaturesReviewOrderFeatureAddTrackUrlAddTrackUrlRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ReviewOrderApi(configuration);

let farymaComposerApiFeaturesReviewOrderFeatureAddTrackUrlAddTrackUrlRequest: FarymaComposerApiFeaturesReviewOrderFeatureAddTrackUrlAddTrackUrlRequest; // (optional)

const { status, data } = await apiInstance.apiReviewOrderAddTrackUrlPost(
    farymaComposerApiFeaturesReviewOrderFeatureAddTrackUrlAddTrackUrlRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiFeaturesReviewOrderFeatureAddTrackUrlAddTrackUrlRequest** | **FarymaComposerApiFeaturesReviewOrderFeatureAddTrackUrlAddTrackUrlRequest**|  | |


### Return type

**FarymaComposerApiFeaturesReviewOrderFeatureAddTrackUrlAddTrackUrlResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiReviewOrderCancelReviewOrderPost**
> FarymaComposerApiFeaturesReviewOrderFeatureCancelCancelReviewOrderResponse apiReviewOrderCancelReviewOrderPost()


### Example

```typescript
import {
    ReviewOrderApi,
    Configuration,
    FarymaComposerApiFeaturesReviewOrderFeatureCancelCancelReviewOrderRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ReviewOrderApi(configuration);

let farymaComposerApiFeaturesReviewOrderFeatureCancelCancelReviewOrderRequest: FarymaComposerApiFeaturesReviewOrderFeatureCancelCancelReviewOrderRequest; // (optional)

const { status, data } = await apiInstance.apiReviewOrderCancelReviewOrderPost(
    farymaComposerApiFeaturesReviewOrderFeatureCancelCancelReviewOrderRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiFeaturesReviewOrderFeatureCancelCancelReviewOrderRequest** | **FarymaComposerApiFeaturesReviewOrderFeatureCancelCancelReviewOrderRequest**|  | |


### Return type

**FarymaComposerApiFeaturesReviewOrderFeatureCancelCancelReviewOrderResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiReviewOrderCompleteReviewOrderPost**
> FarymaComposerApiFeaturesReviewOrderFeatureCompleteCompleteReviewOrderResponse apiReviewOrderCompleteReviewOrderPost()


### Example

```typescript
import {
    ReviewOrderApi,
    Configuration,
    FarymaComposerApiFeaturesReviewOrderFeatureCompleteCompleteReviewOrderRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ReviewOrderApi(configuration);

let farymaComposerApiFeaturesReviewOrderFeatureCompleteCompleteReviewOrderRequest: FarymaComposerApiFeaturesReviewOrderFeatureCompleteCompleteReviewOrderRequest; // (optional)

const { status, data } = await apiInstance.apiReviewOrderCompleteReviewOrderPost(
    farymaComposerApiFeaturesReviewOrderFeatureCompleteCompleteReviewOrderRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiFeaturesReviewOrderFeatureCompleteCompleteReviewOrderRequest** | **FarymaComposerApiFeaturesReviewOrderFeatureCompleteCompleteReviewOrderRequest**|  | |


### Return type

**FarymaComposerApiFeaturesReviewOrderFeatureCompleteCompleteReviewOrderResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiReviewOrderCreateReviewOrderPost**
> FarymaComposerApiFeaturesReviewOrderFeatureCreateCreateReviewOrderResponse apiReviewOrderCreateReviewOrderPost()


### Example

```typescript
import {
    ReviewOrderApi,
    Configuration,
    FarymaComposerApiFeaturesReviewOrderFeatureCreateCreateReviewOrderRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ReviewOrderApi(configuration);

let idempotencyKey: string; //Ключ идемпотентности (optional) (default to undefined)
let farymaComposerApiFeaturesReviewOrderFeatureCreateCreateReviewOrderRequest: FarymaComposerApiFeaturesReviewOrderFeatureCreateCreateReviewOrderRequest; //Запрос создания заказа (optional)

const { status, data } = await apiInstance.apiReviewOrderCreateReviewOrderPost(
    idempotencyKey,
    farymaComposerApiFeaturesReviewOrderFeatureCreateCreateReviewOrderRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiFeaturesReviewOrderFeatureCreateCreateReviewOrderRequest** | **FarymaComposerApiFeaturesReviewOrderFeatureCreateCreateReviewOrderRequest**| Запрос создания заказа | |
| **idempotencyKey** | [**string**] | Ключ идемпотентности | (optional) defaults to undefined|


### Return type

**FarymaComposerApiFeaturesReviewOrderFeatureCreateCreateReviewOrderResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiReviewOrderFreezeReviewOrderPost**
> FarymaComposerApiFeaturesReviewOrderFeatureFreezeFreezeReviewOrderResponse apiReviewOrderFreezeReviewOrderPost()


### Example

```typescript
import {
    ReviewOrderApi,
    Configuration,
    FarymaComposerApiFeaturesReviewOrderFeatureFreezeFreezeReviewOrderRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ReviewOrderApi(configuration);

let farymaComposerApiFeaturesReviewOrderFeatureFreezeFreezeReviewOrderRequest: FarymaComposerApiFeaturesReviewOrderFeatureFreezeFreezeReviewOrderRequest; // (optional)

const { status, data } = await apiInstance.apiReviewOrderFreezeReviewOrderPost(
    farymaComposerApiFeaturesReviewOrderFeatureFreezeFreezeReviewOrderRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiFeaturesReviewOrderFeatureFreezeFreezeReviewOrderRequest** | **FarymaComposerApiFeaturesReviewOrderFeatureFreezeFreezeReviewOrderRequest**|  | |


### Return type

**FarymaComposerApiFeaturesReviewOrderFeatureFreezeFreezeReviewOrderResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiReviewOrderTakeOrderInProgressPost**
> FarymaComposerApiFeaturesReviewOrderFeatureTakeInProgressTakeOrderInProgressResponse apiReviewOrderTakeOrderInProgressPost()


### Example

```typescript
import {
    ReviewOrderApi,
    Configuration,
    FarymaComposerApiFeaturesReviewOrderFeatureTakeInProgressTakeOrderInProgressRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ReviewOrderApi(configuration);

let farymaComposerApiFeaturesReviewOrderFeatureTakeInProgressTakeOrderInProgressRequest: FarymaComposerApiFeaturesReviewOrderFeatureTakeInProgressTakeOrderInProgressRequest; // (optional)

const { status, data } = await apiInstance.apiReviewOrderTakeOrderInProgressPost(
    farymaComposerApiFeaturesReviewOrderFeatureTakeInProgressTakeOrderInProgressRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiFeaturesReviewOrderFeatureTakeInProgressTakeOrderInProgressRequest** | **FarymaComposerApiFeaturesReviewOrderFeatureTakeInProgressTakeOrderInProgressRequest**|  | |


### Return type

**FarymaComposerApiFeaturesReviewOrderFeatureTakeInProgressTakeOrderInProgressResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiReviewOrderUnfreezeReviewOrderPost**
> FarymaComposerApiFeaturesReviewOrderFeatureUnfreezeUnfreezeReviewOrderResponse apiReviewOrderUnfreezeReviewOrderPost()


### Example

```typescript
import {
    ReviewOrderApi,
    Configuration,
    FarymaComposerApiFeaturesReviewOrderFeatureUnfreezeUnfreezeReviewOrderRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ReviewOrderApi(configuration);

let farymaComposerApiFeaturesReviewOrderFeatureUnfreezeUnfreezeReviewOrderRequest: FarymaComposerApiFeaturesReviewOrderFeatureUnfreezeUnfreezeReviewOrderRequest; // (optional)

const { status, data } = await apiInstance.apiReviewOrderUnfreezeReviewOrderPost(
    farymaComposerApiFeaturesReviewOrderFeatureUnfreezeUnfreezeReviewOrderRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiFeaturesReviewOrderFeatureUnfreezeUnfreezeReviewOrderRequest** | **FarymaComposerApiFeaturesReviewOrderFeatureUnfreezeUnfreezeReviewOrderRequest**|  | |


### Return type

**FarymaComposerApiFeaturesReviewOrderFeatureUnfreezeUnfreezeReviewOrderResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiReviewOrderUpReviewOrderPost**
> FarymaComposerApiFeaturesReviewOrderFeatureUpUpReviewOrderResponse apiReviewOrderUpReviewOrderPost()


### Example

```typescript
import {
    ReviewOrderApi,
    Configuration,
    FarymaComposerApiFeaturesReviewOrderFeatureUpUpReviewOrderRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ReviewOrderApi(configuration);

let idempotencyKey: string; //Ключ идемпотентности (optional) (default to undefined)
let farymaComposerApiFeaturesReviewOrderFeatureUpUpReviewOrderRequest: FarymaComposerApiFeaturesReviewOrderFeatureUpUpReviewOrderRequest; //Запрос поднятия заказа в очереди (optional)

const { status, data } = await apiInstance.apiReviewOrderUpReviewOrderPost(
    idempotencyKey,
    farymaComposerApiFeaturesReviewOrderFeatureUpUpReviewOrderRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiFeaturesReviewOrderFeatureUpUpReviewOrderRequest** | **FarymaComposerApiFeaturesReviewOrderFeatureUpUpReviewOrderRequest**| Запрос поднятия заказа в очереди | |
| **idempotencyKey** | [**string**] | Ключ идемпотентности | (optional) defaults to undefined|


### Return type

**FarymaComposerApiFeaturesReviewOrderFeatureUpUpReviewOrderResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

