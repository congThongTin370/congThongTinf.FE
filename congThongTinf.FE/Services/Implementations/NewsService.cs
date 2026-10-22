using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;
using Microsoft.Extensions.Caching.Memory;

namespace congThongTinf.FE.Services.Implementations;

public class NewsService : INewsService
{
    private readonly ApiClient _apiClient;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NewsService> _logger;
    private string _apiBaseUrl;

    public NewsService(
        ApiClient apiClient,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<NewsService> logger)
    {
        _apiClient = apiClient;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
        _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    }

    /// <summary>
    /// Lấy danh sách tất cả các bài báo với khả năng lọc
    /// </summary>
    public async Task<List<NewsDto>> GetAllNewsAsync(NewsQueryDto? query = null)
    {
        var cacheKey = "news" + BuildQueryString(query);
        if (PublicReadCache.TryGetFresh(_cache, cacheKey, out List<NewsDto>? fresh) && fresh != null)
        {
            return fresh;
        }

        try
        {
            // categoryIds lặp ?categoryIds=a&categoryIds=b. Không gửi status: bài Draft của phóng viên vẫn lên cổng.
            var endpoint = $"{_apiBaseUrl}/api/news{BuildQueryString(query)}";
            var response = await _apiClient.GetAsync<ApiResponse<PagedData<NewsDto>>>(endpoint);

            if (response?.IsSuccess == true && response.Value?.Data != null)
            {
                var items = response.Value.Data.Items ?? new List<NewsDto>();
                PublicReadCache.Set(_cache, cacheKey, items);
                return items;
            }

            _logger.LogWarning("News list request was unsuccessful: {Message}", response?.Value?.Message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "News list request failed");
        }

        return PublicReadCache.TryGetStale(_cache, cacheKey, out List<NewsDto>? stale) && stale != null
            ? stale
            : new List<NewsDto>();
    }

    /// <summary>
    /// Lấy bài báo theo ID
    /// </summary>
    public async Task<NewsDto> GetNewsByIdAsync(Guid id)
    {
        var endpoint = $"{_apiBaseUrl}/api/news/{id}";
        var response = await _apiClient.GetAsync<ApiResponse<NewsDto>>(endpoint);

        if (response?.IsSuccess == true && response.Value?.Data != null)
        {
            return response.Value.Data;
        }

        throw new Exception($"Failed to fetch news: {response?.Value?.Message}");
    }

    /// <summary>
    /// Tăng lượt xem khi người đọc mở trang chi tiết. GET /api/news/{id} không tự cộng.
    /// </summary>
    public async Task<int?> IncrementViewCountAsync(Guid id)
    {
        try
        {
            var endpoint = $"{_apiBaseUrl}/api/news/{id}/view";
            var response = await _apiClient.PostAsync<ApiResponse<int>>(endpoint);

            if (response?.IsSuccess == true)
            {
                return response.Value?.Data;
            }

            _logger.LogWarning("Could not increment view count for {NewsId}: {Message}", id, response?.Value?.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not increment view count for {NewsId}", id);
            return null;
        }
    }

    /// <summary>
    /// Xây dựng query string từ NewsQueryDto. categoryIds được lặp lại cho từng Guid.
    /// </summary>
    private static string BuildQueryString(NewsQueryDto? query)
    {
        if (query == null)
            return "?pageIndex=0";

        var parts = new List<string>();

        // Không gửi pageIndex thì API mặc định trang 1, 10 bài. Chỗ cần đủ danh sách phải gửi 0.
        parts.Add($"pageIndex={query.PageIndex ?? 0}");

        if (query.PageSize.HasValue)
            parts.Add($"pageSize={query.PageSize.Value}");

        if (query.FromDate.HasValue)
            parts.Add($"fromDate={Uri.EscapeDataString(query.FromDate.Value.ToString("o"))}");

        if (query.ToDate.HasValue)
            parts.Add($"toDate={Uri.EscapeDataString(query.ToDate.Value.ToString("o"))}");

        if (query.CategoryIds != null)
            parts.AddRange(query.CategoryIds.Select(id => $"categoryIds={id}"));

        if (!string.IsNullOrWhiteSpace(query.Search))
            parts.Add($"search={Uri.EscapeDataString(query.Search.Trim())}");

        if (query.SortBy == 1)
            parts.Add("sortBy=1");

        return "?" + string.Join("&", parts);
    }
}
