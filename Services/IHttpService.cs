using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace swpumc.Services
{
    public interface IHttpService
    {
        IHttpRequestBuilder CreateRequest();
    }

    public interface IHttpRequestBuilder
    {
        IHttpRequestBuilder SetBaseUrl(string baseUrl);
        IHttpRequestBuilder SetApi(string api);
        IHttpRequestBuilder SetMethod(HttpMethod method);
        IHttpRequestBuilder SetData<T>(T data);
        IHttpRequestBuilder SetHeaders(Dictionary<string, string> headers);
        IHttpRequestBuilder SetHeader(string key, string value);
        IHttpRequestBuilder SetQueryParam(string key, string value);
        IHttpRequestBuilder SetTimeout(TimeSpan timeout);
        IHttpRequestBuilder SetContentType(string contentType);
        
        Task<HttpResponse<T>> ExecuteAsync<T>();
        Task<HttpResponse> ExecuteAsync();
        Task<HttpResponse<byte[]>> ExecuteBytesAsync();
    }

    public enum HttpMethod
    {
        GET,
        POST,
        PUT,
        DELETE,
        PATCH
    }

    public class HttpResponse<T>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }

    public class HttpResponse
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string? Content { get; set; }
        public string? ErrorMessage { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }
}
