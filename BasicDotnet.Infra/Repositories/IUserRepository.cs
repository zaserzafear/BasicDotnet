using BasicDotnet.Domain.Entities;
using BasicDotnet.Domain.Enums;

namespace BasicDotnet.Infra.Repositories;

public interface IUserRepository
{
    Task<UserBase?> AuthenticateAsync(string username, string password, UserRole role);
    Task<UserBase> AddUserAsync(UserBase user, UserRole role);
    Task<bool> CheckUserExistsAsync(string username, string email, UserRole role);
    Task<UserBase?> GetUserByIdAsync(Guid userId, UserRole role);
}
