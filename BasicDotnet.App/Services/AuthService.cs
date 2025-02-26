using BasicDotnet.Domain.Entities;
using BasicDotnet.Infra.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null || user.PasswordHash != password)
            return null;

        return GenerateJwtToken(user);
    }

    // Register logic
    public async Task<bool> RegisterAsync(string username, string email, string password)
    {
        if (await _userRepository.CheckUserExistsAsync(email))
            return false;

        var user = new User
        {
            UserName = username,
            Email = email,
            PasswordHash = password
        };

        await _userRepository.AddUserAsync(user);
        return true;
    }

    // JWT Token generation
    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
