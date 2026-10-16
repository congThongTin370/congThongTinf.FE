using System.Text.Json;
using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Utilities;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Thực hiện GET request và deserialize kết quả thành generic type T
    /// </summary>
    public async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParams = null)
    {
        var url = endpoint;

        // Xây dựng query string nếu có tham số
        if (queryParams != null && queryParams.Count > 0)
        {
            var queryString = string.Join("&", queryParams.Select(kvp =>
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            url = $"{endpoint}?{queryString}";
        }

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        return JsonSerializer.Deserialize<T>(content, options)!;
    }

    /// <summary>
    /// Thực hiện POST request
    /// </summary>
    public async Task<T> PostAsync<T>(string endpoint, object? data = null)
    {
        HttpContent? content = null;
        if (data != null)
        {
            var json = JsonSerializer.Serialize(data);
            content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        var response = await _httpClient.PostAsync(endpoint, content);
        var responseContent = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        return JsonSerializer.Deserialize<T>(responseContent, options)!;
    }
}
