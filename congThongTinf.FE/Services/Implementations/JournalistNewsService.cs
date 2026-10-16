using System.Net.Http.Headers;
using System.Net.Http.Json;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;

namespace congThongTinf.FE.Services.Implementations;

public class JournalistNewsService : IJournalistNewsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JournalistNewsService> _logger;

    public JournalistNewsService(HttpClient httpClient, ILogger<JournalistNewsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<NewsDto>> GetMyNewsAsync(string accessToken, Guid authorId, NewsQueryDto? query = null)
    {
        try
        {
            // Backend chưa lọc theo tác giả: lấy danh sách đã lọc/sắp xếp rồi giữ bài của phóng viên.
            using var response = await _httpClient.GetAsync(BuildMyNewsUrl(query));
            var body = await response.Content.ReadFromJsonAsync<BackendEnvelope<PagedData<NewsDto>>>();

            if (body?.IsSuccess != true || body.Value?.Data == null)
            {
                return new List<NewsDto>();
            }

            var mine = body.Value.Data.Items.Where(n => n.AuthorId == authorId);
            if (query?.SortBy == 1)
            {
                return mine
                    .OrderByDescending(n => n.ViewCount)
                    .ThenByDescending(n => n.CreatedAt)
                    .ToList();
            }

            return mine.OrderByDescending(n => n.CreatedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load news list");
            return new List<NewsDto>();
        }
    }

    private static string BuildMyNewsUrl(NewsQueryDto? query)
    {
        var parts = new List<string> { "pageIndex=0" };
        if (query == null)
        {
            return "api/news?pageIndex=0";
        }

        if (query.FromDate.HasValue)
        {
            parts.Add($"fromDate={Uri.EscapeDataString(query.FromDate.Value.ToString("o"))}");
        }

        if (query.ToDate.HasValue)
        {
            parts.Add($"toDate={Uri.EscapeDataString(query.ToDate.Value.ToString("o"))}");
        }

        if (query.CategoryIds != null)
        {
            parts.AddRange(query.CategoryIds.Select(id => $"categoryIds={id}"));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            parts.Add($"search={Uri.EscapeDataString(query.Search.Trim())}");
        }

        if (query.SortBy == 1)
        {
            parts.Add("sortBy=1");
        }

        return "api/news?" + string.Join("&", parts);
    }

    public async Task<NewsDto?> GetNewsByIdAsync(Guid id)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"api/news/{id}");
            var body = await response.Content.ReadFromJsonAsync<BackendEnvelope<NewsDto>>();
            return body?.IsSuccess == true ? body.Value?.Data : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load news {NewsId}", id);
            return null;
        }
    }

    public Task<NewsWriteResult> CreateAsync(string accessToken, NewsWriteRequestDto request)
    {
        return SendMultipartAsync(HttpMethod.Post, "api/news", accessToken, request);
    }

    public Task<NewsWriteResult> UpdateAsync(string accessToken, Guid id, NewsWriteRequestDto request)
    {
        return SendMultipartAsync(HttpMethod.Put, $"api/news/{id}", accessToken, request);
    }

    public async Task<NewsWriteResult> DeleteAsync(string accessToken, Guid id)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/news/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadFromJsonAsync<BackendEnvelope<bool>>();

            if (!response.IsSuccessStatusCode || body?.IsSuccess != true)
            {
                return Failure((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message);
            }

            return new NewsWriteResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete news {NewsId}", id);
            return Failure(0, null, null);
        }
    }

    private async Task<NewsWriteResult> SendMultipartAsync(
        HttpMethod method,
        string url,
        string accessToken,
        NewsWriteRequestDto payload)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(payload.Title.Trim()), "title");
            content.Add(new StringContent(payload.Content ?? string.Empty), "content");
            content.Add(new StringContent(payload.Summary?.Trim() ?? string.Empty), "summary");
            content.Add(new StringContent(payload.CategoryId.ToString()), "categoryId");

            Stream? fileStream = null;
            try
            {
                if (payload.CoverImage is { Length: > 0 })
                {
                    fileStream = payload.CoverImage.OpenReadStream();
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                        string.IsNullOrWhiteSpace(payload.CoverImage.ContentType)
                            ? "application/octet-stream"
                            : payload.CoverImage.ContentType);
                    content.Add(fileContent, "coverImage", payload.CoverImage.FileName);
                }

                using var request = new HttpRequestMessage(method, url) { Content = content };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadFromJsonAsync<BackendEnvelope<NewsDto>>();
                if (!response.IsSuccessStatusCode || body?.IsSuccess != true || body.Value?.Data == null)
                {
                    return Failure((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message);
                }

                return new NewsWriteResult { Success = true, Data = body.Value.Data };
            }
            finally
            {
                fileStream?.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "News request {Method} {Url} failed", method, url);
            return Failure(0, null, null);
        }
    }

    public async Task<NewsImageUploadResult> UploadContentImageAsync(string accessToken, IFormFile file)
    {
        if (file.Length == 0)
        {
            return ImageFailure(400, "400", "File là bắt buộc.");
        }

        try
        {
            using var content = new MultipartFormDataContent();
            await using var fileStream = file.OpenReadStream();
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            using var request = new HttpRequestMessage(HttpMethod.Post, "api/news/images")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadFromJsonAsync<BackendEnvelope<NewsImageDto>>();

            if (!response.IsSuccessStatusCode || body?.IsSuccess != true || string.IsNullOrWhiteSpace(body.Value?.Data?.Url))
            {
                return ImageFailure((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message);
            }

            return new NewsImageUploadResult { Success = true, Url = body.Value.Data.Url };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload news content image");
            return ImageFailure(0, null, null);
        }
    }

    private static NewsWriteResult Failure(int statusCode, string? code, string? message) => new()
    {
        Success = false,
        ErrorMessage = UserFacingErrors.ForNews(statusCode, code, message)
    };

    private static NewsImageUploadResult ImageFailure(int statusCode, string? code, string? message) => new()
    {
        Success = false,
        ErrorMessage = UserFacingErrors.ForNewsImage(statusCode, code, message)
    };
}
