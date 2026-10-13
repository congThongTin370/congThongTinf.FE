using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;

namespace congThongTinf.FE.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(HttpClient httpClient, ILogger<DashboardService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<DashboardResult> GetDashboardAsync(string accessToken, DateTime fromDate, DateTime toDate)
    {
        var range = $"fromDate={Format(fromDate)}&toDate={Format(toDate)}";

        // 5 endpoint độc lập, gọi tuần tự để tránh song song trên cùng HttpClient/scope không cần thiết
        // và để một endpoint lỗi không làm hỏng cả trang.
        var users = await GetAsync<DashboardUsersPayload>(accessToken, $"api/dashboard/users?{range}");
        if (users.SessionExpired)
        {
            return SessionExpired();
        }

        var news = await GetAsync<DashboardNewsPayload>(accessToken, $"api/dashboard/news?{range}");
        if (news.SessionExpired)
        {
            return SessionExpired();
        }

        var documents = await GetAsync<DashboardDocumentsPayload>(accessToken, $"api/dashboard/documents?{range}");
        if (documents.SessionExpired)
        {
            return SessionExpired();
        }

        var categories = await GetAsync<DashboardCountsPayload>(accessToken, "api/dashboard/categories");
        if (categories.SessionExpired)
        {
            return SessionExpired();
        }

        var banners = await GetAsync<DashboardCountsPayload>(accessToken, "api/dashboard/banners");
        if (banners.SessionExpired)
        {
            return SessionExpired();
        }

        var loaded = new[] { users.Success, news.Success, documents.Success, categories.Success, banners.Success };
        if (!loaded.Any(x => x))
        {
            return new DashboardResult { ErrorMessage = users.ErrorMessage ?? UserFacingErrors.TryAgain };
        }

        var data = new DashboardDto
        {
            FromDate = news.Data?.FromDate ?? fromDate,
            ToDate = news.Data?.ToDate ?? toDate,
            Users = users.Data is { } u ? new DashboardUserStats
            {
                Total = u.Total,
                Active = u.Active,
                Inactive = u.Inactive,
                Admin = u.Admin,
                Journalist = u.Journalist,
                LoggedInInRange = u.LoggedInInRange
            } : new DashboardUserStats(),
            News = news.Data?.Stats ?? new DashboardNewsStats(),
            NewsByCategory = news.Data?.NewsByCategory ?? new(),
            NewsByDay = news.Data?.NewsByDay ?? new(),
            PendingNews = news.Data?.PendingNews ?? new(),
            TopViewedNews = news.Data?.TopViewedNews ?? new(),
            Documents = documents.Data is { } d ? new DashboardDocumentStats
            {
                Total = d.Total,
                UploadedInRange = d.UploadedInRange,
                TotalFileSize = d.TotalFileSize,
                ByCategory = d.ByCategory
            } : new DashboardDocumentStats(),
            RecentDocuments = documents.Data?.RecentItems ?? new(),
            Categories = categories.Data is { } c ? new DashboardCategoryStats { Total = c.Total, Active = c.Active } : new(),
            Banners = banners.Data is { } b ? new DashboardBannerStats { Total = b.Total, Active = b.Active } : new()
        };

        return new DashboardResult { Success = true, Data = data };
    }

    private async Task<EndpointResult<T>> GetAsync<T>(string accessToken, string url) where T : class
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return EndpointResult<T>.Expired();
            }

            var body = await response.Content.ReadFromJsonAsync<BackendEnvelope<T>>();
            if (!response.IsSuccessStatusCode || body?.IsSuccess != true || body.Value?.Data == null)
            {
                _logger.LogWarning("Dashboard endpoint {Url} returned status {StatusCode}", url, response.StatusCode);
                return EndpointResult<T>.Failed(response.StatusCode == HttpStatusCode.Forbidden
                    ? UserFacingErrors.NoPermission
                    : UserFacingErrors.TryAgain);
            }

            return EndpointResult<T>.Ok(body.Value.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load dashboard endpoint {Url}", url);
            return EndpointResult<T>.Failed(UserFacingErrors.TryAgain);
        }
    }

    private static DashboardResult SessionExpired() => new()
    {
        SessionExpired = true,
        ErrorMessage = UserFacingErrors.SessionExpired
    };

    private static string Format(DateTime date) =>
        date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private sealed class EndpointResult<T>
    {
        public bool Success { get; private init; }
        public bool SessionExpired { get; private init; }
        public T? Data { get; private init; }
        public string? ErrorMessage { get; private init; }

        public static EndpointResult<T> Ok(T data) => new() { Success = true, Data = data };
        public static EndpointResult<T> Failed(string message) => new() { ErrorMessage = message };
        public static EndpointResult<T> Expired() => new() { SessionExpired = true };
    }
}
