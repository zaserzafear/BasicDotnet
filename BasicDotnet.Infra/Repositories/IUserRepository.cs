using BasicDotnet.Domain.Entities;
using BasicDotnet.Domain.Enums;

namespace BasicDotnet.Infra.Repositories;

public interface IUserRepository
{
    Task<UserBase?> AuthenticateAsync(string username, string password, UserRoleEnum role);
    Task<UserBase> AddUserAsync(UserBase user, UserRoleEnum role);
    Task<bool> CheckUserExistsAsync(string username, string email, UserRoleEnum role);
    Task<UserBase?> GetUserByIdAsync(Guid userId, UserRoleEnum role);
}
