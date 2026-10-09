using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Services.Interfaces;

/// <summary>
/// Lấy thông tin tài khoản đang đăng nhập từ api/me. Gọi backend bằng access token trong session.
/// </summary>
public interface IMeService
{
    Task<UserDto?> GetCurrentUserDetailsAsync(string accessToken);
}
