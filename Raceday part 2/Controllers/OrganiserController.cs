using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;
public class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api; public OrganiserController(RaceDayApiClient api) => _api = api;
    private bool Allowed() => HttpContext.Session.GetString("Role") == "Organiser";
    public async Task<IActionResult> Index() { if (!Allowed()) return RedirectToAction("Login", "Account"); var userId = HttpContext.Session.GetInt32("UserId"); ViewBag.Events = (await _api.GetAsync<List<EventViewModel>>("api/events") ?? new()).Where(e => e.OrganiserId == userId).ToList(); return View(); }
    public IActionResult Create() { if (!Allowed()) return RedirectToAction("Login", "Account"); return View(new EventViewModel()); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Create(EventViewModel model, IFormFile? banner)
    {
        if (!Allowed()) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);
        var r = await _api.SendAsync(HttpMethod.Post, "api/events", new { model.Name, model.Description, model.Date, model.Location, model.Distance, model.EventType });
        if (!r.Ok) { ModelState.AddModelError("", r.Message); return View(model); }
        if (banner is { Length: > 0 })
        {
            var id = r.Data.ValueKind == System.Text.Json.JsonValueKind.Object && r.Data.TryGetProperty("id", out var prop) ? prop.GetInt32() : 0;
            if (id > 0) { var up = await _api.UploadAsync($"api/events/{id}/banner", banner); if (!up.Ok) TempData["Error"] = "Event created, but banner upload failed: " + up.Message; }
        }
        TempData["Success"] = "Event created successfully."; return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id) { if (!Allowed()) return RedirectToAction("Login", "Account"); var item = (await _api.GetAsync<List<EventViewModel>>("api/events") ?? new()).FirstOrDefault(e => e.Id == id); return item == null ? NotFound() : View(item); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Edit(EventViewModel model, IFormFile? banner) { if (!Allowed()) return RedirectToAction("Login", "Account"); if (!ModelState.IsValid) return View(model); var r = await _api.SendAsync(HttpMethod.Put, $"api/events/{model.Id}", new { model.Name, model.Description, model.Date, model.Location, model.Distance, model.EventType }); if (!r.Ok) { ModelState.AddModelError("", r.Message); return View(model); } if (banner is { Length: > 0 }) { var up = await _api.UploadAsync($"api/events/{model.Id}/banner", banner); if (!up.Ok) TempData["Error"] = "Event updated, but banner upload failed: " + up.Message; } TempData["Success"] ??= "Event updated."; return RedirectToAction(nameof(Index)); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Delete(int id) { if (!Allowed()) return RedirectToAction("Login", "Account"); var r = await _api.SendAsync(HttpMethod.Delete, $"api/events/{id}"); TempData[r.Ok ? "Success" : "Error"] = r.Ok ? "Event deleted." : r.Message; return RedirectToAction(nameof(Index)); }
    public async Task<IActionResult> Categories(int id) { if (!Allowed()) return RedirectToAction("Login", "Account"); ViewBag.EventId = id; ViewBag.Event = (await _api.GetAsync<List<EventViewModel>>("api/events") ?? new()).FirstOrDefault(e => e.Id == id); ViewBag.Categories = await _api.GetAsync<List<CategoryViewModel>>($"api/categories/event/{id}") ?? new(); return View(new CategoryViewModel { EventId = id }); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> AddCategory(CategoryViewModel model) { if (!Allowed()) return RedirectToAction("Login", "Account"); var r = await _api.SendAsync(HttpMethod.Post, "api/categories", new { model.EventId, model.Name }); TempData[r.Ok ? "Success" : "Error"] = r.Ok ? "Category added." : r.Message; return RedirectToAction(nameof(Categories), new { id = model.EventId }); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> DeleteCategory(int id, int eventId) { if (!Allowed()) return RedirectToAction("Login", "Account"); var r = await _api.SendAsync(HttpMethod.Delete, $"api/categories/{id}"); TempData[r.Ok ? "Success" : "Error"] = r.Ok ? "Category deleted." : r.Message; return RedirectToAction(nameof(Categories), new { id = eventId }); }
    public async Task<IActionResult> Enrolments(int id) { if (!Allowed()) return RedirectToAction("Login", "Account"); ViewBag.EventId = id; ViewBag.Event = (await _api.GetAsync<List<EventViewModel>>("api/events") ?? new()).FirstOrDefault(e => e.Id == id); ViewBag.Enrolments = await _api.GetAsync<List<System.Text.Json.JsonElement>>($"api/enrolments/event/{id}") ?? new(); return View(); }
    public IActionResult Results(int id, int enrolmentId = 0) { if (!Allowed()) return RedirectToAction("Login", "Account"); ViewBag.EventId = id; return View(new ResultEntryViewModel { EnrolmentId = enrolmentId }); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Results(ResultEntryViewModel model, int eventId) { if (!Allowed()) return RedirectToAction("Login", "Account"); if (!ModelState.IsValid) { ViewBag.EventId = eventId; return View(model); } var r = await _api.SendAsync(HttpMethod.Post, "api/results", model); TempData[r.Ok ? "Success" : "Error"] = r.Ok ? "Result saved." : r.Message; return RedirectToAction(nameof(Enrolments), new { id = eventId }); }
}
