using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;
public class EventsController : Controller
{
    private readonly RaceDayApiClient _api; public EventsController(RaceDayApiClient api) => _api = api;
    public async Task<IActionResult> Details(int id) { var events = await _api.GetAsync<List<EventViewModel>>("api/events") ?? new(); var item = events.FirstOrDefault(e => e.Id == id); return item == null ? NotFound() : View(item); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Enrol(EnrolViewModel model)
    {
        if (HttpContext.Session.GetString("Role") != "Participant") return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) { TempData["Error"] = "Please select an available category."; return RedirectToAction(nameof(Details), new { id = model.EventId }); }
        var r = await _api.SendAsync(HttpMethod.Post, "api/enrolments", new { model.EventId, model.CategoryId });
        TempData[r.Ok ? "Success" : "Error"] = r.Ok ? "You have successfully enrolled for this event." : r.Message;
        return RedirectToAction(nameof(Details), new { id = model.EventId });
    }
}
