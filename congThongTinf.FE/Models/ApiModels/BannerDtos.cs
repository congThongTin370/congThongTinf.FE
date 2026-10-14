using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace congThongTinf.FE.Models.ApiModels;

public class BannerDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public Guid UploadedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BannerListViewModel
{
    public List<BannerDto> Items { get; set; } = new();
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
    public string? LoadError { get; set; }
}

public class BannerFormViewModel
{
    public Guid? Id { get; set; }

    [Display(Name = "Tiêu đề")]
    public string? Title { get; set; }

    [Display(Name = "Thứ tự hiển thị")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Đang hiện")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Ảnh banner")]
    public IFormFile? File { get; set; }

    public string? CurrentImageUrl { get; set; }
    public string? CurrentFileName { get; set; }

    public bool IsEdit => Id.HasValue && Id.Value != Guid.Empty;
}

public class BannerQuery
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}

public class BannerListResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool SessionExpired { get; set; }
    public List<BannerDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}

public class BannerWriteResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool SessionExpired { get; set; }
    public BannerDto? Data { get; set; }
}
