namespace congThongTinf.FE.Models.ApiModels;

/// <summary>
/// value.data của GET /api/admin/storage.
/// </summary>
public class StorageUsageDto
{
    public string? BucketName { get; set; }
    public long PayloadBytes { get; set; }
    public long MetadataBytes { get; set; }
    public long UsedBytes { get; set; }
    public long RemainingBytes { get; set; }
    public long QuotaBytes { get; set; }
    public long ObjectCount { get; set; }
    public double UsedPercent { get; set; }
    public DateTime MeasuredAt { get; set; }
}

public class StorageUsageResult
{
    public bool Success { get; set; }
    public bool SessionExpired { get; set; }
    public string? ErrorMessage { get; set; }
    public StorageUsageDto? Data { get; set; }
}
