using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;

namespace congThongTinf.FE.Services.Implementations;

public class LeaderService : ILeaderService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LeaderService> _logger;

    public LeaderService(HttpClient httpClient, ILogger<LeaderService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<LeaderListResult> GetLeadersAsync()
    {
        try
        {
            using var response = await _httpClient.GetAsync("api/leaders");
            var body = await ReadEnvelopeAsync<List<LeaderDto>>(response);
            if (!response.IsSuccessStatusCode || body?.IsSuccess != true)
            {
                return new LeaderListResult
                {
                    ErrorMessage = UserFacingErrors.ForLeaders((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message)
                };
            }

            return new LeaderListResult
            {
                Success = true,
                Items = body.Value?.Data ?? new List<LeaderDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load leaders");
            return new LeaderListResult { ErrorMessage = UserFacingErrors.TryAgain };
        }
    }

    public async Task<LeaderWriteResult> GetByIdAsync(Guid id)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"api/leaders/{id}");
            return await ReadLeaderAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load leader {LeaderId}", id);
            return Failure(0, null, null);
        }
    }

    public Task<LeaderWriteResult> CreateAsync(string accessToken, LeaderFormViewModel model)
    {
        return SendAsync(HttpMethod.Post, "api/leaders", accessToken, ToRequest(model));
    }

    public Task<LeaderWriteResult> UpdateAsync(string accessToken, Guid id, LeaderFormViewModel model)
    {
        return SendAsync(HttpMethod.Put, $"api/leaders/{id}", accessToken, ToRequest(model));
    }

    public async Task<LeaderWriteResult> DeleteAsync(string accessToken, Guid id)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/leaders/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(request);
            return await ReadDeleteAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete leader {LeaderId}", id);
            return Failure(0, null, null);
        }
    }

    private async Task<LeaderWriteResult> SendAsync(HttpMethod method, string url, string accessToken, LeaderWriteRequest payload)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            return await ReadLeaderAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Leader request {Method} {Url} failed", method, url);
            return Failure(0, null, null);
        }
    }

    private static async Task<LeaderWriteResult> ReadLeaderAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return SessionExpiredResult();
        }

        var body = await ReadEnvelopeAsync<LeaderDto>(response);
        if (!response.IsSuccessStatusCode || body?.IsSuccess != true || body.Value?.Data == null)
        {
            return Failure((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message);
        }

        return new LeaderWriteResult { Success = true, Data = body.Value.Data };
    }

    private static async Task<LeaderWriteResult> ReadDeleteAsync(HttpResponseMessage response)
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

        return new LeaderWriteResult { Success = true };
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

    private static LeaderWriteRequest ToRequest(LeaderFormViewModel model)
    {
        return new LeaderWriteRequest
        {
            FullName = model.FullName.Trim(),
            Position = model.Position.Trim(),
            Rank = model.Rank.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? string.Empty : model.ImageUrl.Trim(),
            DisplayOrder = model.DisplayOrder
        };
    }

    private static LeaderWriteResult SessionExpiredResult() => new()
    {
        Success = false,
        SessionExpired = true,
        ErrorMessage = UserFacingErrors.SessionExpired
    };

    private static LeaderWriteResult Failure(int statusCode, string? code, string? message) => new()
    {
        Success = false,
        ErrorMessage = UserFacingErrors.ForLeaders(statusCode, code, message)
    };

    private sealed class LeaderWriteRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Rank { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}
