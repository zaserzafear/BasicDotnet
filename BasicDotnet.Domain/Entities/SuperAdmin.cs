using BasicDotnet.Domain.Enums;

namespace BasicDotnet.Domain.Entities;

public class SuperAdmin : UserBase
{
    public SuperAdmin()
    {
        RoleId = (int)UserRoleEnum.SuperAdmin; // SuperAdmin Role ID
    }
}
