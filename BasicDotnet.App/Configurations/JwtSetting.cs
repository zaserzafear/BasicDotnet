namespace BasicDotnet.App.Configurations;

public class JwtSetting
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public double ExpiredMinute { get; set; }
}
