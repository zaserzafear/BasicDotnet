namespace DemoMySql.Models;

public class Customer
{
    public Guid Id { get; set; } = Ulid.NewUlid().ToGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
