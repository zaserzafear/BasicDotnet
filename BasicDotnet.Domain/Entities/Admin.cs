namespace BasicDotnet.Domain.Entities;

public class Admin : UserBase
{
    public Admin()
    {
        RoleId = 2; // Admin Role ID
    }
}
