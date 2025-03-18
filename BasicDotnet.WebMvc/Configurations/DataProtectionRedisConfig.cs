namespace BasicDotnet.WebMvc.Configurations;

public class DataProtectionRedisConfig
{
    public string RedisConnection { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public int DatabaseId { get; set; } = 0;
}
