namespace congThongTinf.FE.Models.ApiModels;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public bool IsFailure { get; set; }
    public ApiResponseValue<T>? Value { get; set; }
    public object? Error { get; set; }
}

public class ApiResponseValue<T>
{
    public string? Code { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}
