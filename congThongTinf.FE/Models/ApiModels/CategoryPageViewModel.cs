namespace congThongTinf.FE.Models.ApiModels;

/// <summary>
/// Dữ liệu cho trang danh sách bài báo theo chuyên mục.
/// </summary>
public class CategoryPageViewModel
{
    public CategoryDto Category { get; set; } = new();
    public List<NewsDto> News { get; set; } = new();
}
