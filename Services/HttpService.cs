using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace swpumc.Services
{
    public class HttpService : IHttpService
    {
        public HttpService()
        {
        }

        public IHttpRequestBuilder CreateRequest()
        {
            return new HttpRequestBuilder();
        }
    }

    public class HttpRequestBuilder : IHttpRequestBuilder
    {
        private string _baseUrl = string.Empty;
        private string _api = string.Empty;
        private HttpMethod _method = HttpMethod.GET;
        private object? _data;
        private Dictionary<string, string> _headers = new Dictionary<string, string>();
        private Dictionary<string, string> _queryParams = new Dictionary<string, string>();
        private TimeSpan _timeout = TimeSpan.FromSeconds(30);
        private string _contentType = "application/json";

        public HttpRequestBuilder()
        {
        }

        public IHttpRequestBuilder SetBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            return this;
        }

        public IHttpRequestBuilder SetApi(string api)
        {
            _api = api.StartsWith('/') ? api : $"/{api}";
            return this;
        }

        public IHttpRequestBuilder SetMethod(HttpMethod method)
        {
            _method = method;
            return this;
        }

        public IHttpRequestBuilder SetData<T>(T data)
        {
            _data = data;
            return this;
        }

        public IHttpRequestBuilder SetHeaders(Dictionary<string, string> headers)
        {
            _headers = headers ?? new Dictionary<string, string>();
            return this;
        }

        public IHttpRequestBuilder SetHeader(string key, string value)
        {
            _headers[key] = value;
            return this;
        }

        public IHttpRequestBuilder SetQueryParam(string key, string value)
        {
            _queryParams[key] = value;
            return this;
        }

        public IHttpRequestBuilder SetTimeout(TimeSpan timeout)
        {
            _timeout = timeout;
            return this;
        }

        public IHttpRequestBuilder SetContentType(string contentType)
        {
            _contentType = contentType;
            return this;
        }

        public async Task<HttpResponse<T>> ExecuteAsync<T>()
        {
            try
            {
                var response = await ExecuteRequestAsync();
                var result = new HttpResponse<T>
                {
                    IsSuccess = response.IsSuccess,
                    StatusCode = response.StatusCode,
                    Headers = response.Headers,
                    ErrorMessage = response.ErrorMessage
                };

                if (response.IsSuccess && !string.IsNullOrEmpty(response.Content))
                {
                    try
                    {
                        result.Data = JsonSerializer.Deserialize<T>(response.Content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                    }
                    catch (JsonException ex)
                    {
                        result.IsSuccess = false;
                        result.ErrorMessage = $"JSON反序列化失败: {ex.Message}";
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new HttpResponse<T>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<HttpResponse> ExecuteAsync()
        {
            return await ExecuteRequestAsync();
        }

        public async Task<HttpResponse<byte[]>> ExecuteBytesAsync()
        {
            return await ExecuteRequestBytesAsync();
        }

        private async Task<HttpResponse> ExecuteRequestAsync()
        {
            try
            {
                var url = BuildUrl();
                var request = new HttpRequestMessage(GetSystemHttpMethod(), url);

                // 设置请求头
                foreach (var header in _headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                // 设置请求体
                if (_data != null && (_method == HttpMethod.POST || _method == HttpMethod.PUT || _method == HttpMethod.PATCH))
                {
                    var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                    request.Content = new StringContent(json, Encoding.UTF8, _contentType);
                }

                Console.WriteLine($"[HttpService] 发送请求: {_method} {url}");
                if (_data != null)
                {
                    Console.WriteLine($"[HttpService] 请求数据: {JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true })}");
                }

                using var httpClient = new HttpClient();
                httpClient.Timeout = _timeout;
                var response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[HttpService] 响应状态: {response.StatusCode}");
                Console.WriteLine($"[HttpService] 响应内容: {content}");

                var result = new HttpResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Content = content
                };

                // 添加响应头
                foreach (var header in response.Headers)
                {
                    result.Headers[header.Key] = string.Join(", ", header.Value);
                }

                if (!response.IsSuccessStatusCode)
                {
                    result.ErrorMessage = $"HTTP错误: {response.StatusCode}";
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"[HttpService] HTTP请求异常: {ex.Message}");
                return new HttpResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"网络请求失败: {ex.Message}"
                };
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"[HttpService] 请求超时: {ex.Message}");
                return new HttpResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "请求超时"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpService] 未知异常: {ex.Message}");
                return new HttpResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"请求失败: {ex.Message}"
                };
            }
        }

        private string BuildUrl()
        {
            var url = $"{_baseUrl}{_api}";
            
            if (_queryParams.Count > 0)
            {
                var queryString = string.Join("&", _queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                url += $"?{queryString}";
            }
            
            return url;
        }

        private System.Net.Http.HttpMethod GetSystemHttpMethod()
        {
            return _method switch
            {
                HttpMethod.GET => System.Net.Http.HttpMethod.Get,
                HttpMethod.POST => System.Net.Http.HttpMethod.Post,
                HttpMethod.PUT => System.Net.Http.HttpMethod.Put,
                HttpMethod.DELETE => System.Net.Http.HttpMethod.Delete,
                HttpMethod.PATCH => System.Net.Http.HttpMethod.Patch,
                _ => System.Net.Http.HttpMethod.Get
            };
        }

        private async Task<HttpResponse<byte[]>> ExecuteRequestBytesAsync()
        {
            try
            {
                var url = BuildUrl();
                var request = new HttpRequestMessage(GetSystemHttpMethod(), url);

                // 设置请求头
                foreach (var header in _headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                // 设置请求体
                if (_data != null && (_method == HttpMethod.POST || _method == HttpMethod.PUT || _method == HttpMethod.PATCH))
                {
                    var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                    request.Content = new StringContent(json, Encoding.UTF8, _contentType);
                }

                Console.WriteLine($"[HttpService] 发送请求: {_method} {url}");
                if (_data != null)
                {
                    Console.WriteLine($"[HttpService] 请求数据: {JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true })}");
                }

                using var httpClient = new HttpClient();
                httpClient.Timeout = _timeout;
                var response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsByteArrayAsync();

                Console.WriteLine($"[HttpService] 响应状态: {response.StatusCode}");
                Console.WriteLine($"[HttpService] 响应内容: [二进制数据，长度: {content.Length} 字节]");

                var result = new HttpResponse<byte[]>
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Data = content
                };

                // 添加响应头
                foreach (var header in response.Headers)
                {
                    result.Headers[header.Key] = string.Join(", ", header.Value);
                }

                if (!response.IsSuccessStatusCode)
                {
                    result.ErrorMessage = $"HTTP错误: {response.StatusCode}";
                }

                return result;
            }
            catch (Exception ex)
            {
                return new HttpResponse<byte[]>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}
