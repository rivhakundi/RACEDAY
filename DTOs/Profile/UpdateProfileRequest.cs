using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs.Profile;

public class UpdateProfileRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }
}
