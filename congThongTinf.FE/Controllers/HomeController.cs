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

        public HomeController(INewsService newsService, ILogger<HomeController> logger, IConfiguration configuration)
        {
            _newsService = newsService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(string? categoryIds)
        {
            try
            {
                var categoryFilter = ParseCategoryIds(categoryIds);
                _logger.LogInformation("Loading news for home page");
                var newsList = await _newsService.GetAllNewsAsync(
                    categoryFilter == null ? null : new NewsQueryDto { CategoryIds = categoryFilter });

                _logger.LogInformation($"Fetched {newsList.Count} news items");
                
                // Sắp xếp theo ngày tạo mới nhất trước
                var sortedNews = newsList.OrderByDescending(n => n.CreatedAt).ToList();

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
                _logger.LogError(ex, "Error loading news: {Message}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                // Trả về view với danh sách trống nếu có lỗi
                return View(new List<NewsDto>());
            }
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

        public IActionResult Documents()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
