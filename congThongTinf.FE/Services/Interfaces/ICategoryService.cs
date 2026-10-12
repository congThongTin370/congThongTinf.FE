using congThongTinf.FE.Models;
using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Services.Interfaces;

/// <summary>
/// Đọc danh mục bài báo đang hoạt động từ api/categories.
/// Ghi (tạo, sửa, xóa mềm) cần token Admin.
/// </summary>
public interface ICategoryService
{
    Task<List<CategoryDto>> GetActiveCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
    Task<CategoryWriteResult> CreateAsync(string accessToken, CategoryFormViewModel model);
    Task<CategoryWriteResult> UpdateAsync(string accessToken, Guid id, CategoryFormViewModel model);
    Task<CategoryWriteResult> DeleteAsync(string accessToken, Guid id);
}
