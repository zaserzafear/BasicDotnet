using BasicDotnet.Domain.Entities;
using BasicDotnet.Domain.Enums;
using BasicDotnet.Domain.Exceptions;
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

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return user;
    }

    public async Task<UserBase> AddUserAsync(UserBase user, UserRole role)
    {
        // Check if user already exists
        bool userExists = await CheckUserExistsAsync(user.Username, user.Email, role);
        if (userExists)
        {
            throw new UserAlreadyExistsException($"A user with the username '{user.Username}' or email '{user.Email}' already exists.");
        }

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

    public async Task<bool> CheckUserExistsAsync(string username, string email, UserRole role)
    {
        bool isExists = role switch
        {
            UserRole.SuperAdmin => await _context.SuperAdmins.AnyAsync(u => u.Email == email || u.Username == username),
            UserRole.Admin => await _context.Admins.AnyAsync(u => u.Email == email || u.Username == username),
            UserRole.Customer => await _context.Customers.AnyAsync(u => u.Email == email || u.Username == username),
            _ => throw new InvalidOperationException("Unsupported role type")
        };

        return isExists;
    }

    public async Task<UserBase?> GetUserByIdAsync(Guid userId, UserRole role)
    {
        return role switch
        {
            UserRole.SuperAdmin => await _context.SuperAdmins.FirstOrDefaultAsync(u => u.Id == userId),
            UserRole.Admin => await _context.Admins.FirstOrDefaultAsync(u => u.Id == userId),
            UserRole.Customer => await _context.Customers.FirstOrDefaultAsync(u => u.Id == userId),
            _ => throw new InvalidOperationException("Unsupported role type")
        };
    }
}
