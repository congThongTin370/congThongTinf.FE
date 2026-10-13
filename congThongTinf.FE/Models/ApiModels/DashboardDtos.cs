namespace congThongTinf.FE.Models.ApiModels;

/// <summary>
/// Dữ liệu tổng hợp cho trang Bảng điều khiển. Được ghép từ 5 endpoint /api/dashboard/*.
/// </summary>
public class DashboardDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DashboardUserStats Users { get; set; } = new();
    public DashboardNewsStats News { get; set; } = new();
    public DashboardDocumentStats Documents { get; set; } = new();
    public DashboardCategoryStats Categories { get; set; } = new();
    public DashboardBannerStats Banners { get; set; } = new();
    public List<DashboardNewsCategoryCount> NewsByCategory { get; set; } = new();
    public List<DashboardNewsByDay> NewsByDay { get; set; } = new();
    public List<DashboardNewsItem> PendingNews { get; set; } = new();
    public List<DashboardNewsItem> TopViewedNews { get; set; } = new();
    public List<DashboardRecentDocument> RecentDocuments { get; set; } = new();
}

public class DashboardUserStats
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int Admin { get; set; }
    public int Journalist { get; set; }
    public int LoggedInInRange { get; set; }
}

public class DashboardNewsStats
{
    public int Total { get; set; }
    public int Draft { get; set; }
    public int Pending { get; set; }
    public int Published { get; set; }
    public int Archived { get; set; }
    public int CreatedInRange { get; set; }
    public int PublishedInRange { get; set; }
    public long TotalViews { get; set; }
}

public class DashboardDocumentStats
{
    public int Total { get; set; }
    public int UploadedInRange { get; set; }
    public long TotalFileSize { get; set; }
    public List<DashboardDocumentCategoryCount> ByCategory { get; set; } = new();
}

public class DashboardCategoryStats
{
    public int Total { get; set; }
    public int Active { get; set; }
}

public class DashboardBannerStats
{
    public int Total { get; set; }
    public int Active { get; set; }
}

public class DashboardDocumentCategoryCount
{
    public int Category { get; set; }
    public int Count { get; set; }
}

public class DashboardNewsCategoryCount
{
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int Count { get; set; }
}

public class DashboardNewsByDay
{
    public DateTime Date { get; set; }
    public int Created { get; set; }
    public int Published { get; set; }
}

public class DashboardNewsItem
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? AuthorName { get; set; }
    public string? CategoryName { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DashboardRecentDocument
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? FileName { get; set; }
    public int Category { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ---- Payload từng endpoint (khớp value.data của backend) ----

public class DashboardUsersPayload
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    public int Admin { get; set; }
    public int Journalist { get; set; }
    public int LoggedInInRange { get; set; }
}

public class DashboardNewsPayload
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DashboardNewsStats Stats { get; set; } = new();
    public List<DashboardNewsCategoryCount> NewsByCategory { get; set; } = new();
    public List<DashboardNewsByDay> NewsByDay { get; set; } = new();
    public List<DashboardNewsItem> PendingNews { get; set; } = new();
    public List<DashboardNewsItem> TopViewedNews { get; set; } = new();
}

public class DashboardDocumentsPayload
{
    public int Total { get; set; }
    public int UploadedInRange { get; set; }
    public long TotalFileSize { get; set; }
    public List<DashboardDocumentCategoryCount> ByCategory { get; set; } = new();
    public List<DashboardRecentDocument> RecentItems { get; set; } = new();
}

public class DashboardCountsPayload
{
    public int Total { get; set; }
    public int Active { get; set; }
}
