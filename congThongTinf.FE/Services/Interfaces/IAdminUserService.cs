using congThongTinf.FE.Models;
using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Services.Interfaces;

public interface IAdminUserService
{
    Task<AdminUserListResult> GetUsersAsync(string accessToken, AdminUserQuery query);
    Task<AdminUserWriteResult> GetByIdAsync(string accessToken, Guid id);
    Task<AdminUserWriteResult> CreateAsync(string accessToken, UserFormViewModel model);
    Task<AdminUserWriteResult> UpdateAsync(string accessToken, Guid id, UserFormViewModel model);
    Task<AdminUserWriteResult> DeleteAsync(string accessToken, Guid id);
}
