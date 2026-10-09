namespace congThongTinf.FE.Models.ApiModels;

public class NewsQueryDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<Guid>? CategoryIds { get; set; }
}
