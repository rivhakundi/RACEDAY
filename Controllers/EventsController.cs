using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly BlobStorageService _blob;

    public EventsController(RaceDayDbContext context, BlobStorageService? blob = null)
    {
        _context = context;
        _blob = blob!;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents() => Ok(await _context.Events.AsNoTracking().Include(e => e.Categories).Select(e => new { e.Id, e.Name, e.Description, e.Date, e.Location, e.Distance, e.EventType, e.BannerUrl, e.OrganiserId, Categories = e.Categories.Select(c => new { c.Id, c.Name, c.EventId }), EnrolmentCount = _context.Enrolments.Count(n => n.EventId == e.Id) }).OrderBy(e => e.Date).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var item = await _context.Events.AsNoTracking().Include(e => e.Categories).FirstOrDefaultAsync(e => e.Id == id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] EventDto dto)
    {
        var role = HttpContext.Session.GetString("UserRole");
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null || role != "Organiser") return Unauthorized("Only authenticated Organisers can create events.");
        if (dto.Date < DateTime.Today || dto.Distance <= 0) return BadRequest("Choose a valid future date and positive distance.");
        var item = new Event { Name = dto.Name.Trim(), Description = dto.Description.Trim(), Date = dto.Date, Location = dto.Location.Trim(), Distance = dto.Distance, EventType = dto.EventType.Trim(), OrganiserId = userId.Value };
        _context.Events.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEvent), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventDto dto)
    {
        var item = await _context.Events.FindAsync(id);
        if (item == null) return NotFound();
        if (!Owns(item.OrganiserId)) return StatusCode(StatusCodes.Status403Forbidden, "You do not own this event.");
        if (dto.Distance <= 0) return BadRequest("Distance must be greater than zero.");
        item.Name = dto.Name.Trim();
        item.Description = dto.Description.Trim();
        item.Date = dto.Date;
        item.Location = dto.Location.Trim();
        item.Distance = dto.Distance;
        item.EventType = dto.EventType.Trim();
        await _context.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var item = await _context.Events.Include(e => e.Categories).FirstOrDefaultAsync(e => e.Id == id);
        if (item == null) return NotFound();
        if (!Owns(item.OrganiserId)) return StatusCode(StatusCodes.Status403Forbidden, "You do not own this event.");
        if (await _context.Enrolments.AnyAsync(e => e.EventId == id)) return Conflict("Events with enrolments cannot be deleted.");
        _context.Events.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/banner")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        var item = await _context.Events.FindAsync(id);
        if (item == null) return NotFound();
        if (!Owns(item.OrganiserId)) return StatusCode(StatusCodes.Status403Forbidden, "You do not own this event.");

        try
        {
            item.BannerUrl = await _blob.UploadImageAsync(file, $"events/{id}");
            await _context.SaveChangesAsync();
            return Ok(new { item.BannerUrl });
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return StatusCode(503, ex.Message); }
        catch (Exception ex) { return StatusCode(503, ex.Message); }
    }

    private bool Owns(int organiserId) => HttpContext.Session.GetInt32("UserId") == organiserId && HttpContext.Session.GetString("UserRole") == "Organiser";
}

public record EventDto(string Name, string Description, DateTime Date, string Location, double Distance, string EventType);
