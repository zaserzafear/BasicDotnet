namespace BasicDotnet.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; set; } = Ulid.NewUlid().ToGuid();
    public required User User { get; set; }

    public int RoleId { get; set; }
    public required Role Role { get; set; }
}
