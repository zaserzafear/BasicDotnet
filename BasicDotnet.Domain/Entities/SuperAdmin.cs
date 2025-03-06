namespace BasicDotnet.Domain.Entities;

public class SuperAdmin : UserBase
{
    public SuperAdmin()
    {
        RoleId = 1; // SuperAdmin Role ID
    }
}
