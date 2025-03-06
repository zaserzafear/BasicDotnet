using BasicDotnet.Domain.Entities;
using BasicDotnet.Domain.Enums;
using BasicDotnet.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BasicDotnet.Infra.Repositories;

internal class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserBase?> AuthenticateAsync(string username, string password, UserRole role)
    {
        UserBase? user = role switch
        {
            UserRole.SuperAdmin => await _context.SuperAdmins.FirstOrDefaultAsync(u => u.Username == username),
            UserRole.Admin => await _context.Admins.FirstOrDefaultAsync(u => u.Username == username),
            UserRole.Customer => await _context.Customers.FirstOrDefaultAsync(u => u.Username == username),
            _ => null
        };

        if (user == null || !VerifyPassword(password, user.PasswordHash))
            return null;

        return user;
    }

    public async Task<UserBase> AddUserAsync(UserBase user, UserRole role)
    {
        // Hash the password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

        switch (role)
        {
            case UserRole.SuperAdmin:
                _context.SuperAdmins.Add((SuperAdmin)user);
                break;
            case UserRole.Admin:
                _context.Admins.Add((Admin)user);
                break;
            case UserRole.Customer:
                _context.Customers.Add((Customer)user);
                break;
            default:
                throw new InvalidOperationException("Unsupported role type");
        }

        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<bool> CheckUserExistsAsync(string email, UserRole role)
    {
        bool isExists = role switch
        {
            UserRole.SuperAdmin => await _context.SuperAdmins.AnyAsync(u => u.Email == email),
            UserRole.Admin => await _context.Admins.AnyAsync(u => u.Email == email),
            UserRole.Customer => await _context.Customers.AnyAsync(u => u.Email == email),
            _ => throw new InvalidOperationException("Unsupported role type")
        };

        return isExists;
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
