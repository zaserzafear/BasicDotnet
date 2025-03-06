namespace BasicDotnet.Domain.Entities;

public abstract class UserBase
{
    public Guid Id { get; set; } = Ulid.NewUlid().ToGuid();
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string Email { get; set; } = string.Empty;
}
