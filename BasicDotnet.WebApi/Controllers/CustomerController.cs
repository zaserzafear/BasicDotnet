﻿using BasicDotnet.App.Dtos;
using BasicDotnet.App.Services;
using BasicDotnet.Domain.Enums;
using BasicDotnet.Domain.Exceptions;
using BasicDotnet.WebApi.RateLimit;
using BasicDotnet.WebApi.Security.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicDotnet.WebApi.Controllers;

public class CustomerController : BaseController
{
    private readonly UserRoleEnum _userRole = UserRoleEnum.Customer;

    public readonly AuthService _authService;

    public CustomerController(AuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [RateLimitPolicy(RateLimitPolicies.Sensitive)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var user = await _authService.AddUserAsync(dto.UserName, dto.Email, dto.Password, _userRole);
            return Success(new { userId = user.Id }, "User registered successfully");
        }
        catch (UserAlreadyExistsException ex)
        {
            return Error(ex.Message, 409);
        }
        catch (Exception ex)
        {
            return Error($"An unexpected error occurred. {ex.Message}", 500);
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [RateLimitPolicy(RateLimitPolicies.Sensitive)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var token = await _authService.AuthenticateAsync(dto.Username, dto.Password, _userRole);
            if (token == null)
                return Error("Invalid username or password", 401);

            return Success(token, "Login successful");
        }
        catch (Exception ex)
        {
            return Error($"An unexpected error occurred. {ex.Message}", 500);
        }
    }

    [HttpGet("me")]
    [RateLimitPolicy(RateLimitPolicies.Public)]
    public async Task<IActionResult> GetCurrentUserAsync()
    {
        // Get the current user and role from claims
        var currentUserId = GetCurrentUserId();

        if (currentUserId == null)
        {
            return Error("Invalid or missing user ID or role.", 400);
        }

        var user = await _authService.GetUserByIdAsync(currentUserId.Value, _userRole);

        return Success(user);
    }

    [HttpGet("{user_id}")]
    [RateLimitPolicy(RateLimitPolicies.Public)]
    [HasPermission(PermissionEnum.ViewAllCustomers)]
    [HasPermission(PermissionEnum.ViewOwnUserId)]
    public async Task<IActionResult> GetUserByIdAsync(Guid user_id)
    {
        var user = await _authService.GetUserByIdAsync(user_id, _userRole);
        return Success(user);
    }
}
