namespace BasicDotnet.WebApi.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HasOwnUserIdPermissionAttribute : Attribute
{
    public string PermissionName { get; }

    public HasOwnUserIdPermissionAttribute(string permissionName)
    {
        PermissionName = permissionName;
    }
}
