using DemoMySql.Models;
using DemoMySql.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoMySql.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _appDbContext;
    private readonly TokenService _tokenService;

    public CustomersController(AppDbContext appDbContext, TokenService tokenService)
    {
        _appDbContext = appDbContext;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<IActionResult> AddCustomer([FromBody] Customer model)
    {
        var result = await _appDbContext.Customers.AddAsync(model);
        await _appDbContext.SaveChangesAsync();
        return Created();
    }

    [HttpGet]
    public async Task<IActionResult> LoginCustomer(string username, string password)
    {
        var result = await _appDbContext.Customers
            .AsNoTracking()
            .Where(x => x.Username == username && x.PasswordHash == password)
            .Select(x => new Customer { Id = x.Id, Username = x.Username, Email = x.Email })
            .FirstOrDefaultAsync();
        if (result == null || string.IsNullOrEmpty(result.Username))
        {
            return Unauthorized();
        }

        var token = _tokenService.GenerateToken(result.Id.ToString(), result.Username, "customer");

        return Ok(token);
    }

    [HttpGet("me")]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> GetCurrentUserAsync()
    {
        var userId = _tokenService.GetCurrentUserId();
        var result = await _appDbContext.Customers
            .AsNoTracking()
            .Where(x => x.Id.ToString() == userId)
            .Select(x => new Customer { Id = x.Id, Username = x.Username, Email = x.Email })
            .FirstOrDefaultAsync();
        if (result == null || string.IsNullOrEmpty(result.Username))
        {
            return Unauthorized();
        }
        return Ok(result);
    }
}
