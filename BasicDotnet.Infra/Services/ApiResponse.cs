namespace BasicDotnet.Infra.Services;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}
