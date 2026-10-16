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

public class JournalistArticlesViewModel
{
    public List<NewsDto> Items { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? CategoryId { get; set; }
    public int? Status { get; set; }
    public string? Search { get; set; }

    /// <summary>0 = mới nhất. 1 = nhiều lượt xem nhất.</summary>
    public int SortBy { get; set; }

    public string? LoadError { get; set; }
    public bool HasFilter { get; set; }
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
    public int PostedCount { get; set; }
    public int ArchivedCount { get; set; }
    public bool StatusCountsFromCurrentPage { get; set; }
}

public static class NewsStatuses
{
    public const int Draft = 0;
    public const int Pending = 1;
    public const int Published = 2;
    public const int Archived = 3;
    public const string PostedLabel = "Đã đăng";

    // API vẫn trả Draft/Pending; phóng viên đăng là lên cổng, không duyệt.
    public static readonly (int Value, string Label)[] Options =
    {
        (Published, PostedLabel),
        (Archived, "Lưu trữ")
    };

    public static bool IsPosted(int status) =>
        status == Draft || status == Pending || status == Published;

    public static bool MatchesFilter(int status, int filter) =>
        filter == Archived ? status == Archived : IsPosted(status);

    public static string Label(int status) => status switch
    {
        Archived => "Lưu trữ",
        Draft or Pending or Published => PostedLabel,
        _ => "Không xác định"
    };

    public static string BadgeClass(int status) =>
        IsPosted(status) ? "admin-badge admin-badge-success" : "admin-badge admin-badge-muted";

    public static List<NewsDto> VisibleOnPortal(IEnumerable<NewsDto> news) =>
        news.Where(item => IsPosted(item.Status)).ToList();
}
