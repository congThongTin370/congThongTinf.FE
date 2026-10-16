using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace congThongTinf.FE.Models.ApiModels;

/// <summary>
/// Tài liệu nội bộ từ GET/POST /api/documents.
/// </summary>
public class DocumentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Category { get; set; }
    public Guid UploadedByUserId { get; set; }
    public string? UploadedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DocumentListViewModel
{
    public List<DocumentDto> Items { get; set; } = new();
    public string? Search { get; set; }
    public int? Category { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public StorageUsageResult Storage { get; set; } = new();

    public bool HasPrevious => PageIndex > 1;
    public bool HasNext => Items.Count >= PageSize;
}

public class DocumentUploadViewModel
{
    [Required(ErrorMessage = "Chọn tệp tài liệu.")]
    [Display(Name = "Tệp tài liệu")]
    public IFormFile? File { get; set; }

    [StringLength(300, ErrorMessage = "Tiêu đề không quá 300 ký tự.")]
    [Display(Name = "Tiêu đề")]
    public string? Title { get; set; }

    [StringLength(1000, ErrorMessage = "Mô tả không quá 1000 ký tự.")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Danh mục")]
    public int Category { get; set; }
}

public static class DocumentCategories
{
    public const int General = 0;
    public const int Policy = 1;
    public const int Procedure = 2;
    public const int Report = 3;
    public const int Guideline = 4;

    public static readonly (int Value, string Label)[] Options =
    {
        (General, "Chung"),
        (Policy, "Chính sách"),
        (Procedure, "Quy trình"),
        (Report, "Báo cáo"),
        (Guideline, "Hướng dẫn")
    };

    public static string Label(int category)
    {
        foreach (var option in Options)
        {
            if (option.Value == category)
            {
                return option.Label;
            }
        }

        return "Không xác định";
    }
}

public class DocumentWriteResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool SessionExpired { get; set; }
    public DocumentDto? Data { get; set; }
}
