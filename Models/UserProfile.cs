using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class UserProfile
{
    public int ProfileId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }
}
