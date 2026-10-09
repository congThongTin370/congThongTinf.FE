namespace congThongTinf.FE.Services.Interfaces;

public class AuthResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password);
}
