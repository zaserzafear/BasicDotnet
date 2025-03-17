using BasicDotnet.App.Dtos;

namespace BasicDotnet.WebMvc.Models.Auth;

public class LoginViewModel
{
    public LoginDto LoginDto { get; set; } = new();
    public string LoginApiEndpoint { get; set; } = string.Empty;
}
