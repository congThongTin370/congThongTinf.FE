using System.Net.Http.Json;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;

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
}
