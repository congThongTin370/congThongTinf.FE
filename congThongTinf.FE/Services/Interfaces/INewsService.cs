using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Services.Interfaces;

public interface INewsService
{
    Task<List<NewsDto>> GetAllNewsAsync(NewsQueryDto? query = null);
    Task<NewsDto> GetNewsByIdAsync(Guid id);
}
