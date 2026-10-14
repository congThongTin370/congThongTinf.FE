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
        private readonly IBannerService _bannerService;

        public HomeController(
            INewsService newsService,
            ILogger<HomeController> logger,
            IConfiguration configuration,
            ICategoryService categoryService,
            IDocumentService documentService,
            IBannerService bannerService)
        {
            _newsService = newsService;
            _logger = logger;
            _configuration = configuration;
            _categoryService = categoryService;
            _documentService = documentService;
            _bannerService = bannerService;
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
                
                // API không lọc Draft; bài phóng viên đăng hiện ngay. Chỉ ẩn bài lưu trữ.
                var sortedNews = NewsStatuses.VisibleOnPortal(newsList)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();

                // Ảnh override cho tin mới nhất (nếu được cấu hình trong appsettings)
                ViewData["LatestNewsCoverOverride"] = _configuration["HomeSettings:LatestNewsCoverImageOverride"];
                ViewData["HomeBanners"] = await LoadHomeBannersAsync();

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
                ViewData["HomeBanners"] = new List<BannerDto>();
                return View(new List<NewsDto>());
            }
        }

        private async Task<List<BannerDto>> LoadHomeBannersAsync()
        {
            var listed = await _bannerService.GetBannersAsync(string.Empty, new BannerQuery
            {
                PageIndex = 0,
                PageSize = 10,
                IsActive = true
            });

            if (!listed.Success || listed.Items.Count == 0)
            {
                return new List<BannerDto>();
            }

            var banners = new List<BannerDto>();
            foreach (var item in listed.Items)
            {
                var detail = await _bannerService.GetByIdAsync(string.Empty, item.Id);
                if (detail.Success && detail.Data != null && !string.IsNullOrWhiteSpace(detail.Data.ImageUrl))
                {
                    banners.Add(detail.Data);
                }
            }

            return banners;
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

        public async Task<IActionResult> Documents()
        {
            // pageIndex = 0: API trả toàn bộ danh sách, không phân trang
            var documents = await _documentService.GetDocumentsAsync(new DocumentQuery
            {
                PageIndex = 0,
                PageSize = 10
            });
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
