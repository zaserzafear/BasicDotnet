namespace BasicDotnet.Domain.Enums;

public static class PermissionExtensions
{
    public static string GetName(this PermissionEnum permission)
    {
        return Enum.GetName(typeof(PermissionEnum), permission) ?? throw new ArgumentException("Invalid permission");
    }
}
