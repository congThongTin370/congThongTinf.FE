namespace congThongTinf.FE.Models.ApiModels;

public class ApiResult<T>
{
    public T? Data { get; set; }
    public string? Code { get; set; }
    public string? Message { get; set; }
    public bool Success { get; set; }
}
