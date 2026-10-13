using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardResult> GetDashboardAsync(string accessToken, DateTime fromDate, DateTime toDate);
}

public class DashboardResult
{
    public bool Success { get; set; }
    public bool SessionExpired { get; set; }
    public string? ErrorMessage { get; set; }
    public DashboardDto? Data { get; set; }
}
