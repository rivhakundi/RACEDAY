using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs.Profile;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize] // both roles - Organiser and Participant - can view/update their own profile
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Returns the logged-in user's own account and profile details.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyProfile()
    {
        var user = await _db.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

        if (user is null)
            return Unauthorized();

        return Ok(ToProfileResponse(user));
    }

    /// <summary>Updates the logged-in user's own profile information.</summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _db.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

        if (user is null)
            return Unauthorized();

        user.FullName = request.FullName;

        user.Profile ??= new UserProfile { UserId = user.UserId };
        user.Profile.PhoneNumber = request.PhoneNumber;
        user.Profile.DateOfBirth = request.DateOfBirth;
        user.Profile.Bio = request.Bio;

        await _db.SaveChangesAsync();

        return Ok(ToProfileResponse(user));
    }

    private static ProfileResponse ToProfileResponse(User user) => new()
    {
        UserId = user.UserId,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role.ToString(),
        PhoneNumber = user.Profile?.PhoneNumber,
        DateOfBirth = user.Profile?.DateOfBirth,
        Bio = user.Profile?.Bio
    };
}
