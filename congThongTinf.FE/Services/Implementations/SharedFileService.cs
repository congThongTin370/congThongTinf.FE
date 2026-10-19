using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;

namespace congThongTinf.FE.Services.Implementations;

public class SharedFileService : ISharedFileService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SharedFileService> _logger;

    public SharedFileService(HttpClient httpClient, ILogger<SharedFileService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SharedFileListViewModel> GetMyFilesAsync(string accessToken, int pageIndex, int pageSize, string? search)
    {
        var model = new SharedFileListViewModel
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            Search = search
        };

        try
        {
            var url = $"api/shared-files?pageIndex={pageIndex}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"&search={Uri.EscapeDataString(search.Trim())}";
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            var body = await ReadEnvelopeAsync<PagedData<SharedFileDto>>(response);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                model.ErrorMessage = UserFacingErrors.SessionExpired;
                return model;
            }

            if (!response.IsSuccessStatusCode || body?.IsSuccess != true || body.Value?.Data == null)
            {
                // Log để chẩn đoán: status và message thật từ backend (không hiển thị cho người dùng)
                _logger.LogWarning(
                    "Shared files list failed: HTTP {Status}, code {Code}, message {Message}",
                    (int)response.StatusCode, body?.Error?.Code, body?.Error?.Message);

                model.ErrorMessage = SharedFileErrors.Map((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message);
                return model;
            }

            var data = body.Value.Data;
            model.Items = data.Items;
            model.TotalCount = data.TotalCount;
            model.TotalPages = data.TotalPages;
            model.PageIndex = data.CurrentPage;
            model.PageSize = data.PageSize;
            model.HasPrevious = data.HasPrevious;
            model.HasNext = data.HasNext;
            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load shared files");
            model.ErrorMessage = UserFacingErrors.TryAgain;
            return model;
        }
    }

    public async Task<SharedFileWriteResult> UploadAsync(string accessToken, IFormFile file)
    {
        if (file.Length == 0)
        {
            return Failure(400, "400", "File là bắt buộc.");
        }

        try
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/shared-files") { Content = content };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            var body = await ReadEnvelopeAsync<SharedFileDto>(response);
            return ToWriteResult(response.StatusCode, response.IsSuccessStatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload shared file");
            return Failure(0, null, null);
        }
    }

    public Task<SharedFileWriteResult> CreateShareLinkAsync(string accessToken, Guid id) =>
        SendWriteAsync(HttpMethod.Post, $"api/shared-files/{id}/share", accessToken);

    public Task<SharedFileWriteResult> RevokeShareLinkAsync(string accessToken, Guid id) =>
        SendWriteAsync(HttpMethod.Delete, $"api/shared-files/{id}/share", accessToken);

    public async Task<SharedFileWriteResult> DeleteAsync(string accessToken, Guid id)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/shared-files/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            var body = await ReadEnvelopeAsync<bool>(response);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new SharedFileWriteResult { SessionExpired = true, ErrorMessage = UserFacingErrors.SessionExpired };
            }

            if (!response.IsSuccessStatusCode || body?.IsSuccess != true)
            {
                return Failure((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message);
            }

            return new SharedFileWriteResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete shared file");
            return Failure(0, null, null);
        }
    }

    public async Task<SharedFileReadResult> GetByLinkTokenAsync(string accessToken, string token)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/shared-files/links/{Uri.EscapeDataString(token)}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            var body = await ReadEnvelopeAsync<SharedFileDto>(response);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new SharedFileReadResult { SessionExpired = true, ErrorMessage = UserFacingErrors.SessionExpired };
            }

            if (!response.IsSuccessStatusCode || body?.IsSuccess != true || body.Value?.Data == null)
            {
                return new SharedFileReadResult
                {
                    ErrorMessage = SharedFileErrors.Map((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message)
                };
            }

            return new SharedFileReadResult { Success = true, Data = body.Value.Data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open shared file by link");
            return new SharedFileReadResult { ErrorMessage = UserFacingErrors.TryAgain };
        }
    }

    public async Task<SharedFileStreamResult> OpenStreamAsync(string accessToken, string backendUrl)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, backendUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Không dispose response ở đây: controller stream xong mới dispose qua ResponseStream
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                return new SharedFileStreamResult { SessionExpired = true, ErrorMessage = UserFacingErrors.SessionExpired };
            }

            if (!response.IsSuccessStatusCode)
            {
                // Lỗi nghiệp vụ có envelope: đọc message rồi đổi sang câu tiếng Việt
                var text = await response.Content.ReadAsStringAsync();
                response.Dispose();
                var body = TryParseEnvelope(text);
                return new SharedFileStreamResult
                {
                    ErrorMessage = SharedFileErrors.Map((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message)
                };
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"');
            var stream = await response.Content.ReadAsStreamAsync();

            return new SharedFileStreamResult
            {
                Success = true,
                Content = new ResponseStream(stream, response),
                ContentType = contentType,
                FileName = fileName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open shared file stream");
            return new SharedFileStreamResult { ErrorMessage = UserFacingErrors.TryAgain };
        }
    }

    private static BackendEnvelope<object>? TryParseEnvelope(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<BackendEnvelope<object>>(
                text,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    private async Task<SharedFileWriteResult> SendWriteAsync(HttpMethod method, string url, string accessToken)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            var body = await ReadEnvelopeAsync<SharedFileDto>(response);
            return ToWriteResult(response.StatusCode, response.IsSuccessStatusCode, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Share link request failed");
            return Failure(0, null, null);
        }
    }

    private static async Task<BackendEnvelope<T>?> ReadEnvelopeAsync<T>(HttpResponseMessage response)
    {
        // Lỗi framework (401/403) có thể không có body JSON
        var text = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<BackendEnvelope<T>>(
                text,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    private static SharedFileWriteResult ToWriteResult(HttpStatusCode status, bool ok, BackendEnvelope<SharedFileDto>? body)
    {
        if (status == HttpStatusCode.Unauthorized)
        {
            return new SharedFileWriteResult { SessionExpired = true, ErrorMessage = UserFacingErrors.SessionExpired };
        }

        if (!ok || body?.IsSuccess != true || body.Value?.Data == null)
        {
            return Failure((int)status, body?.Error?.Code, body?.Error?.Message);
        }

        return new SharedFileWriteResult { Success = true, Data = body.Value.Data };
    }

    private static SharedFileWriteResult Failure(int statusCode, string? code, string? message) => new()
    {
        Success = false,
        ErrorMessage = SharedFileErrors.Map(statusCode, code, message)
    };
}

/// <summary>
/// Đổi lỗi API sang câu tiếng Việt cho người dùng. Không lộ mã HTTP hay chi tiết exception.
/// </summary>
internal static class SharedFileErrors
{
    public static string Map(int statusCode, string? code, string? message)
    {
        var status = string.IsNullOrWhiteSpace(code) ? statusCode.ToString() : code;
        var text = message ?? string.Empty;

        if (status == "401")
        {
            return UserFacingErrors.SessionExpired;
        }

        if (status == "403")
        {
            return "Bạn không có quyền thao tác file này.";
        }

        if (text.Contains("10 MB", StringComparison.OrdinalIgnoreCase) || text.Contains("exceeds", StringComparison.OrdinalIgnoreCase))
        {
            return "File không được lớn hơn 10 MB.";
        }

        if (text.Contains("File là bắt buộc", StringComparison.OrdinalIgnoreCase))
        {
            return "Vui lòng chọn file cần tải lên.";
        }

        if (status == "404" && text.Contains("không hợp lệ", StringComparison.OrdinalIgnoreCase))
        {
            return "Liên kết chia sẻ không hợp lệ hoặc đã bị thu hồi.";
        }

        if (status == "404" && text.Contains("File với ID", StringComparison.OrdinalIgnoreCase))
        {
            return "Không tìm thấy file này.";
        }

        if (status == "404")
        {
            return "Không xác định được tài khoản. Vui lòng đăng nhập lại.";
        }

        if (status == "400" && text.Contains("không hợp lệ", StringComparison.OrdinalIgnoreCase))
        {
            return "Liên kết chia sẻ không hợp lệ.";
        }

        return UserFacingErrors.TryAgain;
    }
}
