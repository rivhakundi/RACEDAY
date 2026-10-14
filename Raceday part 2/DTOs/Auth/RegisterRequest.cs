using Microsoft.VisualStudio.Services.UserAccountMapping;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs.Auth;

public class RegisterRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>Must be either "Organiser" or "Participant".</summary>
    [Required]
    public UserRole Role { get; set; }
}