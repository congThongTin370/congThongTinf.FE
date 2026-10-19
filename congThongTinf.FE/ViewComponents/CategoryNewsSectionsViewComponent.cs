using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.ViewComponents;

/// <summary>
/// Các khối tin theo chuyên mục trên trang chủ. Nhận danh sách tin trang chủ đã tải,
/// gom theo CategoryId. Chuyên mục chưa có bài vẫn hiện tiêu đề.
/// </summary>
public class CategoryNewsSectionsViewComponent : ViewComponent
{
    private const int NewsPerCategory = 4;

    private readonly ICategoryService _categoryService;

    public CategoryNewsSectionsViewComponent(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IViewComponentResult> InvokeAsync(List<NewsDto>? news)
    {
        var categories = await _categoryService.GetActiveCategoriesAsync();
        if (categories.Count == 0)
        {
            return Content(string.Empty);
        }

        var newsByCategory = (news ?? new List<NewsDto>())
            .Where(n => n.CategoryId.HasValue)
            .GroupBy(n => n.CategoryId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(n => n.CreatedAt).Take(NewsPerCategory).ToList());

        var sections = categories
            .Select(c => new CategoryPageViewModel
            {
                Category = c,
                News = newsByCategory.TryGetValue(c.Id, out var items) ? items : new List<NewsDto>()
            })
            .ToList();

        return View(sections);
    }
}
