using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context; public ResultsController(RaceDayDbContext context) => _context = context;
    [HttpPost]
    public async Task<IActionResult> AddResult([FromBody] ResultDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId"); if (userId == null || HttpContext.Session.GetString("UserRole") != "Organiser") return Unauthorized("Only Organisers can capture race results.");
        if (dto.Position < 1 || !TimeSpan.TryParse(dto.FinishTime, out var finishTime) || finishTime < TimeSpan.Zero) return BadRequest("Enter a valid finishing position and HH:mm:ss finish time.");
        var enrolment = await _context.Enrolments.Include(e => e.Event).FirstOrDefaultAsync(e => e.Id == dto.EnrolmentId);
        if (enrolment == null || enrolment.Event!.OrganiserId != userId.Value) return BadRequest("Invalid enrolment or you do not own this event.");
        var existing = await _context.Results.FirstOrDefaultAsync(r => r.EnrolmentId == dto.EnrolmentId);
        if (existing == null) { existing = new Result { EnrolmentId = dto.EnrolmentId }; _context.Results.Add(existing); }
        existing.Position = dto.Position; existing.FinishTime = finishTime; existing.Time = finishTime; await _context.SaveChangesAsync(); return Ok(new { message = "Result published successfully", existing.Id });
    }
    [HttpGet("my-results")]
    public async Task<IActionResult> GetMyResults()
    {
        var userId = HttpContext.Session.GetInt32("UserId"); if (userId == null || HttpContext.Session.GetString("UserRole") != "Participant") return Unauthorized();
        var raw = await _context.Results.Include(r => r.Enrolment).ThenInclude(e => e!.Event).Include(r => r.Enrolment).ThenInclude(e => e!.Category).Where(r => r.Enrolment!.UserId == userId.Value).OrderBy(r => r.Enrolment!.Event!.Date).ToListAsync();
        var results = raw.Select(r => new { EventName = r.Enrolment!.Event!.Name, EventDate = r.Enrolment.Event.Date, CategoryName = r.Enrolment.Category!.Name, r.Position, FinishTime = r.FinishTime.ToString(@"hh\:mm\:ss"), FinisherCount = _context.Results.Count(x => x.Enrolment!.EventId == r.Enrolment.EventId) }).ToList();
        return Ok(results);
    }
}
public record ResultDto(int EnrolmentId, int Position, string FinishTime);
