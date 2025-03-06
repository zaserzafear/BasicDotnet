namespace BasicDotnet.Domain.Entities;

public class Customer : UserBase
{
    public Customer()
    {
        RoleId = 3; // Customer Role ID
    }
}
