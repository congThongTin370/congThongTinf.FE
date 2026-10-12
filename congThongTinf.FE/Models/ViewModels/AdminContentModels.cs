using System.ComponentModel.DataAnnotations;
using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Models;

public class CategoryFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
    [Display(Name = "Tên danh mục")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Biểu tượng")]
    public string? Icon { get; set; }

    [Display(Name = "Thứ tự hiển thị")]
    public int DisplayOrder { get; set; }

    public bool IsEdit => Id.HasValue && Id.Value != Guid.Empty;
}

public class CategoryWriteResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool SessionExpired { get; set; }
    public CategoryDto? Data { get; set; }
}

public class AdminNewsListViewModel
{
    public List<NewsDto> Items { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? CategoryId { get; set; }
    public int? Status { get; set; }
    public string? Search { get; set; }
    public string? LoadError { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
    public int DraftCount { get; set; }
    public int PendingCount { get; set; }
    public int PublishedCount { get; set; }
    public int ArchivedCount { get; set; }
    public bool StatusCountsFromCurrentPage { get; set; }
}

public static class NewsStatuses
{
    public static readonly (int Value, string Label)[] Options =
    {
        (0, "Bản nháp"),
        (1, "Chờ duyệt"),
        (2, "Đã xuất bản"),
        (3, "Lưu trữ")
    };

    public static string Label(int status)
    {
        foreach (var option in Options)
        {
            if (option.Value == status)
            {
                return option.Label;
            }
        }

        return "Không xác định";
    }

    public static string BadgeClass(int status) => status switch
    {
        1 => "admin-badge admin-badge-warning",
        2 => "admin-badge admin-badge-success",
        _ => "admin-badge admin-badge-muted"
    };
}
