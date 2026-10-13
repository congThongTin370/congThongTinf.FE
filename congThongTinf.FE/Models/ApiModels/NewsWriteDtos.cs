using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace congThongTinf.FE.Models.ApiModels;

/// <summary>
/// Form gửi lên POST /api/news và PUT /api/news/{id} (multipart). Không gửi coverImageUrl.
/// </summary>
public class NewsWriteRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public IFormFile? CoverImage { get; set; }
}

/// <summary>
/// Form dùng chung cho tạo/sửa bài trên UI.
/// </summary>
public class NewsFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Nhập tiêu đề bài viết.")]
    [StringLength(300, ErrorMessage = "Tiêu đề không quá 300 ký tự.")]
    [Display(Name = "Tiêu đề bài viết")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Mô tả ngắn không quá 1000 ký tự.")]
    [Display(Name = "Mô tả ngắn")]
    public string? Summary { get; set; }

    [Required(ErrorMessage = "Nhập nội dung bài viết.")]
    [Display(Name = "Nội dung")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Ảnh đại diện")]
    public string? CoverImageUrl { get; set; }

    public IFormFile? CoverImage { get; set; }

    [Required(ErrorMessage = "Chọn chuyên mục cho bài viết.")]
    [Display(Name = "Chuyên mục")]
    public Guid? CategoryId { get; set; }

    public string? StatusLabel { get; set; }

    public string? CategoryName { get; set; }

    [BindNever]
    public string AuthorName { get; set; } = string.Empty;

    [BindNever]
    public List<CategoryDto> Categories { get; set; } = new();
}
