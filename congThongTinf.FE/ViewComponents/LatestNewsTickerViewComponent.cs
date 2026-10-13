using congThongTinf.FE.Models;
using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.ViewComponents
{
    /// <summary>
    /// Băng tin chạy phía trên header: lấy tin mới nhất từ API (title + link chi tiết).
    /// Không có dữ liệu thì không hiển thị gì.
    /// </summary>
    public class LatestNewsTickerViewComponent : ViewComponent
    {
        private const int MaxItems = 5;

        private readonly INewsService _newsService;
        private readonly ILogger<LatestNewsTickerViewComponent> _logger;

        public LatestNewsTickerViewComponent(INewsService newsService, ILogger<LatestNewsTickerViewComponent> logger)
        {
            _newsService = newsService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var news = await _newsService.GetAllNewsAsync();
                var items = NewsStatuses.VisibleOnPortal(news)
                    .Where(n => !string.IsNullOrWhiteSpace(n.Title))
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(MaxItems)
                    .ToList();

                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load ticker news");
                return View(new List<NewsDto>());
            }
        }
    }
}
