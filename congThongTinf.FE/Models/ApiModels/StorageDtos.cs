namespace congThongTinf.FE.Models.ApiModels;

/// <summary>
/// value.data của GET /api/admin/storage.
/// </summary>
public class StorageUsageDto
{
    public long UsedBytes { get; set; }
    public long RemainingBytes { get; set; }
    public long QuotaBytes { get; set; }
    public long ObjectCount { get; set; }
    public double UsedPercent { get; set; }
}

public class StorageUsageResult
{
    public bool Success { get; set; }
    public bool SessionExpired { get; set; }
    public string? ErrorMessage { get; set; }
    public StorageUsageDto? Data { get; set; }
}
