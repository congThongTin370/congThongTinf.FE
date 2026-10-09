using System.Net.Http.Headers;
using System.Net.Http.Json;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;

namespace congThongTinf.FE.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;

    public AuthService(HttpClient httpClient, ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        try
        {
            var request = new LoginRequestDto { Email = email, Password = password };
            using var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

            var body = await response.Content.ReadFromJsonAsync<BackendEnvelope<LoginDataDto>>();

            if (!response.IsSuccessStatusCode || body?.IsSuccess != true || body.Value?.Data == null)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = UserFacingErrors.ForLogin(body?.Error?.Code, body?.Error?.Message)
                };
            }

            var token = body.Value.Data.AccessToken;
            var user = body.Value.Data.User;

            // Role dạng chuỗi ("Admin"/"Journalist") lấy từ /api/me/role
            var role = await GetRoleAsync(token);
            if (string.IsNullOrEmpty(role))
            {
                role = MapRoleName(user?.RoleName);
            }

            if (string.IsNullOrEmpty(role))
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Tài khoản chưa được cấp quyền sử dụng. Vui lòng liên hệ quản trị viên."
                };
            }

            return new AuthResult
            {
                Success = true,
                AccessToken = token,
                UserId = user?.UserId ?? Guid.Empty,
                FullName = user?.FullName ?? string.Empty,
                Role = role
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login request failed");
            return new AuthResult
            {
                Success = false,
                ErrorMessage = UserFacingErrors.ForLogin(null, null)
            };
        }
    }

    private async Task<string?> GetRoleAsync(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "api/me/role");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var body = await response.Content.ReadFromJsonAsync<BackendEnvelope<MyRoleDto>>();
        return body?.Value?.Data?.Role;
    }

    /// <summary>
    /// Map roleName số của backend (0 = Admin, 1 = Journalist) sang chuỗi role.
    /// </summary>
    private static string? MapRoleName(int? roleName) => roleName switch
    {
        0 => "Admin",
        1 => "Journalist",
        _ => null
    };
}
