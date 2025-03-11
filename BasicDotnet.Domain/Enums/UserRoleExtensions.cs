namespace BasicDotnet.Domain.Enums;

public static class UserRoleExtensions
{
    public static string GetName(this UserRoleEnum role)
    {
        return Enum.GetName(typeof(UserRoleEnum), role) ?? throw new ArgumentException("Invalid role");
    }
}
