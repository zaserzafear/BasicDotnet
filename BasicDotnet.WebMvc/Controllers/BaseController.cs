using BasicDotnet.WebMvc.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BasicDotnet.WebMvc.Controllers;

[Route("[controller]")]
public class BaseController : Controller
{
    protected string ControllerName => ControllerContext.ActionDescriptor.ControllerName;

    protected readonly string _baseApiUrlFrontend;
    protected readonly string _baseApiUrlBackend;

    public BaseController(IOptions<ApiConfig> apiConfigOption)
    {
        var apiConfig = apiConfigOption.Value;
        _baseApiUrlFrontend = apiConfig.BaseApiUrlFrontend;
        _baseApiUrlBackend = apiConfig.BaseApiUrlBackend;
    }
}
