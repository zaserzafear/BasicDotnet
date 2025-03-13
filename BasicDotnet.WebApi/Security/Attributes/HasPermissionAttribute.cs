using BasicDotnet.Domain.Enums;

namespace BasicDotnet.WebApi.Security.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HasPermissionAttribute : Attribute
{
    public PermissionEnum Permission { get; }

    public HasPermissionAttribute(PermissionEnum permission)
    {
        Permission = permission;
    }
}
