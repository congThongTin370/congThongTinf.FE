using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.Controllers
{
    /// <summary>
    /// Controller để test API communication với backend
    /// </summary>
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;
        private readonly ILogger<NewsController> _logger;

        public NewsController(INewsService newsService, ILogger<NewsController> logger)
        {
            _newsService = newsService;
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
                return View(news);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading news: {ex.Message}");
                ViewBag.Error = $"Failed to load news: {ex.Message}";
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
                
                return View("Index", news);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error filtering news: {ex.Message}");
                ViewBag.Error = $"Failed to filter news: {ex.Message}";
                return View("Index", new List<NewsDto>());
            }
        }

        /// <summary>
        /// Hiển thị chi tiết một bài báo
        /// </summary>
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                _logger.LogInformation($"Loading news details for ID: {id}");
                var news = await _newsService.GetNewsByIdAsync(id);
                return View(news);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading news details: {ex.Message}");
                ViewBag.Error = $"Failed to load news: {ex.Message}";
                return RedirectToAction("Index");
            }
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
