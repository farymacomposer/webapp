# OrderQueueApi

All URIs are relative to *http://localhost*

|Method | HTTP request | Description|
|------------- | ------------- | -------------|
|[**apiOrderQueueGetOrderQueuePost**](#apiorderqueuegetorderqueuepost) | **POST** /api/OrderQueue/GetOrderQueue | |

# **apiOrderQueueGetOrderQueuePost**
> object apiOrderQueueGetOrderQueuePost()


### Example

```typescript
import {
    OrderQueueApi,
    Configuration
} from './api';

const configuration = new Configuration();
const apiInstance = new OrderQueueApi(configuration);

const { status, data } = await apiInstance.apiOrderQueueGetOrderQueuePost();
```

### Parameters
This endpoint does not have any parameters.


### Return type

**object**

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

