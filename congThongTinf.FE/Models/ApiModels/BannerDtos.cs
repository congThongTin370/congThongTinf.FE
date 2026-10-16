using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace congThongTinf.FE.Models.ApiModels;

public static class BannerPositions
{
    public const int Header = 0;
    public const int HomeMid = 1;

    public static readonly (int Value, string Label)[] Options =
    [
        (Header, "Header"),
        (HomeMid, "Dải giữa trang chủ")
    ];

    public static bool IsValid(int position) => position is Header or HomeMid;

    public static string Label(int position) => position switch
    {
        Header => "Header",
        HomeMid => "Dải giữa trang chủ",
        _ => "Không xác định"
    };
}

public class BannerDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Position { get; set; }
    public bool IsActive { get; set; }
    public Guid UploadedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class BannerSlotViewModel
{
    public BannerDto Banner { get; set; } = new();
    public int Position { get; set; }
}

public class BannerFileDto
{
    public string Key { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime? LastModified { get; set; }
}

public class BannerFileListResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public bool SessionExpired { get; set; }
    public List<BannerFileDto> Items { get; set; } = new();
}

public class BannerListViewModel
{
    public List<BannerDto> Items { get; set; } = new();
    public List<BannerFileDto> Files { get; set; } = new();
    public string? FilesError { get; set; }
    public string? Search { get; set; }
    public int? Position { get; set; }
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

    [Display(Name = "Link khi bấm")]
    [StringLength(1000, ErrorMessage = "Link banner không được vượt quá 1000 ký tự.")]
    public string? LinkUrl { get; set; }

    [Display(Name = "Vị trí")]
    [Range(BannerPositions.Header, BannerPositions.HomeMid, ErrorMessage = "Vị trí banner không hợp lệ.")]
    public int Position { get; set; } = BannerPositions.Header;

    [Display(Name = "Đang hiện")]
    public bool IsActive { get; set; }

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
    public int? Position { get; set; }
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
