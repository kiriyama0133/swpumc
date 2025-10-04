using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace swpumc.Services
{
    /// <summary>
    /// 下载进度记录
    /// </summary>
    public record DownloadProgress(long BytesDownloaded, long TotalBytes);

    /// <summary>
    /// HTTP下载服务接口
    /// </summary>
    public interface IHttpDownloadService
    {
        /// <summary>
        /// 下载文件到指定路径
        /// </summary>
        /// <param name="url">下载URL</param>
        /// <param name="destinationPath">目标路径</param>
        /// <param name="progress">进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>下载任务</returns>
        Task DownloadFileAsync(string url, string destinationPath, 
            IProgress<DownloadProgress>? progress = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取文件内容类型
        /// </summary>
        /// <param name="url">文件URL</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>内容类型</returns>
        Task<string?> GetFileContentTypeAsync(string url, CancellationToken cancellationToken = default);

        /// <summary>
        /// 下载文件到字节数组
        /// </summary>
        /// <param name="url">下载URL</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>文件字节数组</returns>
        Task<byte[]?> DownloadBytesAsync(string url, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// HTTP下载服务实现
    /// </summary>
    public class HttpDownloadService : IHttpDownloadService, IDisposable
    {
        private readonly HttpClient _httpClient;
        public int Retries { get; set; } = 3;
        public int BufferSize { get; set; } = 8192; // 8KB缓冲区

        public HttpDownloadService(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(5); // 5分钟超时
        }

        /// <summary>
        /// 下载文件到指定路径
        /// </summary>
        public async Task DownloadFileAsync(string url, string destinationPath, 
            IProgress<DownloadProgress>? progress = null, 
            CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < Retries; i++)
            {
                try
                {
                    Console.WriteLine($"[HttpDownloadService] 开始下载文件: {url}");
                    await PerformDownloadAsync(url, destinationPath, progress, cancellationToken);
                    Console.WriteLine($"[HttpDownloadService] 文件下载成功: {destinationPath}");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HttpDownloadService] 第 {i + 1} 次下载失败，将在2秒后重试: {ex.Message}");
                    if (i == Retries - 1)
                    {
                        Console.WriteLine($"[HttpDownloadService] 已达到最大重试次数，下载文件失败: {url}");
                        throw;
                    }
                    await Task.Delay(2000, cancellationToken);
                }
            }
        }

        /// <summary>
        /// 执行下载操作
        /// </summary>
        private async Task PerformDownloadAsync(string url, string destinationPath, 
            IProgress<DownloadProgress>? progress, CancellationToken cancellationToken)
        {
            // 确保目标目录存在
            var directory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            long existingFileSize = 0;
            if (File.Exists(destinationPath))
            {
                existingFileSize = new FileInfo(destinationPath).Length;
            }

            var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
            if (existingFileSize > 0)
            {
                request.Headers.Range = new RangeHeaderValue(existingFileSize, null);
                Console.WriteLine($"[HttpDownloadService] 文件已存在，从 {existingFileSize} 字节处继续下载");
            }

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                existingFileSize = 0;
            }
            else if (response.StatusCode != System.Net.HttpStatusCode.PartialContent && existingFileSize > 0)
            {
                throw new InvalidOperationException($"服务器不支持断点续传 (状态码: {response.StatusCode})");
            }
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? 0;
            if (existingFileSize > 0 && response.Content.Headers.ContentRange != null)
            {
                totalBytes = response.Content.Headers.ContentRange.Length ?? 0;
            }
            Console.WriteLine($"[HttpDownloadService] 开始下载，总大小 {totalBytes} 字节");

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(destinationPath, existingFileSize == 0 ? FileMode.Create : FileMode.Append, FileAccess.Write, FileShare.None);

            var buffer = new byte[BufferSize];
            long totalBytesRead = existingFileSize;
            int bytesRead;
            
            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalBytesRead += bytesRead;
                progress?.Report(new DownloadProgress(totalBytesRead, totalBytes));
            }
        }

        /// <summary>
        /// 获取文件内容类型
        /// </summary>
        public async Task<string?> GetFileContentTypeAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Head, url);
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
                return response.Content.Headers.ContentType?.MediaType;
            }
            catch (HttpRequestException ex) when (ex.StatusCode != null)
            {
                Console.WriteLine($"[HttpDownloadService] 获取文件内容类型失败，HTTP 状态码: {ex.StatusCode} URL: {url}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpDownloadService] 获取文件内容类型失败: {url} - {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 下载文件到字节数组
        /// </summary>
        public async Task<byte[]?> DownloadBytesAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                Console.WriteLine($"[HttpDownloadService] 开始下载字节数据: {url}");
                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                Console.WriteLine($"[HttpDownloadService] 成功下载 {bytes.Length} 字节");
                return bytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpDownloadService] 下载字节数据失败: {url} - {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
