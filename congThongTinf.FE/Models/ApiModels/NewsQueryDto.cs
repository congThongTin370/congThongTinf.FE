namespace congThongTinf.FE.Models.ApiModels;

public class NewsQueryDto
{
    /// <summary>
    /// Trang bắt đầu từ 1. 0 = lấy toàn bộ, bỏ Skip/Take phía API.
    /// </summary>
    public int? PageIndex { get; set; }

    public int? PageSize { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<Guid>? CategoryIds { get; set; }

    /// <summary>Lọc title chứa chuỗi, không phân biệt hoa thường.</summary>
    public string? Search { get; set; }
}
