using System.Text.Json;
using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Utilities;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Thực hiện GET request và deserialize kết quả thành generic type T
    /// </summary>
    public async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        try
        {
            var url = endpoint;
            
            // Xây dựng query string nếu có tham số
            if (queryParams != null && queryParams.Count > 0)
            {
                var queryString = string.Join("&", queryParams.Select(kvp => 
                    $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                url = $"{endpoint}?{queryString}";
            }

            _logger.LogInformation($"GET request: {url}");
            
            var response = await _httpClient.GetAsync(url);
            
            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Response status: {response.StatusCode}");
            _logger.LogInformation($"Response content: {content}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"API returned status {response.StatusCode}");
            }

            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            var result = JsonSerializer.Deserialize<T>(content, options);
            
            return result!;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"HTTP request error: {ex.Message}");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"JSON deserialization error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Thực hiện POST request
    /// </summary>
    public async Task<T> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            _logger.LogInformation($"POST request to: {endpoint}");
            
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<T>(responseContent, options);
            
            return result!;
        }
        catch (Exception ex)
        {
            _logger.LogError($"POST request error: {ex.Message}");
            throw;
        }
    }
}
