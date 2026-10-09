namespace congThongTinf.FE.Models.ApiModels;

public class NewsDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Summary { get; set; }
    public string? CoverImageUrl { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public int Status { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int ViewCount { get; set; }
}
