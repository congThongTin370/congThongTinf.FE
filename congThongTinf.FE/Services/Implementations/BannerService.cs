using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;

namespace congThongTinf.FE.Services.Implementations;

public class BannerService : IBannerService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<BannerService> _logger;

    public BannerService(HttpClient httpClient, ILogger<BannerService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<BannerListResult> GetBannersAsync(string accessToken, BannerQuery query)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BuildListUrl(query));
            var hasToken = ApplyBearer(request, accessToken);
            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized && hasToken)
            {
                return new BannerListResult
                {
                    Success = false,
                    SessionExpired = true,
                    ErrorMessage = UserFacingErrors.SessionExpired
                };
            }

            var body = await ReadEnvelopeAsync<PagedData<BannerDto>>(response);
            if (!response.IsSuccessStatusCode || body?.IsSuccess != true)
            {
                return new BannerListResult
                {
                    Success = false,
                    ErrorMessage = BannerError(response.StatusCode, body?.Error?.Code, body?.Error?.Message)
                };
            }

            var page = body.Value?.Data;
            return new BannerListResult
            {
                Success = true,
                Items = page?.Items ?? new List<BannerDto>(),
                TotalCount = page?.TotalCount ?? 0,
                HasPrevious = page?.HasPrevious ?? query.PageIndex > 1,
                HasNext = page?.HasNext ?? false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load banners");
            return new BannerListResult
            {
                Success = false,
                ErrorMessage = UserFacingErrors.TryAgain
            };
        }
    }

    public async Task<BannerFileListResult> GetFilesAsync(string accessToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/admin/banners/files");
            var hasToken = ApplyBearer(request, accessToken);
            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized && hasToken)
            {
                return new BannerFileListResult
                {
                    SessionExpired = true,
                    ErrorMessage = UserFacingErrors.SessionExpired
                };
            }

            var body = await ReadEnvelopeAsync<List<BannerFileDto>>(response);
            if (!response.IsSuccessStatusCode || body?.IsSuccess != true)
            {
                return new BannerFileListResult
                {
                    ErrorMessage = string.IsNullOrWhiteSpace(body?.Error?.Message)
                        ? UserFacingErrors.ForBanners((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message)
                        : body.Error.Message.Trim()
                };
            }

            return new BannerFileListResult
            {
                Success = true,
                Items = body.Value?.Data ?? new List<BannerFileDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list banner files");
            return new BannerFileListResult { ErrorMessage = UserFacingErrors.TryAgain };
        }
    }

    public async Task<BannerWriteResult> GetByIdAsync(string accessToken, Guid id)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/admin/banners/{id}");
            ApplyBearer(request, accessToken);
            using var response = await _httpClient.SendAsync(request);
            return await ReadBannerAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load banner {BannerId}", id);
            return Failure(0, null, null);
        }
    }

    public Task<BannerWriteResult> CreateAsync(string accessToken, BannerFormViewModel model)
    {
        return SendFormAsync(HttpMethod.Post, "api/admin/banners", accessToken, model, includeFile: true);
    }

    public Task<BannerWriteResult> UpdateAsync(string accessToken, Guid id, BannerFormViewModel model)
    {
        return SendFormAsync(HttpMethod.Put, $"api/admin/banners/{id}", accessToken, model, includeFile: model.File is { Length: > 0 });
    }

    public async Task<BannerWriteResult> SetActiveAsync(string accessToken, Guid id)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"api/admin/banners/{id}/active");
            ApplyBearer(request, accessToken);
            using var response = await _httpClient.SendAsync(request);
            return await ReadBannerAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set active banner {BannerId}", id);
            return Failure(0, null, null);
        }
    }

    public async Task<BannerWriteResult> DeleteAsync(string accessToken, Guid id)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/admin/banners/{id}");
            ApplyBearer(request, accessToken);
            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return SessionExpiredResult();
            }

            var body = await ReadEnvelopeAsync<bool>(response);
            if (!response.IsSuccessStatusCode || body?.IsSuccess != true)
            {
                return Failure(response.StatusCode, body?.Error?.Code, body?.Error?.Message);
            }

            return new BannerWriteResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete banner {BannerId}", id);
            return Failure(0, null, null);
        }
    }

    private async Task<BannerWriteResult> SendFormAsync(
        HttpMethod method,
        string url,
        string accessToken,
        BannerFormViewModel model,
        bool includeFile)
    {
        Stream? fileStream = null;
        try
        {
            using var content = new MultipartFormDataContent();

            if (includeFile && model.File != null)
            {
                fileStream = model.File.OpenReadStream();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(model.File.ContentType)
                        ? "application/octet-stream"
                        : model.File.ContentType);
                content.Add(fileContent, "file", model.File.FileName);
            }

            if (!string.IsNullOrWhiteSpace(model.Title))
            {
                content.Add(new StringContent(model.Title.Trim()), "title");
            }

            content.Add(new StringContent(model.LinkUrl?.Trim() ?? string.Empty), "linkUrl");
            content.Add(new StringContent(model.Position.ToString(System.Globalization.CultureInfo.InvariantCulture)), "position");
            content.Add(new StringContent(model.IsActive ? "true" : "false"), "isActive");

            using var request = new HttpRequestMessage(method, url) { Content = content };
            ApplyBearer(request, accessToken);
            using var response = await _httpClient.SendAsync(request);
            return await ReadBannerAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Banner request {Method} {Url} failed", method, url);
            return Failure(0, null, null);
        }
        finally
        {
            if (fileStream != null)
            {
                await fileStream.DisposeAsync();
            }
        }
    }

    private static async Task<BannerWriteResult> ReadBannerAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return SessionExpiredResult();
        }

        var body = await ReadEnvelopeAsync<BannerDto>(response);
        if (!response.IsSuccessStatusCode || body?.IsSuccess != true || body.Value?.Data == null)
        {
            return Failure(response.StatusCode, body?.Error?.Code, body?.Error?.Message);
        }

        return new BannerWriteResult { Success = true, Data = body.Value.Data };
    }

    private static async Task<BackendEnvelope<T>?> ReadEnvelopeAsync<T>(HttpResponseMessage response)
    {
        var responseText = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseText))
        {
            return null;
        }

        return JsonSerializer.Deserialize<BackendEnvelope<T>>(responseText, JsonOptions);
    }

    private static string BuildListUrl(BannerQuery query)
    {
        var parts = new List<string>
        {
            $"pageIndex={query.PageIndex}",
            $"pageSize={query.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            parts.Add($"search={Uri.EscapeDataString(query.Search.Trim())}");
        }

        if (query.Position.HasValue)
        {
            parts.Add($"position={query.Position.Value}");
        }

        if (query.IsActive.HasValue)
        {
            parts.Add($"isActive={(query.IsActive.Value ? "true" : "false")}");
        }

        return "api/admin/banners?" + string.Join("&", parts);
    }

    private static bool ApplyBearer(HttpRequestMessage request, string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return false;
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return true;
    }

    private static BannerWriteResult SessionExpiredResult() => new()
    {
        Success = false,
        SessionExpired = true,
        ErrorMessage = UserFacingErrors.SessionExpired
    };

    private static string BannerError(HttpStatusCode statusCode, string? code, string? message) =>
        UserFacingErrors.ForBanners((int)statusCode, code, message);

    private static BannerWriteResult Failure(HttpStatusCode statusCode, string? code, string? message) => new()
    {
        Success = false,
        ErrorMessage = BannerError(statusCode, code, message)
    };

    private static BannerWriteResult Failure(int statusCode, string? code, string? message) =>
        Failure((HttpStatusCode)statusCode, code, message);
}
