using System.ComponentModel.DataAnnotations;

namespace congThongTinf.FE.Models.ApiModels;

public class LeaderDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class LeaderFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Họ tên thủ trưởng là bắt buộc.")]
    [StringLength(256, ErrorMessage = "Họ tên không được vượt quá 256 ký tự.")]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Chức vụ là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Chức vụ không được vượt quá 200 ký tự.")]
    [Display(Name = "Chức vụ")]
    public string Position { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cấp bậc là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Cấp bậc không được vượt quá 100 ký tự.")]
    [Display(Name = "Cấp bậc")]
    public string Rank { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Đường dẫn ảnh không được vượt quá 1000 ký tự.")]
    [Display(Name = "Ảnh")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Thứ tự hiển thị")]
    public int DisplayOrder { get; set; }

    public bool IsEdit => Id.HasValue && Id.Value != Guid.Empty;
}

public class LeaderListViewModel
{
    public List<LeaderDto> Items { get; set; } = new();
    public string? LoadError { get; set; }
}

public class LeaderWriteResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool SessionExpired { get; set; }
    public LeaderDto? Data { get; set; }
}

public class LeaderListResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<LeaderDto> Items { get; set; } = new();
}
