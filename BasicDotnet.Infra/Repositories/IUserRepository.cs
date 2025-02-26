using BasicDotnet.Domain.Entities;

namespace BasicDotnet.Infra.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task AddUserAsync(User user);
    Task<bool> CheckUserExistsAsync(string email);
}
