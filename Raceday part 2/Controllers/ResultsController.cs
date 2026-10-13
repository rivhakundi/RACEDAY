// Controllers/ResultsController.cs
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ResultsController(RaceDayDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> AddResult([FromBody] ResultDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null || role != "Organiser")
            return Unauthorized("Only Organisers can capture race results.");

        var enrolment = await _context.Enrolments
            .Include(e => e.Event)
            .FirstOrDefaultAsync(e => e.Id == dto.EnrolmentId);

        if (enrolment == null || enrolment.Event.OrganiserId != userId.Value)
            return BadRequest("Invalid enrolment or you do not own this event.");

        var result = new Result
        {
            EnrolmentId = dto.EnrolmentId,
            Position = dto.Position,
            FinishTime = TimeSpan.Parse(dto.FinishTime) // Format: "hh:mm:ss"
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Result captured successfully", result.Id });
    }

    [HttpGet("my-results")]
    public async Task<IActionResult> GetMyResults()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return Unauthorized();

        var results = await _context.Results
            .Include(r => r.Enrolment)
            .ThenInclude(e => e.Event)
            .Where(r => r.Enrolment.UserId == userId.Value)
            .Select(r => new
            {
                EventName = r.Enrolment.Event.Name,
                EventDate = r.Enrolment.Event.Date,
                r.Position,
                FinishTime = r.FinishTime.ToString(@"hh\:mm\:ss")
            })
            .ToListAsync();

        return Ok(results);
    }
}

public record ResultDto(int EnrolmentId, int Position, string FinishTime);