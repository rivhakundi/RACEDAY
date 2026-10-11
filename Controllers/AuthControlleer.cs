using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs.Auth;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthController(RaceDayDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    /// <summary>Registers a new user as either an Organiser or a Participant.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var emailExists = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
            return Conflict(new { message = "A user with this email is already registered." });

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Role = request.Role
        };

        // Password is hashed (PBKDF2 via ASP.NET Core's PasswordHasher) - never stored in original form.
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Every user gets a profile row they can later fill in via PUT /api/users/me.
        _db.UserProfiles.Add(new UserProfile { UserId = user.UserId });
        await _db.SaveChangesAsync();

        var response = new UserResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };

        return CreatedAtAction(nameof(Register), new { id = user.UserId }, response);
    }

    /// <summary>Authenticates a user and establishes their session, recording identity and role for subsequent requests.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null)
            return Unauthorized(new { message = "Invalid email or password." });

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verification == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Invalid email or password." });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok(new UserResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        });
    }

    /// <summary>Ends the current authenticated session.</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Logged out." });
    }
}