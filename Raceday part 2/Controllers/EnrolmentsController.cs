// Controllers/EnrolmentsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EnrolmentsController(RaceDayDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Enrol([FromBody] EnrolmentDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null || role != "Participant")
            return Unauthorized("Only authenticated Participants can enrol in events.");

        // Check if event and category exist
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.EventId == dto.EventId);
        if (category == null)
            return BadRequest("Invalid event or category combination.");

        // Check if already enrolled
        var existing = await _context.Enrolments
            .FirstOrDefaultAsync(e => e.UserId == userId.Value && e.EventId == dto.EventId);
        if (existing != null)
            return BadRequest("You are already enrolled in this event.");

        var enrolment = new Enrolment
        {
            UserId = userId.Value,
            EventId = dto.EventId,
            CategoryId = dto.CategoryId
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Successfully enrolled in event", enrolment.Id });
    }

    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");

        if (userId == null || role != "Organiser")
            return Unauthorized("Only Organisers can view event enrolments.");

        var eventItem = await _context.Events.FindAsync(eventId);
        if (eventItem == null || eventItem.OrganiserId != userId.Value)
            return Forbid("You do not own this event.");

        var enrolments = await _context.Enrolments
            .Where(e => e.EventId == eventId)
            .Include(e => e.User)
            .Include(e => e.Category)
            .Select(e => new { e.Id, e.User.Username, e.User.Email, CategoryName = e.Category.Name, e.RegistrationDate })
            .ToListAsync();

        return Ok(enrolments);
    }
}

public record EnrolmentDto(int EventId, int CategoryId);