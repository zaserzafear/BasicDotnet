using BasicDotnet.App.Configurations;
using BasicDotnet.Domain.Entities;
using BasicDotnet.Domain.Enums;
using BasicDotnet.Infra.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BasicDotnet.App.Services;

public class AuthService
{
    private readonly JwtSetting _jwtSetting;
    private readonly IUserRepository _userRepository;

    public AuthService(JwtSetting jwtSetting, IUserRepository userRepository)
    {
        _jwtSetting = jwtSetting;
        _userRepository = userRepository;
    }

    public async Task<UserBase> AddUserAsync(string userName, string email, string password, UserRole userRole)
    {
        UserBase user = userRole switch
        {
            UserRole.SuperAdmin => new SuperAdmin { Username = userName, PasswordHash = password, Email = email },
            UserRole.Admin => new Admin { Username = userName, PasswordHash = password, Email = email },
            UserRole.Customer => new Customer { Username = userName, PasswordHash = password, Email = email },
            _ => throw new InvalidOperationException("Unsupported role type")
        };

        var addedUser = await _userRepository.AddUserAsync(user, userRole);

        return addedUser;
    }

    public async Task<string?> AuthenticateAsync(string username, string password, UserRole userRole)
    {
        var user = await _userRepository.AuthenticateAsync(username, password, userRole);
        if (user == null)
            return null;

        return GenerateToken(user);
    }

    public async Task<UserBase?> GetUserByIdAsync(Guid userId, UserRole userRole)
    {
        return await _userRepository.GetUserByIdAsync(userId, userRole);
    }

    private string GenerateToken(UserBase user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSetting.SecretKey);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.RoleId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSetting.ExpiredMinute),
            Issuer = _jwtSetting.Issuer,
            Audience = _jwtSetting.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
