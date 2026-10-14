using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Services.Interfaces;

public interface IBannerService
{
    Task<BannerListResult> GetBannersAsync(string accessToken, BannerQuery query);
    Task<BannerWriteResult> GetByIdAsync(string accessToken, Guid id);
    Task<BannerWriteResult> CreateAsync(string accessToken, BannerFormViewModel model);
    Task<BannerWriteResult> UpdateAsync(string accessToken, Guid id, BannerFormViewModel model);
    Task<BannerWriteResult> SetActiveAsync(string accessToken, Guid id);
    Task<BannerWriteResult> DeleteAsync(string accessToken, Guid id);
}
