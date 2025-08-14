# AuthApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiAuthLoginPost**](#apiauthloginpost) | **POST** /api/Auth/Login | Выполняет аутентификацию пользователя и возвращает JWT токен|
|[**apiAuthRegisterPost**](#apiauthregisterpost) | **POST** /api/Auth/Register | Регистрирует нового пользователя в системе|

# **apiAuthLoginPost**
> FarymaComposerApiAuthLoginLoginResponse apiAuthLoginPost()


### Example

```typescript
import {
    AuthApi,
    Configuration,
    FarymaComposerApiAuthLoginLoginRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new AuthApi(configuration);

let farymaComposerApiAuthLoginLoginRequest: FarymaComposerApiAuthLoginLoginRequest; // (optional)

const { status, data } = await apiInstance.apiAuthLoginPost(
    farymaComposerApiAuthLoginLoginRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiAuthLoginLoginRequest** | **FarymaComposerApiAuthLoginLoginRequest**|  | |


### Return type

**FarymaComposerApiAuthLoginLoginResponse**

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

# **apiAuthRegisterPost**
> apiAuthRegisterPost()


### Example

```typescript
import {
    AuthApi,
    Configuration,
    FarymaComposerApiAuthRegisterRegisterRequest
} from './api';

const configuration = new Configuration();
const apiInstance = new AuthApi(configuration);

let farymaComposerApiAuthRegisterRegisterRequest: FarymaComposerApiAuthRegisterRegisterRequest; // (optional)

const { status, data } = await apiInstance.apiAuthRegisterPost(
    farymaComposerApiAuthRegisterRegisterRequest
);
```

### Parameters

|Name | Type | Description  | Notes|
|------------- | ------------- | ------------- | -------------|
| **farymaComposerApiAuthRegisterRegisterRequest** | **FarymaComposerApiAuthRegisterRegisterRequest**|  | |


### Return type

void (empty response body)

### Authorization

[Bearer](../README.md#Bearer)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
|**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

