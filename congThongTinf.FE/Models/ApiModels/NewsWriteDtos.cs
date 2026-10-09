using System.ComponentModel.DataAnnotations;

namespace congThongTinf.FE.Models.ApiModels;

/// <summary>
/// Body gửi lên POST /api/news (tạo bài).
/// </summary>
public class CreateNewsRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public Guid CategoryId { get; set; }
}

/// <summary>
/// Body gửi lên PUT /api/news/{id} (sửa bài). Field null hoặc không gửi thì backend giữ nguyên.
/// </summary>
public class UpdateNewsRequestDto
{
    public string? Title { get; set; }
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? CoverImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
}

/// <summary>
/// Form dùng chung cho tạo/sửa bài trên UI.
/// </summary>
public class NewsFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Nhập tiêu đề bài viết.")]
    [StringLength(300, ErrorMessage = "Tiêu đề không quá 300 ký tự.")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Tóm tắt không quá 1000 ký tự.")]
    [Display(Name = "Tóm tắt")]
    public string? Summary { get; set; }

    [Required(ErrorMessage = "Nhập nội dung bài viết.")]
    [Display(Name = "Nội dung")]
    public string Content { get; set; } = string.Empty;

    [Url(ErrorMessage = "Ảnh bìa phải là URL hợp lệ.")]
    [Display(Name = "Ảnh bìa (URL)")]
    public string? CoverImageUrl { get; set; }

    [Required(ErrorMessage = "Chọn chuyên mục cho bài viết.")]
    [Display(Name = "Chuyên mục")]
    public Guid? CategoryId { get; set; }

    public string? StatusLabel { get; set; }
}
