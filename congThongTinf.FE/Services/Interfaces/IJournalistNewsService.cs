using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Services.Interfaces;

public class NewsWriteResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public NewsDto? Data { get; set; }
}

public class NewsImageUploadResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Url { get; set; }
}

/// <summary>
/// Thao tác CRUD bài báo của phóng viên. Gọi backend bằng access token trong session.
/// </summary>
public interface IJournalistNewsService
{
    Task<List<NewsDto>> GetMyNewsAsync(string accessToken, Guid authorId, NewsQueryDto? query = null);
    Task<NewsDto?> GetNewsByIdAsync(Guid id);
    Task<NewsWriteResult> CreateAsync(string accessToken, NewsWriteRequestDto request);
    Task<NewsWriteResult> UpdateAsync(string accessToken, Guid id, NewsWriteRequestDto request);
    Task<NewsWriteResult> DeleteAsync(string accessToken, Guid id);
    Task<NewsImageUploadResult> UploadContentImageAsync(string accessToken, IFormFile file);
}
