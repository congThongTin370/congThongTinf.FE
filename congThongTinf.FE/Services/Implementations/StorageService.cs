using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;

namespace congThongTinf.FE.Services.Implementations;

public class StorageService : IStorageService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StorageService> _logger;

    public StorageService(HttpClient httpClient, ILogger<StorageService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<StorageUsageResult> GetUsageAsync(string accessToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/admin/storage");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new StorageUsageResult { SessionExpired = true };
            }

            var body = await response.Content.ReadFromJsonAsync<BackendEnvelope<StorageUsageDto>>();
            if (response.IsSuccessStatusCode && body?.IsSuccess == true && body.Value?.Data != null)
            {
                return new StorageUsageResult { Success = true, Data = body.Value.Data };
            }

            return new StorageUsageResult
            {
                ErrorMessage = string.IsNullOrWhiteSpace(body?.Error?.Message)
                    ? "Không lấy được dung lượng lưu trữ."
                    : body.Error.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load storage usage");
            return new StorageUsageResult { ErrorMessage = "Không lấy được dung lượng lưu trữ." };
        }
    }
}
