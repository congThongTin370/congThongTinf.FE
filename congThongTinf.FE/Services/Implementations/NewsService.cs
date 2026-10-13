using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;

namespace congThongTinf.FE.Services.Implementations;

public class NewsService : INewsService
{
    private readonly ApiClient _apiClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NewsService> _logger;
    private string _apiBaseUrl;

    public NewsService(ApiClient apiClient, IConfiguration configuration, ILogger<NewsService> logger)
    {
        _apiClient = apiClient;
        _configuration = configuration;
        _logger = logger;
        _apiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
    }

    /// <summary>
    /// Lấy danh sách tất cả các bài báo với khả năng lọc
    /// </summary>
    public async Task<List<NewsDto>> GetAllNewsAsync(NewsQueryDto? query = null)
    {
        try
        {
            // categoryIds phải lặp lại dạng ?categoryIds=a&categoryIds=b (theo docs backend)
            var endpoint = $"{_apiBaseUrl}/api/news{BuildQueryString(query)}";

            _logger.LogInformation($"Fetching news from: {endpoint}");

            var response = await _apiClient.GetAsync<ApiResponse<PagedData<NewsDto>>>(endpoint);

            if (response?.IsSuccess == true && response.Value?.Data != null)
            {
                var items = response.Value.Data.Items;
                _logger.LogInformation($"Successfully fetched {items.Count} news items");
                return items;
            }
            else
            {
                _logger.LogWarning($"API returned unsuccessful response: {response?.Value?.Message}");
                return new List<NewsDto>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching news: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Lấy bài báo theo ID
    /// </summary>
    public async Task<NewsDto> GetNewsByIdAsync(Guid id)
    {
        try
        {
            var endpoint = $"{_apiBaseUrl}/api/news/{id}";
            _logger.LogInformation($"Fetching news with ID: {id}");

            var response = await _apiClient.GetAsync<ApiResponse<NewsDto>>(endpoint);

            if (response?.IsSuccess == true && response.Value?.Data != null)
            {
                return response.Value.Data;
            }
            else
            {
                throw new Exception($"Failed to fetch news: {response?.Value?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching news by ID: {ex.Message}");
            throw;
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

        return "?" + string.Join("&", parts);
    }
}
