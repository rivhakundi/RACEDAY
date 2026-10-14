// Models/UserProfile.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("UserProfiles")]
public class UserProfile
{
    [Key]
    public int ProfileId { get; set; }
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public User? User { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }
}

