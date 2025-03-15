using BasicDotnet.App.Dtos;

namespace BasicDotnet.WebMvc.Models.Auth;

public class RegisterViewModel
{
    public RegisterDto RegisterDto { get; set; } = new();
    public string RegisterApiEndpoint { get; set; } = string.Empty;
}
