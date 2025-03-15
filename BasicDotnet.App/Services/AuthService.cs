﻿using BasicDotnet.App.Configurations;
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
    private readonly IRoleRepository _roleRepository;

    public AuthService(JwtSetting jwtSetting, IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _jwtSetting = jwtSetting;
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

        return GenerateToken(user);
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

    private TokenResponse GenerateToken(UserBase user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSetting.SecretKey);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.RoleId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_jwtSetting.ExpiredMinute);
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_jwtSetting.RefreshTokenExpiredDays);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = accessTokenExpiration,
            Issuer = _jwtSetting.Issuer,
            Audience = _jwtSetting.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var accessToken = tokenHandler.CreateToken(tokenDescriptor);
        var refreshToken = Guid.NewGuid().ToString("N"); // Generate a strong refresh token (can improve if needed)

        // Store refresh token securely in the database with user reference if required

        return new TokenResponse
        {
            AccessToken = tokenHandler.WriteToken(accessToken),
            AccessTokenExpires = accessTokenExpiration,
            RefreshToken = refreshToken,
            RefreshTokenExpires = refreshTokenExpiration
        };
    }
}
