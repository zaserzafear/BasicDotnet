using BasicDotnet.Domain.Enums;

namespace BasicDotnet.Domain.Entities;

public class Admin : UserBase
{
    public Admin()
    {
        RoleId = (int)UserRoleEnum.Admin; // Admin Role ID
    }
}
