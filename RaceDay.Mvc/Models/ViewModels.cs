using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models;

public sealed class LoginViewModel { [Required, EmailAddress] public string Email { get; set; } = ""; [Required, DataType(DataType.Password)] public string Password { get; set; } = ""; }
public sealed class RegisterViewModel { [Required, StringLength(80)] public string Username { get; set; } = ""; [Required, EmailAddress] public string Email { get; set; } = ""; [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = ""; [Required] public string Role { get; set; } = "Participant"; }
public sealed class EventViewModel { public int Id { get; set; } [Required, StringLength(120)] public string Name { get; set; } = ""; [Required, StringLength(2000)] public string Description { get; set; } = ""; [Required] public DateTime Date { get; set; } = DateTime.Today.AddDays(14); [Required] public string Location { get; set; } = ""; [Range(0.1, 500)] public double Distance { get; set; } [Required] public string EventType { get; set; } = "Run"; public string? BannerUrl { get; set; } public int EnrolmentCount { get; set; } public int OrganiserId { get; set; } public List<CategoryViewModel> Categories { get; set; } = new(); }
public sealed class CategoryViewModel { public int Id { get; set; } public string Name { get; set; } = ""; public int EventId { get; set; } }
public sealed class EnrolViewModel { public int EventId { get; set; } [Required] public int CategoryId { get; set; } }
public sealed class ResultEntryViewModel { [Required] public int EnrolmentId { get; set; } [Range(1, 100000)] public int Position { get; set; } [Required, RegularExpression(@"^\d{1,2}:\d{2}:\d{2}$", ErrorMessage="Use HH:mm:ss format.")] public string FinishTime { get; set; } = "00:30:00"; }
public sealed class ProfileViewModel { public string Username { get; set; } = ""; [Required, EmailAddress] public string Email { get; set; } = ""; public string? ProfilePictureUrl { get; set; }
}
