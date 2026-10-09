using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Services.Interfaces;

/// <summary>
/// Đọc danh mục bài báo đang hoạt động từ api/categories (công khai).
/// </summary>
public interface ICategoryService
{
    Task<List<CategoryDto>> GetActiveCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(Guid id);
}
