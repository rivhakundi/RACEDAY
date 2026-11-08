using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context; public EnrolmentsController(RaceDayDbContext context) => _context = context;
    [HttpPost]
    public async Task<IActionResult> Enrol([FromBody] EnrolmentDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId"); if (userId == null || HttpContext.Session.GetString("UserRole") != "Participant") return Unauthorized("Only authenticated Participants can enrol in events.");
        var ev = await _context.Events.FindAsync(dto.EventId); if (ev == null || ev.Date < DateTime.Today) return BadRequest("This event is unavailable for enrolment.");
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.EventId == dto.EventId); if (category == null) return BadRequest("Select a valid category for this event.");
        if (await _context.Enrolments.AnyAsync(e => e.UserId == userId.Value && e.EventId == dto.EventId)) return BadRequest("You are already enrolled in this event.");
        var enrolment = new Enrolment { UserId = userId.Value, EventId = dto.EventId, CategoryId = dto.CategoryId, Status = "Confirmed" }; _context.Enrolments.Add(enrolment); await _context.SaveChangesAsync(); return Ok(new { message = "Successfully enrolled in event", enrolment.Id, enrolment.Status });
    }
    [HttpGet("event/{eventId:int}")]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        var userId = HttpContext.Session.GetInt32("UserId"); if (userId == null || HttpContext.Session.GetString("UserRole") != "Organiser") return Unauthorized("Only Organisers can view event enrolments.");
        var eventItem = await _context.Events.FindAsync(eventId); if (eventItem == null) return NotFound(); if (eventItem.OrganiserId != userId.Value) return Forbid();
        return Ok(await _context.Enrolments.Where(e => e.EventId == eventId).Include(e => e.User).Include(e => e.Category).Select(e => new { e.Id, Username = e.User!.Username, Email = e.User.Email, CategoryName = e.Category!.Name, e.RegistrationDate, e.Status }).ToListAsync());
    }
    [HttpGet("my")]
    public async Task<IActionResult> MyEnrolments()
    {
        var id = HttpContext.Session.GetInt32("UserId"); if (id == null || HttpContext.Session.GetString("UserRole") != "Participant") return Unauthorized();
        return Ok(await _context.Enrolments.Where(e => e.UserId == id.Value).Include(e => e.Event).Include(e => e.Category).Select(e => new { e.Id, EventId = e.EventId, EventName = e.Event!.Name, EventDate = e.Event.Date, CategoryName = e.Category!.Name, e.RegistrationDate, e.Status }).OrderBy(e => e.EventDate).ToListAsync());
    }
}
public record EnrolmentDto(int EventId, int CategoryId);
