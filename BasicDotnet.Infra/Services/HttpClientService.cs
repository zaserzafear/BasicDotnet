using BasicDotnet.Infra.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace BasicDotnet.Infra.Services;

public class HttpClientService
{
    private readonly ILogger<HttpClientService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpClientSettings _httpClientSettings;

    public HttpClientService(ILogger<HttpClientService> logger,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        HttpClientSettings httpClientSettings)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _httpClientSettings = httpClientSettings;
    }

    private void AddClientIpHeader(HttpClient client)
    {
        var existingForwardedFor = _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"].ToString();
        var clientIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        if (!string.IsNullOrEmpty(clientIp))
        {
            var newForwardedFor = string.IsNullOrEmpty(existingForwardedFor) ? clientIp : $"{existingForwardedFor}, {clientIp}";
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Forwarded-For", newForwardedFor);
        }
    }

    private void AddClientTimeout(HttpClient client)
    {
        client.Timeout = TimeSpan.FromSeconds(_httpClientSettings.TimeoutInSeconds);
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string clientName, string url)
    {
        var client = _httpClientFactory.CreateClient(clientName);
        AddClientTimeout(client);
        AddClientIpHeader(client);
        try
        {
            var response = await client.GetAsync(url);
            return await HandleResponse<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling GET {Url}", url);
            return new ApiResponse<T> { Message = "An error occurred while processing the request.", StatusCode = 500 };
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string clientName, string url, object? body = null)
    {
        var client = _httpClientFactory.CreateClient(clientName);
        AddClientTimeout(client);
        AddClientIpHeader(client);
        try
        {
            HttpContent? content = body != null ? JsonContent.Create(body) : null;
            var response = await client.PostAsync(url, content);
            return await HandleResponse<T>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calling POST {Url}", url);
            return new ApiResponse<T> { Message = "An error occurred while processing the request.", StatusCode = 500 };
        }
    }

    private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        try
        {
            return JsonSerializer.Deserialize<ApiResponse<T>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize response: {Json}", json);
            return new ApiResponse<T> { Message = "Invalid response format.", StatusCode = (int)response.StatusCode };
        }
    }
}
