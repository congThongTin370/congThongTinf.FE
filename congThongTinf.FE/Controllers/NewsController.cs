using congThongTinf.FE.Models;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.Controllers
{
    /// <summary>
    /// Controller để test API communication với backend
    /// </summary>
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;
        private readonly IJournalistNewsService _journalistNewsService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(
            INewsService newsService,
            IJournalistNewsService journalistNewsService,
            ILogger<NewsController> logger)
        {
            _newsService = newsService;
            _journalistNewsService = journalistNewsService;
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị danh sách tất cả các bài báo
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading all news...");
                var news = await _newsService.GetAllNewsAsync();
                return View(NewsStatuses.VisibleOnPortal(news));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading news: {ex.Message}");
                ViewBag.Error = UserFacingErrors.LoadNewsFailed;
                return View(new List<NewsDto>());
            }
        }

        /// <summary>
        /// Hiển thị danh sách bài báo với các bộ lọc
        /// </summary>
        public async Task<IActionResult> Filter(DateTime? fromDate, DateTime? toDate, string? categoryIds)
        {
            try
            {
                var query = new NewsQueryDto
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    CategoryIds = ParseCategoryIds(categoryIds)
                };

                _logger.LogInformation($"Filtering news with parameters - From: {fromDate}, To: {toDate}, Categories: {categoryIds}");
                var news = await _newsService.GetAllNewsAsync(query);

                ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
                ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
                ViewBag.CategoryIds = categoryIds;

                return View("Index", NewsStatuses.VisibleOnPortal(news));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error filtering news: {ex.Message}");
                ViewBag.Error = UserFacingErrors.LoadNewsFailed;
                return View("Index", new List<NewsDto>());
            }
        }

        /// <summary>
        /// Chi tiết một bài báo. Công khai cho mọi role (GET /api/news/{id} không cần token).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var news = await _journalistNewsService.GetNewsByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            var viewCount = await _newsService.IncrementViewCountAsync(id);
            if (viewCount.HasValue)
            {
                news.ViewCount = viewCount.Value;
            }

            ViewData["Title"] = news.Title;
            return View(news);
        }

        /// <summary>
        /// Parse category IDs từ string
        /// </summary>
        private List<Guid>? ParseCategoryIds(string? categoryIds)
        {
            if (string.IsNullOrEmpty(categoryIds))
                return null;

            var ids = categoryIds.Split(',')
                .Where(id => Guid.TryParse(id.Trim(), out _))
                .Select(id => Guid.Parse(id.Trim()))
                .ToList();

            return ids.Count > 0 ? ids : null;
        }
    }
}
