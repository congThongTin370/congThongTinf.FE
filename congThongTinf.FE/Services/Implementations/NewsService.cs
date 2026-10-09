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
            var endpoint = $"{_apiBaseUrl}/api/news";
            var queryParams = BuildQueryParameters(query);

            _logger.LogInformation($"Fetching news from: {endpoint}");

            var response = await _apiClient.GetAsync<ApiResponse<List<NewsDto>>>(endpoint, queryParams);

            if (response?.IsSuccess == true && response.Value?.Data != null)
            {
                _logger.LogInformation($"Successfully fetched {response.Value.Data.Count} news items");
                return response.Value.Data;
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
    /// Xây dựng query parameters từ NewsQueryDto
    /// </summary>
    private Dictionary<string, string>? BuildQueryParameters(NewsQueryDto? query)
    {
        if (query == null)
            return null;

        var queryParams = new Dictionary<string, string>();

        if (query.FromDate.HasValue)
            queryParams.Add("fromDate", query.FromDate.Value.ToString("o"));

        if (query.ToDate.HasValue)
            queryParams.Add("toDate", query.ToDate.Value.ToString("o"));

        if (query.CategoryIds != null && query.CategoryIds.Count > 0)
            queryParams.Add("categoryIds", string.Join(",", query.CategoryIds));

        return queryParams.Count > 0 ? queryParams : null;
    }
}
