using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.ViewComponents
{
    /// <summary>
    /// Danh sách chuyên mục trong dropdown "Tin tức" của navbar (API công khai).
    /// </summary>
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public CategoryMenuViewComponent(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            return View(categories);
        }
    }
}
