using congThongTinf.FE.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace congThongTinf.FE.ViewComponents;

/// <summary>
/// Dải thủ trưởng trên trang chủ. GET /api/leaders, không token.
/// Không có dữ liệu thì không render.
/// </summary>
public class LeaderStripViewComponent : ViewComponent
{
    private readonly ILeaderService _leaderService;
    private readonly ILogger<LeaderStripViewComponent> _logger;

    public LeaderStripViewComponent(ILeaderService leaderService, ILogger<LeaderStripViewComponent> logger)
    {
        _leaderService = leaderService;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            var listed = await _leaderService.GetLeadersAsync();
            if (!listed.Success || listed.Items.Count == 0)
            {
                return Content(string.Empty);
            }

            return View(listed.Items);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load leaders");
            return Content(string.Empty);
        }
    }
}
