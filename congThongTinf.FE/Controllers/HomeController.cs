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

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading news for home page");
                var newsList = await _newsService.GetAllNewsAsync();
                
                _logger.LogInformation($"Fetched {newsList.Count} news items");
                
                // Sắp xếp theo ngày tạo mới nhất trước
                var sortedNews = newsList.OrderByDescending(n => n.CreatedAt).ToList();

                // Ảnh override cho tin mới nhất (nếu được cấu hình trong appsettings)
                ViewData["LatestNewsCoverOverride"] = _configuration["HomeSettings:LatestNewsCoverImageOverride"];

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
