namespace BasicDotnet.App.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HasPermissionAttribute : Attribute
{
    public string PermissionName { get; }

    public HasPermissionAttribute(string permissionName)
    {
        PermissionName = permissionName;
    }
}
