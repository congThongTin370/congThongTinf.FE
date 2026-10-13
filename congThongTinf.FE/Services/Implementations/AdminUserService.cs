using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using congThongTinf.FE.Models;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;

namespace congThongTinf.FE.Services.Implementations;

public class AdminUserService : IAdminUserService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<AdminUserService> _logger;

    public AdminUserService(HttpClient httpClient, ILogger<AdminUserService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AdminUserListResult> GetUsersAsync(string accessToken, AdminUserQuery query)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BuildListUrl(query));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return new AdminUserListResult
                {
                    Success = false,
                    SessionExpired = true,
                    ErrorMessage = UserFacingErrors.SessionExpired
                };
            }

            var body = await ReadEnvelopeAsync<PagedData<UserDto>>(response);
            if (!response.IsSuccessStatusCode || body?.IsSuccess != true)
            {
                return new AdminUserListResult
                {
                    Success = false,
                    ErrorMessage = UserFacingErrors.ForUsers((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message)
                };
            }

            return new AdminUserListResult
            {
                Success = true,
                Items = body.Value?.Data?.Items ?? new List<UserDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load users");
            return new AdminUserListResult
            {
                Success = false,
                ErrorMessage = UserFacingErrors.TryAgain
            };
        }
    }

    public async Task<AdminUserWriteResult> GetByIdAsync(string accessToken, Guid id)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/admin/users/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(request);
            return await ReadUserAsync(response, requireData: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load user {UserId}", id);
            return Failure(0, null, null);
        }
    }

    public Task<AdminUserWriteResult> CreateAsync(string accessToken, UserFormViewModel model)
    {
        var payload = new CreateUserRequest
        {
            Username = model.Username.Trim(),
            Email = model.Email.Trim(),
            Password = model.Password ?? string.Empty,
            FullName = model.FullName.Trim(),
            RoleName = model.RoleName
        };

        return SendAsync(HttpMethod.Post, "api/admin/users", accessToken, payload);
    }

    public Task<AdminUserWriteResult> UpdateAsync(string accessToken, Guid id, UserFormViewModel model)
    {
        var payload = new UpdateUserRequest
        {
            Username = model.Username.Trim(),
            Email = model.Email.Trim(),
            FullName = model.FullName.Trim(),
            RoleName = model.RoleName,
            IsActive = model.IsActive,
            Password = string.IsNullOrWhiteSpace(model.Password) ? null : model.Password
        };

        return SendAsync(HttpMethod.Put, $"api/admin/users/{id}", accessToken, payload);
    }

    public async Task<AdminUserWriteResult> DeleteAsync(string accessToken, Guid id)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/admin/users/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return SessionExpiredResult();
            }

            var body = await ReadEnvelopeAsync<bool>(response);
            if (!response.IsSuccessStatusCode || body?.IsSuccess != true)
            {
                return Failure((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message);
            }

            return new AdminUserWriteResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user {UserId}", id);
            return Failure(0, null, null);
        }
    }

    private async Task<AdminUserWriteResult> SendAsync<TBody>(HttpMethod method, string url, string accessToken, TBody payload)
    {
        try
        {
            using var request = new HttpRequestMessage(method, url)
            {
                Content = JsonContent.Create(payload, options: JsonOptions)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var response = await _httpClient.SendAsync(request);
            return await ReadUserAsync(response, requireData: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "User request {Method} {Url} failed", method, url);
            return Failure(0, null, null);
        }
    }

    private static async Task<AdminUserWriteResult> ReadUserAsync(HttpResponseMessage response, bool requireData)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return SessionExpiredResult();
        }

        var body = await ReadEnvelopeAsync<UserDto>(response);
        if (!response.IsSuccessStatusCode || body?.IsSuccess != true || (requireData && body.Value?.Data == null))
        {
            return Failure((int)response.StatusCode, body?.Error?.Code, body?.Error?.Message);
        }

        return new AdminUserWriteResult { Success = true, Data = body.Value?.Data };
    }

    private static async Task<BackendEnvelope<T>?> ReadEnvelopeAsync<T>(HttpResponseMessage response)
    {
        var responseText = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(responseText))
        {
            return null;
        }

        return JsonSerializer.Deserialize<BackendEnvelope<T>>(responseText, JsonOptions);
    }

    private static string BuildListUrl(AdminUserQuery query)
    {
        var parts = new List<string>
        {
            $"pageIndex={query.PageIndex}",
            $"pageSize={query.PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            parts.Add($"search={Uri.EscapeDataString(query.Search.Trim())}");
        }

        if (query.IsActive.HasValue)
        {
            parts.Add($"isActive={query.IsActive.Value.ToString().ToLowerInvariant()}");
        }

        if (query.RoleName.HasValue)
        {
            parts.Add($"roleName={query.RoleName.Value}");
        }

        return "api/admin/users?" + string.Join("&", parts);
    }

    private static AdminUserWriteResult SessionExpiredResult() => new()
    {
        Success = false,
        SessionExpired = true,
        ErrorMessage = UserFacingErrors.SessionExpired
    };

    private static AdminUserWriteResult Failure(int statusCode, string? code, string? message) => new()
    {
        Success = false,
        ErrorMessage = UserFacingErrors.ForUsers(statusCode, code, message)
    };

    private sealed class CreateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int RoleName { get; set; }
    }

    private sealed class UpdateUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}
