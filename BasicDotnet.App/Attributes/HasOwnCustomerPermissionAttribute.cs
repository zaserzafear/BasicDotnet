namespace BasicDotnet.App.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HasOwnCustomerPermissionAttribute : Attribute
{
    public string PermissionName { get; }

    public HasOwnCustomerPermissionAttribute(string permissionName)
    {
        PermissionName = permissionName;
    }
}
