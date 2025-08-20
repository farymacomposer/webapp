# AppSettingsApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiAppSettingsGetAppSettingsGet**](#apiappsettingsgetappsettingsget) | **GET** /api/AppSettings/GetAppSettings | Возвращает текущие настройки|
|[**apiAppSettingsUpdateAppSettingsPost**](#apiappsettingsupdateappsettingspost) | **POST** /api/AppSettings/UpdateAppSettings | Обновляет настройки|

# **apiAppSettingsGetAppSettingsGet**
> FarymaComposerApiFeaturesAppSettingsFeatureAppSettingsDto apiAppSettingsGetAppSettingsGet()


### Example

```typescript
import {
    AppSettingsApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new AppSettingsApi(configuration);

const { status, data } = await apiInstance.apiAppSettingsGetAppSettingsGet();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**FarymaComposerApiFeaturesAppSettingsFeatureAppSettingsDto**

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

# **apiAppSettingsUpdateAppSettingsPost**
> FarymaComposerApiFeaturesAppSettingsFeatureAppSettingsDto apiAppSettingsUpdateAppSettingsPost()


### Example

```typescript
import {
    AppSettingsApi,
    Configuration,
    FarymaComposerApiFeaturesAppSettingsFeatureAppSettingsDto
} from './api';

const configuration = new Configuration();
const apiInstance = new AppSettingsApi(configuration);

let farymaComposerApiFeaturesAppSettingsFeatureAppSettingsDto: FarymaComposerApiFeaturesAppSettingsFeatureAppSettingsDto; // (optional)

const { status, data } = await apiInstance.apiAppSettingsUpdateAppSettingsPost(
    farymaComposerApiFeaturesAppSettingsFeatureAppSettingsDto
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiFeaturesAppSettingsFeatureAppSettingsDto** | **FarymaComposerApiFeaturesAppSettingsFeatureAppSettingsDto**|  | |


### Return type

**FarymaComposerApiFeaturesAppSettingsFeatureAppSettingsDto**

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

