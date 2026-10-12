using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using congThongTinf.FE.Models;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;

namespace congThongTinf.FE.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(HttpClient httpClient, ILogger<CategoryService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetActiveCategoriesAsync()
    {
        try
        {
            // GET api/categories là công khai, không cần token
            using var response = await _httpClient.GetAsync("api/categories");
            var body = await response.Content.ReadFromJsonAsync<BackendEnvelope<List<CategoryDto>>>();

            if (!response.IsSuccessStatusCode || body?.IsSuccess != true)
            {
                return new List<CategoryDto>();
            }

            return body.Value?.Data ?? new List<CategoryDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load categories");
            return new List<CategoryDto>();
        }
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(Guid id)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"api/categories/{id}");
            var body = await response.Content.ReadFromJsonAsync<BackendEnvelope<CategoryDto>>();

            return response.IsSuccessStatusCode && body?.IsSuccess == true ? body.Value?.Data : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load category {CategoryId}", id);
            return null;
        }
    }

    public Task<CategoryWriteResult> CreateAsync(string accessToken, CategoryFormViewModel model)
    {
        return SendAsync(HttpMethod.Post, "api/categories", accessToken, ToRequest(model));
    }

    public Task<CategoryWriteResult> UpdateAsync(string accessToken, Guid id, CategoryFormViewModel model)
    {
        return SendAsync(HttpMethod.Put, $"api/categories/{id}", accessToken, ToRequest(model));
    }

    public async Task<CategoryWriteResult> DeleteAsync(string accessToken, Guid id)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/categories/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(request);
            return await ReadDeleteAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete category {CategoryId}", id);
            return Failure(0, null, null);
        }
    }

    private async Task<CategoryWriteResult> SendAsync(HttpMethod method, string url, string accessToken, CategoryWriteRequest payload)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            return await ReadCategoryAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Category request {Method} {Url} failed", method, url);
            return Failure(0, null, null);
        }
    }

    private static async Task<CategoryWriteResult> ReadCategoryAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return SessionExpiredResult();
        }

        var body = await ReadEnvelopeAsync<CategoryDto>(response);
        if (!response.IsSuccessStatusCode || body?.IsSuccess != true || body.Value?.Data == null)
        {
            return Failure((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message);
        }

        return new CategoryWriteResult { Success = true, Data = body.Value.Data };
    }

    private static async Task<CategoryWriteResult> ReadDeleteAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return SessionExpiredResult();
        }

        var body = await ReadEnvelopeAsync<bool>(response);
        if (!response.IsSuccessStatusCode || body?.IsSuccess != true)
        {
            return Failure((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message);
        }

        return new CategoryWriteResult { Success = true };
    }

    private static async Task<BackendEnvelope<T>?> ReadEnvelopeAsync<T>(HttpResponseMessage response)
    {
        var responseText = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseText))
        {
            return null;
        }

        return JsonSerializer.Deserialize<BackendEnvelope<T>>(responseText, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private static CategoryWriteResult SessionExpiredResult() => new()
    {
        Success = false,
        SessionExpired = true,
        ErrorMessage = UserFacingErrors.SessionExpired
    };

    private static CategoryWriteRequest ToRequest(CategoryFormViewModel model)
    {
        return new CategoryWriteRequest
        {
            Name = model.Name.Trim(),
            Description = model.Description?.Trim() ?? string.Empty,
            Icon = string.IsNullOrWhiteSpace(model.Icon) ? null : model.Icon.Trim(),
            DisplayOrder = model.DisplayOrder,
            IsActive = true
        };
    }

    private static CategoryWriteResult Failure(int statusCode, string? code, string? message) => new()
    {
        Success = false,
        ErrorMessage = UserFacingErrors.ForCategories(statusCode, code, message)
    };

    private sealed class CategoryWriteRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
