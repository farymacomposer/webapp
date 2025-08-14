# ComposerStreamApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiComposerStreamCreateComposerStreamPost**](#apicomposerstreamcreatecomposerstreampost) | **POST** /api/ComposerStream/CreateComposerStream | Создает стрим|
|[**apiComposerStreamFindComposerStreamGet**](#apicomposerstreamfindcomposerstreamget) | **GET** /api/ComposerStream/FindComposerStream | Возвращает список стримов|

# **apiComposerStreamCreateComposerStreamPost**
> FarymaComposerApiFeaturesComposerStreamFeatureCreateCreateComposerStreamResponse apiComposerStreamCreateComposerStreamPost()


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

const { status, data } = await apiInstance.apiComposerStreamCreateComposerStreamPost(
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

# **apiComposerStreamFindComposerStreamGet**
> FarymaComposerApiFeaturesComposerStreamFeatureFindFindComposerStreamResponse apiComposerStreamFindComposerStreamGet()


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

const { status, data } = await apiInstance.apiComposerStreamFindComposerStreamGet(
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

