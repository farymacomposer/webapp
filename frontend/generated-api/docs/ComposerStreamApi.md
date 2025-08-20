# ComposerStreamApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiComposerStreamCancelStreamPost**](#apicomposerstreamcancelstreampost) | **POST** /api/ComposerStream/CancelStream | Отменяет стрим|
|[**apiComposerStreamCompleteStreamPost**](#apicomposerstreamcompletestreampost) | **POST** /api/ComposerStream/CompleteStream | Завершает стрим|
|[**apiComposerStreamCreateStreamPost**](#apicomposerstreamcreatestreampost) | **POST** /api/ComposerStream/CreateStream | Создает стрим|
|[**apiComposerStreamFindCurrentAndScheduledStreamsGet**](#apicomposerstreamfindcurrentandscheduledstreamsget) | **GET** /api/ComposerStream/FindCurrentAndScheduledStreams | Возвращает текущий и запланированные стримы|
|[**apiComposerStreamFindStreamsGet**](#apicomposerstreamfindstreamsget) | **GET** /api/ComposerStream/FindStreams | Возвращает список стримов|
|[**apiComposerStreamStartStreamPost**](#apicomposerstreamstartstreampost) | **POST** /api/ComposerStream/StartStream | Запускает стрим|

# **apiComposerStreamCancelStreamPost**
> FarymaComposerApiFeaturesComposerStreamFeatureCancelCancelStreamResponse apiComposerStreamCancelStreamPost()


### Example

```typescript
import {
    ComposerStreamApi,
    Configuration,
    FarymaComposerApiFeaturesComposerStreamFeatureCancelCancelStreamRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ComposerStreamApi(configuration);

let farymaComposerApiFeaturesComposerStreamFeatureCancelCancelStreamRequest: FarymaComposerApiFeaturesComposerStreamFeatureCancelCancelStreamRequest; // (optional)

const { status, data } = await apiInstance.apiComposerStreamCancelStreamPost(
    farymaComposerApiFeaturesComposerStreamFeatureCancelCancelStreamRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiFeaturesComposerStreamFeatureCancelCancelStreamRequest** | **FarymaComposerApiFeaturesComposerStreamFeatureCancelCancelStreamRequest**|  | |


### Return type

**FarymaComposerApiFeaturesComposerStreamFeatureCancelCancelStreamResponse**

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

# **apiComposerStreamCompleteStreamPost**
> FarymaComposerApiFeaturesComposerStreamFeatureCompleteCompleteStreamResponse apiComposerStreamCompleteStreamPost()


### Example

```typescript
import {
    ComposerStreamApi,
    Configuration,
    FarymaComposerApiFeaturesComposerStreamFeatureCompleteCompleteStreamRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ComposerStreamApi(configuration);

let farymaComposerApiFeaturesComposerStreamFeatureCompleteCompleteStreamRequest: FarymaComposerApiFeaturesComposerStreamFeatureCompleteCompleteStreamRequest; // (optional)

const { status, data } = await apiInstance.apiComposerStreamCompleteStreamPost(
    farymaComposerApiFeaturesComposerStreamFeatureCompleteCompleteStreamRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiFeaturesComposerStreamFeatureCompleteCompleteStreamRequest** | **FarymaComposerApiFeaturesComposerStreamFeatureCompleteCompleteStreamRequest**|  | |


### Return type

**FarymaComposerApiFeaturesComposerStreamFeatureCompleteCompleteStreamResponse**

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

# **apiComposerStreamCreateStreamPost**
> FarymaComposerApiFeaturesComposerStreamFeatureCreateCreateComposerStreamResponse apiComposerStreamCreateStreamPost()


### Example

```typescript
import {
    ComposerStreamApi,
    Configuration,
    FarymaComposerApiFeaturesComposerStreamFeatureCreateCreateComposerStreamRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ComposerStreamApi(configuration);

let farymaComposerApiFeaturesComposerStreamFeatureCreateCreateComposerStreamRequest: FarymaComposerApiFeaturesComposerStreamFeatureCreateCreateComposerStreamRequest; // (optional)

const { status, data } = await apiInstance.apiComposerStreamCreateStreamPost(
    farymaComposerApiFeaturesComposerStreamFeatureCreateCreateComposerStreamRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiFeaturesComposerStreamFeatureCreateCreateComposerStreamRequest** | **FarymaComposerApiFeaturesComposerStreamFeatureCreateCreateComposerStreamRequest**|  | |


### Return type

**FarymaComposerApiFeaturesComposerStreamFeatureCreateCreateComposerStreamResponse**

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

# **apiComposerStreamFindCurrentAndScheduledStreamsGet**
> FarymaComposerApiFeaturesComposerStreamFeatureGetCurrentAndScheduledFindCurrentAndScheduledStreamsResponse apiComposerStreamFindCurrentAndScheduledStreamsGet()


### Example

```typescript
import {
    ComposerStreamApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ComposerStreamApi(configuration);

const { status, data } = await apiInstance.apiComposerStreamFindCurrentAndScheduledStreamsGet();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**FarymaComposerApiFeaturesComposerStreamFeatureGetCurrentAndScheduledFindCurrentAndScheduledStreamsResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiComposerStreamFindStreamsGet**
> FarymaComposerApiFeaturesComposerStreamFeatureFindFindComposerStreamResponse apiComposerStreamFindStreamsGet()


### Example

```typescript
import {
    ComposerStreamApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new ComposerStreamApi(configuration);

let dateFrom: string; //Начальная дата периода поиска (default to undefined)
let dateTo: string; //Конечная дата периода поиска (default to undefined)

const { status, data } = await apiInstance.apiComposerStreamFindStreamsGet(
    dateFrom,
    dateTo
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **dateFrom** | [**string**] | Начальная дата периода поиска | defaults to undefined|
| **dateTo** | [**string**] | Конечная дата периода поиска | defaults to undefined|


### Return type

**FarymaComposerApiFeaturesComposerStreamFeatureFindFindComposerStreamResponse**

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **apiComposerStreamStartStreamPost**
> FarymaComposerApiFeaturesComposerStreamFeatureStartStartStreamResponse apiComposerStreamStartStreamPost()


### Example

```typescript
import {
    ComposerStreamApi,
    Configuration,
    FarymaComposerApiFeaturesComposerStreamFeatureStartStartStreamRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new ComposerStreamApi(configuration);

let farymaComposerApiFeaturesComposerStreamFeatureStartStartStreamRequest: FarymaComposerApiFeaturesComposerStreamFeatureStartStartStreamRequest; // (optional)

const { status, data } = await apiInstance.apiComposerStreamStartStreamPost(
    farymaComposerApiFeaturesComposerStreamFeatureStartStartStreamRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiFeaturesComposerStreamFeatureStartStartStreamRequest** | **FarymaComposerApiFeaturesComposerStreamFeatureStartStartStreamRequest**|  | |


### Return type

**FarymaComposerApiFeaturesComposerStreamFeatureStartStartStreamResponse**

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

