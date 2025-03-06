using BasicDotnet.Domain.Entities;
using BasicDotnet.Infra.Repositories;
using Microsoft.Extensions.Configuration;

namespace BasicDotnet.App.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _config = config;
    }

    public async Task<UserBase> AddUserAsync(string userName, string email, string password, Domain.Enums.UserRole userRole)
    {
        UserBase user = userRole switch
        {
            Domain.Enums.UserRole.SuperAdmin => new SuperAdmin { Username = userName, PasswordHash = password, Email = email },
            Domain.Enums.UserRole.Admin => new Admin { Username = userName, PasswordHash = password, Email = email },
            Domain.Enums.UserRole.Customer => new Customer { Username = userName, PasswordHash = password, Email = email },
            _ => throw new InvalidOperationException("Unsupported role type")
        };

        var addedUser = await _userRepository.AddUserAsync(user, userRole);

        return addedUser;
    }

    public async Task<string?> AuthenticateAsync(string username, string password, Domain.Enums.UserRole userRole)
    {
        var user = await _userRepository.AuthenticateAsync(username, password, userRole);
        if (user == null)
            return null;

        return user.Id.ToString();
    }
}
