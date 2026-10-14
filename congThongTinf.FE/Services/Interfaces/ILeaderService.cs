using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Services.Interfaces;

/// <summary>
/// Thủ trưởng trên dải Ban Chỉ huy. GET public. Tạo, sửa, xóa cần token Admin.
/// </summary>
public interface ILeaderService
{
    Task<LeaderListResult> GetLeadersAsync();
    Task<LeaderWriteResult> GetByIdAsync(Guid id);
    Task<LeaderWriteResult> CreateAsync(string accessToken, LeaderFormViewModel model);
    Task<LeaderWriteResult> UpdateAsync(string accessToken, Guid id, LeaderFormViewModel model);
    Task<LeaderWriteResult> DeleteAsync(string accessToken, Guid id);
}
