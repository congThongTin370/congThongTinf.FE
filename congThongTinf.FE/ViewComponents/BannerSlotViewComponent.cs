using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.ViewComponents;

/// <summary>
/// Một banner đang hiện của một slot. Không token.
/// Header (0) trên mọi trang; HomeMid (1) giữa trang chủ.
/// Không có ảnh thì không render.
/// </summary>
public class BannerSlotViewComponent : ViewComponent
{
    private readonly IBannerService _bannerService;
    private readonly ILogger<BannerSlotViewComponent> _logger;

    public BannerSlotViewComponent(IBannerService bannerService, ILogger<BannerSlotViewComponent> logger)
    {
        _bannerService = bannerService;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync(int position)
    {
        if (!BannerPositions.IsValid(position))
        {
            return Content(string.Empty);
        }

        try
        {
            var listed = await _bannerService.GetBannersAsync(string.Empty, new BannerQuery
            {
                PageIndex = 0,
                Position = position,
                IsActive = true
            });

            var banner = listed.Success ? listed.Items.FirstOrDefault() : null;
            if (banner == null || string.IsNullOrWhiteSpace(banner.ImageUrl))
            {
                return Content(string.Empty);
            }

            return View(new BannerSlotViewModel
            {
                Banner = banner,
                Position = position
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load banner slot {Position}", position);
            return Content(string.Empty);
        }
    }
}
