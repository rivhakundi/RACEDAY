using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context; public CategoriesController(RaceDayDbContext context) => _context = context;
    [HttpGet("event/{eventId:int}")]
    public async Task<IActionResult> ForEvent(int eventId) => Ok(await _context.Categories.AsNoTracking().Where(c => c.EventId == eventId).OrderBy(c => c.Name).ToListAsync());
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryDto dto)
    {
        var uid = HttpContext.Session.GetInt32("UserId"); if (uid == null || HttpContext.Session.GetString("UserRole") != "Organiser") return Unauthorized();
        var ev = await _context.Events.FindAsync(dto.EventId); if (ev == null) return NotFound("Event not found."); if (ev.OrganiserId != uid) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Category name is required.");
        var category = new Category { Name = dto.Name.Trim(), EventId = dto.EventId }; _context.Categories.Add(category); await _context.SaveChangesAsync(); return Ok(category);
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var uid = HttpContext.Session.GetInt32("UserId"); if (uid == null || HttpContext.Session.GetString("UserRole") != "Organiser") return Unauthorized();
        var category = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.Id == id); if (category == null) return NotFound(); if (category.Event?.OrganiserId != uid) return Forbid();
        if (await _context.Enrolments.AnyAsync(e => e.CategoryId == id)) return Conflict("A category with enrolments cannot be deleted.");
        _context.Categories.Remove(category); await _context.SaveChangesAsync(); return NoContent();
    }
}
public record CategoryDto(int EventId, string Name);
