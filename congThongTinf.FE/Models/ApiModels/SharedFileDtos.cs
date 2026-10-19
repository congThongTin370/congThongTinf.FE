using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace congThongTinf.FE.Models.ApiModels;

/// <summary>
/// File chia sẻ từ /api/shared-files. Khớp SharedFileDto trong backend-for-frontend.md.
/// </summary>
public class SharedFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string? UploadedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsShared { get; set; }

    /// <summary>Chỉ có khi là chủ file và đang chia sẻ.</summary>
    public string? ShareToken { get; set; }

    /// <summary>Chỉ chủ file. URL metadata .../api/shared-files/links/{token}.</summary>
    public string? ShareUrl { get; set; }

    /// <summary>Chủ file: .../{id}/view. Người nhận: .../links/{token}/view.</summary>
    public string ViewUrl { get; set; } = string.Empty;

    /// <summary>Chủ file: .../{id}/download. Người nhận: .../links/{token}/download.</summary>
    public string DownloadUrl { get; set; } = string.Empty;
}

/// <summary>
/// Dữ liệu trang "Lưu trữ trực tuyến" (file của tôi).
/// </summary>
public class SharedFileListViewModel
{
    public List<SharedFileDto> Items { get; set; } = new();
    public string? Search { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SharedFileOpenViewModel
{
    public SharedFileDto? File { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SharedFileUploadViewModel
{
    [Required(ErrorMessage = "Chọn file cần tải lên.")]
    [Display(Name = "File")]
    public IFormFile? File { get; set; }
}

public class SharedFileWriteResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool SessionExpired { get; set; }
    public SharedFileDto? Data { get; set; }
}

/// <summary>
/// Kết quả đọc một file (chủ file hoặc qua link). Null Data + ErrorMessage khi lỗi.
/// </summary>
public class SharedFileReadResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool SessionExpired { get; set; }
    public SharedFileDto? Data { get; set; }
}
