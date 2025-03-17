using BasicDotnet.Domain.Entities;
using BasicDotnet.Domain.Enums;
using BasicDotnet.Infra.Repositories;

namespace BasicDotnet.App.Services;

public class AuthService
{
    private readonly TokenService _tokenService;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public AuthService(TokenService tokenService,
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        _tokenService = tokenService;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<UserBase> AddUserAsync(string userName, string email, string password, UserRoleEnum userRole)
    {
        UserBase user = userRole switch
        {
            UserRoleEnum.SuperAdmin => new SuperAdmin { Username = userName, PasswordHash = password, Email = email },
            UserRoleEnum.Admin => new Admin { Username = userName, PasswordHash = password, Email = email },
            UserRoleEnum.Customer => new Customer { Username = userName, PasswordHash = password, Email = email },
            _ => throw new InvalidOperationException("Unsupported role type")
        };

        var addedUser = await _userRepository.AddUserAsync(user, userRole);

        return addedUser;
    }

    public async Task<TokenResponse?> AuthenticateAsync(string username, string password, UserRoleEnum userRole)
    {
        var user = await _userRepository.AuthenticateAsync(username, password, userRole);
        if (user == null)
            return null;

        return _tokenService.GenerateToken(user);
    }

    public async Task<UserBase?> GetUserByIdAsync(Guid userId, UserRoleEnum userRole)
    {
        return await _userRepository.GetUserByIdAsync(userId, userRole);
    }

    public async Task<Role?> GetRoleByIdAsync(int roleId)
    {
        var role = await _roleRepository.GetRoleByIdAsync(roleId);

        return role;
    }

}
