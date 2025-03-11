namespace BasicDotnet.WebApi.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HasOwnUserIdPermissionAttribute : Attribute
{
}
