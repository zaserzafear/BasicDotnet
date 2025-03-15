using System.ComponentModel.DataAnnotations;

namespace BasicDotnet.App.Dtos;

public record RegisterDto(
    [Required(ErrorMessage = "Username is required")]
    [MinLength(6, ErrorMessage = "Username must be at least 6 characters")]
    string UserName = "",

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    string Email = "",

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    string Password = ""
);
