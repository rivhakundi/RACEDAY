using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly BlobStorageService _blob;
    private readonly PasswordHasher<User> _passwordHasher = new();

    public AuthController(RaceDayDbContext context, BlobStorageService? blob = null)
    {
        _context = context;
        _blob = blob!;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || !dto.Email.Contains('@') || dto.Password.Length < 8)
            return BadRequest("Enter a name, valid email address, and password of at least 8 characters.");
        if (dto.Role is not ("Organiser" or "Participant"))
            return BadRequest("Role must be Organiser or Participant.");
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email is already registered.");

        var user = new User { Username = dto.Username.Trim(), Email = dto.Email.Trim(), Role = dto.Role };
        user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(new { message = "User registered successfully", user.Id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password) == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid email or password.");

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserRole", user.Role);
        return Ok(new { message = "Logged in successfully", user.Id, user.Username, user.Email, user.Role });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var id = HttpContext.Session.GetInt32("UserId");
        if (id == null) return Unauthorized();
        var u = await _context.Users.FindAsync(id);
        return u == null ? NotFound() : Ok(new { u.Id, u.Username, u.Email, u.Role, u.ProfilePictureUrl });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] ProfileDto dto)
    {
        var id = HttpContext.Session.GetInt32("UserId");
        if (id == null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains('@'))
            return BadRequest("Enter a valid email address.");

        var u = await _context.Users.FindAsync(id);
        if (u == null) return NotFound();
        if (await _context.Users.AnyAsync(x => x.Email == dto.Email && x.Id != id))
            return BadRequest("That email address is already in use.");

        u.Email = dto.Email.Trim();
        await _context.SaveChangesAsync();
        return Ok(new { u.Username, u.Email, u.Role, u.ProfilePictureUrl });
    }

    [HttpPost("profile-picture")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        var id = HttpContext.Session.GetInt32("UserId");
        if (id == null) return Unauthorized();

        var user = await _context.Users.FindAsync(id.Value);
        if (user == null) return NotFound();

        try
        {
            user.ProfilePictureUrl = await _blob.UploadImageAsync(file, $"profiles/{id.Value}");
            await _context.SaveChangesAsync();
            return Ok(new { user.ProfilePictureUrl });
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return StatusCode(503, ex.Message); }
        catch (Exception ex) { return StatusCode(503, ex.Message); }
    }
}

public record RegisterDto(string Username, string Email, string Password, string Role);
public record LoginDto(string Email, string Password);
public record ProfileDto(string Email);
