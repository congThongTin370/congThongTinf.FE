using congThongTinf.FE.Models.ApiModels;

namespace congThongTinf.FE.Models;

public class AdminDashboardViewModel
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DashboardDto? Data { get; set; }
    public string? LoadError { get; set; }
}
