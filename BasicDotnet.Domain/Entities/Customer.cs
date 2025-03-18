using BasicDotnet.Domain.Enums;

namespace BasicDotnet.Domain.Entities;

public class Customer : UserBase
{
    public Customer()
    {
        RoleId = (int)UserRoleEnum.Customer; // Customer Role ID
    }
}
