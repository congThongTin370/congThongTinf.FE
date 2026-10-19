using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Services.Interfaces;

/// <summary>
/// Module file chia sẻ (/api/shared-files). Mọi thao tác cần JWT.
/// </summary>
public interface ISharedFileService
{
    Task<SharedFileListViewModel> GetMyFilesAsync(string accessToken, int pageIndex, int pageSize, string? search);
    Task<SharedFileWriteResult> UploadAsync(string accessToken, IFormFile file);
    Task<SharedFileWriteResult> CreateShareLinkAsync(string accessToken, Guid id);
    Task<SharedFileWriteResult> RevokeShareLinkAsync(string accessToken, Guid id);
    Task<SharedFileReadResult> GetByLinkTokenAsync(string accessToken, string token);

    /// <summary>
    /// Lấy bytes file từ viewUrl/downloadUrl của backend (có JWT). Trả null nếu lỗi.
    /// </summary>
    Task<SharedFileStreamResult> OpenStreamAsync(string accessToken, string backendUrl);
}

public class SharedFileStreamResult
{
    public bool Success { get; set; }
    public bool SessionExpired { get; set; }
    public string? ErrorMessage { get; set; }
    public Stream? Content { get; set; }
    public string ContentType { get; set; } = "application/octet-stream";
    public string? FileName { get; set; }
}
