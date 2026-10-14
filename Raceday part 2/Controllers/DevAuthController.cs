using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;

[ApiController]
[Route("api/[controller]")]
public class DevAuthController : ControllerBase
{
    private readonly IHostEnvironment _env;
    public DevAuthController(IHostEnvironment env) => _env = env;

    [HttpPost("set-session")]
    public IActionResult SetSession([FromBody] DevLoginDto dto)
    {
        if (!_env.IsDevelopment())
            return Forbid(); // only allow in Development

        HttpContext.Session.SetInt32("UserId", dto.UserId);
        HttpContext.Session.SetString("UserRole", dto.Role ?? "Participant");
        return Ok(new { message = "Session set for dev testing" });
    }
}

public record DevLoginDto(int UserId, string? Role);