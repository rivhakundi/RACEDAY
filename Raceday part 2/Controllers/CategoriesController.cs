using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/events/{eventId}/categories")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    public CategoriesController(RaceDayDbContext context) => _context = context;

    [HttpPost]
    public async Task<IActionResult> CreateCategory(int eventId, [FromBody] CategoryCreateDto dto)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var role = HttpContext.Session.GetString("UserRole");
        if (userId == null || role != "Organiser")
            return Unauthorized("Only authenticated Organisers can add categories.");

        var eventItem = await _context.Events.FindAsync(eventId);
        if (eventItem == null)
            return NotFound("Event not found.");
        if (eventItem.OrganiserId != userId.Value)
            return Forbid("You can only add categories to your own events.");

        var category = new Category
        {
            EventId = eventId,
            Name = dto.Name,
            MinAge = dto.MinAge,
            MaxAge = dto.MaxAge,
            CategoryDistance = dto.CategoryDistance
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var response = new CategoryResponseDto(
            category.CategoryId,
            category.EventId,
            category.Name,
            category.MinAge,
            category.MaxAge,
            category.CategoryDistance
        );

        return CreatedAtAction(nameof(GetCategories), new { eventId }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(int eventId)
    {
        var categories = await _context.Categories
            .Where(c => c.EventId == eventId)
            .Select(c => new CategoryResponseDto(
                c.CategoryId,
                c.EventId,
                c.Name,
                c.MinAge,
                c.MaxAge,
                c.CategoryDistance
            ))
            .ToListAsync();

        return Ok(categories);
    }

    public record CategoryCreateDto(string Name, int MinAge, int MaxAge, int CategoryDistance);
    public record CategoryResponseDto(
        int CategoryId,
        int EventId,
        string Name,
        int? MinAge,
        int? MaxAge,
        decimal? CategoryDistance
    );
}