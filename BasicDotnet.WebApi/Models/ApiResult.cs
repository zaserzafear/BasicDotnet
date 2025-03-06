namespace BasicDotnet.WebApi.Models;

public class ApiResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public string RequestId { get; set; }

    public ApiResult(bool success, string message, T? data, string requestId)
    {
        Success = success;
        Message = message;
        Data = data;
        RequestId = requestId;
    }

    // Static helper methods
    public static ApiResult<T> SuccessResult(T data, string requestId, string message = "Request successful") =>
        new(true, message, data, requestId);

    public static ApiResult<T> ErrorResult(string message, string requestId) =>
        new(false, message, default, requestId);
}
