using congThongTinf.FE.Models;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace congThongTinf.FE.Controllers
{
    public class HomeController : Controller
    {
        private readonly INewsService _newsService;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICategoryService _categoryService;
        private readonly IDocumentService _documentService;

        public HomeController(
            INewsService newsService,
            ILogger<HomeController> logger,
            IConfiguration configuration,
            ICategoryService categoryService,
            IDocumentService documentService)
        {
            _newsService = newsService;
            _logger = logger;
            _configuration = configuration;
            _categoryService = categoryService;
            _documentService = documentService;
        }

        public async Task<IActionResult> Index(string? categoryIds, string? search)
        {
            try
            {
                var categoryFilter = ParseCategoryIds(categoryIds);
                var term = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
                ViewData["NewsSearch"] = term;
                var newsList = await _newsService.GetAllNewsAsync(
                    categoryFilter == null && term == null
                        ? null
                        : new NewsQueryDto
                        {
                            PageIndex = 0,
                            CategoryIds = categoryFilter,
                            Search = term
                        });

                // API không lọc Draft; bài phóng viên đăng hiện ngay. Chỉ ẩn bài lưu trữ.
                var sortedNews = NewsStatuses.VisibleOnPortal(newsList)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();

                // Ảnh override cho tin mới nhất (nếu được cấu hình trong appsettings)
                ViewData["LatestNewsCoverOverride"] = _configuration["HomeSettings:LatestNewsCoverImageOverride"];

                // Đang lọc theo chuyên mục: hiển thị tên chuyên mục ở tiêu đề trang
                if (categoryFilter != null && sortedNews.Count > 0)
                {
                    ViewData["CategoryFilterName"] = sortedNews
                        .Select(n => n.CategoryName)
                        .FirstOrDefault(n => !string.IsNullOrEmpty(n));
                }
                ViewData["CategoryFilterActive"] = categoryFilter != null;

                return View(sortedNews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading news for home page");
                // Trả về view với danh sách trống nếu có lỗi
                return View(new List<NewsDto>());
            }
        }

        /// <summary>
        /// Trang danh sách bài báo của một chuyên mục. Giữ layout header như trang chủ.
        /// </summary>
        public async Task<IActionResult> Category(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var news = await _newsService.GetAllNewsAsync(
                new NewsQueryDto { CategoryIds = new List<Guid> { id } });

            ViewData["Title"] = category.Name;
            return View(new CategoryPageViewModel
            {
                Category = category,
                News = NewsStatuses.VisibleOnPortal(news)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList()
            });
        }

        /// <summary>
        /// Parse danh sách Guid từ query "categoryIds=a,b" (bỏ qua giá trị không hợp lệ).
        /// </summary>
        private static List<Guid>? ParseCategoryIds(string? categoryIds)
        {
            if (string.IsNullOrWhiteSpace(categoryIds))
                return null;

            var ids = categoryIds.Split(',')
                .Select(id => Guid.TryParse(id.Trim(), out var g) ? g : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .ToList();

            return ids.Count > 0 ? ids : null;
        }

        public IActionResult Introduction()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Documents(string? search, int? category, string? sort)
        {
            // pageIndex = 0: API trả toàn bộ danh sách, không phân trang
            var documents = await _documentService.GetDocumentsAsync(new DocumentQuery
            {
                PageIndex = 0,
                PageSize = 10
            });

            if (category.HasValue && DocumentCategories.Options.All(o => o.Value != category.Value))
            {
                category = null;
            }

            ViewData["DocumentSearch"] = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
            ViewData["DocumentCategory"] = category;
            ViewData["DocumentSort"] = sort is "name" or "size" ? sort : "date";
            return View(documents);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Status(int code)
        {
            var (title, message) = code switch
            {
                404 => ("Không tìm thấy trang", "Trang bạn mở không có trên cổng thông tin."),
                403 => ("Không xem được nội dung", "Bạn không có quyền xem nội dung này."),
                _ => ("Có lỗi xảy ra", "Hệ thống đang gặp sự cố. Vui lòng thử lại sau.")
            };

            ViewData["Title"] = title;
            ViewData["StatusMessage"] = message;
            Response.StatusCode = code > 0 ? code : StatusCodes.Status500InternalServerError;
            return View();
        }
    }
}
