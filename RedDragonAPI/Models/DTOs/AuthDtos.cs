using System.ComponentModel.DataAnnotations;

namespace RedDragonAPI.Models.DTOs;

public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(3), MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required, MinLength(3), MaxLength(200)]
    public string KingdomName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Race { get; set; } = "Ludzie";
}

public class LoginDto
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int KingdomId { get; set; }
    public DateTime ExpiresAt { get; set; }
}
