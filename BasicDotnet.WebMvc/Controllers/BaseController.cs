using BasicDotnet.WebMvc.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BasicDotnet.WebMvc.Controllers;

[Route("[controller]")]
public class BaseController : Controller
{
    protected readonly string _baseApiUrl;

    public BaseController(IOptions<ApiConfig> apiConfigOption)
    {
        var apiConfig = apiConfigOption.Value;
        _baseApiUrl = apiConfig.BaseApiUrl;
    }
}
