using System.Net.Http.Headers;
using System.Net.Http.Json;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;

namespace congThongTinf.FE.Services.Implementations;

public class MeService : IMeService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MeService> _logger;

    public MeService(HttpClient httpClient, ILogger<MeService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserDto?> GetCurrentUserDetailsAsync(string accessToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/me/details");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadFromJsonAsync<BackendEnvelope<UserDto>>();

            return response.IsSuccessStatusCode && body?.IsSuccess == true
                ? body.Value?.Data
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load current user details");
            return null;
        }
    }
}
