using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Services.Interfaces;

public interface IStorageService
{
    Task<StorageUsageResult> GetUsageAsync(string accessToken);
}
