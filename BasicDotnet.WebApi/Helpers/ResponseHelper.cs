using Microsoft.AspNetCore.Mvc;

namespace BasicDotnet.WebApi.Helpers;

public static class ResponseHelper
{
    public static IActionResult Success<T>(string requestId, T data, string message = "Request successful", int statusCode = 200)
    {
        return new ObjectResult(new
        {
            data,
            message,
            requestId = requestId,
            statusCode = statusCode,
        })
        {
            StatusCode = statusCode
        };
    }

    public static IActionResult Error(string requestId, string message, int statusCode = 400)
    {
        return new ObjectResult(new
        {
            message,
            requestId = requestId,
            statusCode = statusCode,
        })
        {
            StatusCode = statusCode
        };
    }
}
