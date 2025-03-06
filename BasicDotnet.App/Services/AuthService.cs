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

    // Login logic with token generation
    public async Task<string?> LoginAsync(string username, string password)
    {
        var user = await _userRepository.AuthenticateAsync(username, password, Domain.Enums.UserRole.Admin);
        if (user == null || user.PasswordHash != password)
            return null;

        return user.Id.ToString();
    }
}
