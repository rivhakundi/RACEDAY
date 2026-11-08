using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.Models;

namespace RaceDay.Mvc.Controllers;
public class HomeController : Controller
{
    private readonly RaceDayApiClient _api; public HomeController(RaceDayApiClient api) => _api = api;
    public async Task<IActionResult> Index(string? q, string? type)
    {
        var events = await _api.GetAsync<List<EventViewModel>>("api/events") ?? new();
        events = events.Where(e => e.Date >= DateTime.Today).OrderBy(e => e.Date).ToList();
        if (!string.IsNullOrWhiteSpace(q)) events = events.Where(e => (e.Name + " " + e.Location + " " + e.Description).Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(type)) events = events.Where(e => e.EventType.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
        ViewBag.Query = q; ViewBag.Type = type; return View(events);
    }
    public IActionResult Error() => View();
}
