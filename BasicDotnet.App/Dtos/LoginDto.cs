using System.ComponentModel.DataAnnotations;

namespace BasicDotnet.App.Dtos;

public record LoginDto(
    [Required(ErrorMessage = "Username is required")]
    string Username = "",

    [Required(ErrorMessage = "Password is required")]
    string Password = ""
);
