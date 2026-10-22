using congThongTinf.FE.Models.ApiModels;
using congThongTinf.FE.Services.Interfaces;
using congThongTinf.FE.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace congThongTinf.FE.ViewComponents;

/// <summary>
/// Một banner đang hiện của một slot. Không token.
/// Header (0) trên mọi trang; HomeMid (1) giữa trang chủ.
/// Không có ảnh thì không render.
/// </summary>
public class BannerSlotViewComponent : ViewComponent
{
    private readonly IBannerService _bannerService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BannerSlotViewComponent> _logger;

    public BannerSlotViewComponent(
        IBannerService bannerService,
        IMemoryCache cache,
        ILogger<BannerSlotViewComponent> logger)
    {
        _bannerService = bannerService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync(int position)
    {
        if (!BannerPositions.IsValid(position))
        {
            return Content(string.Empty);
        }

        var cacheKey = "banner-slot:" + position;
        if (PublicReadCache.TryGetFresh(_cache, cacheKey, out BannerDto? fresh))
        {
            return Slot(fresh, position);
        }

        try
        {
            var listed = await _bannerService.GetBannersAsync(string.Empty, new BannerQuery
            {
                PageIndex = 0,
                Position = position,
                IsActive = true
            });

            if (listed.Success)
            {
                var banner = listed.Items.FirstOrDefault();
                PublicReadCache.Set<BannerDto?>(_cache, cacheKey, banner);
                return Slot(banner, position);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load banner slot {Position}", position);
        }

        return PublicReadCache.TryGetStale(_cache, cacheKey, out BannerDto? stale)
            ? Slot(stale, position)
            : Content(string.Empty);
    }

    private IViewComponentResult Slot(BannerDto? banner, int position)
    {
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
}
