namespace DemoMySql.Cls;

public class TokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpires { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpires { get; set; }
}
