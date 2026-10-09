using System.Text.Json.Serialization;

namespace congThongTinf.FE.Models.ApiModels;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginDataDto
{
    public string AccessToken { get; set; } = string.Empty;
    public UserDto? User { get; set; }
}

public class UserDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int RoleName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MyRoleDto
{
    public string Role { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Envelope lỗi/thành công của backend: ApiResult. Dùng cho login và me/role.
/// </summary>
public class BackendEnvelope<T>
{
    public bool IsSuccess { get; set; }
    public bool IsFailure { get; set; }
    public BackendValue<T>? Value { get; set; }
    public BackendError? Error { get; set; }
}

public class BackendValue<T>
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

public class BackendError
{
    public string? Code { get; set; }
    public string? Message { get; set; }
}
