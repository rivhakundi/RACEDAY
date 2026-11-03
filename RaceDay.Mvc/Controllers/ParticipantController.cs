using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api; public ParticipantController(RaceDayApiClient api) => _api = api;
    private bool Allowed() => HttpContext.Session.GetString("Role") == "Participant";
    public async Task<IActionResult> Index() { if (!Allowed()) return RedirectToAction("Login", "Account"); ViewBag.Events = (await _api.GetAsync<List<EventViewModel>>("api/events") ?? new()).Where(e => e.Date >= DateTime.Today).OrderBy(e => e.Date).Take(4).ToList(); return View(); }
    public async Task<IActionResult> MyEvents() { if (!Allowed()) return RedirectToAction("Login", "Account"); ViewBag.Items = await _api.GetAsync<List<System.Text.Json.JsonElement>>("api/enrolments/my") ?? new(); return View(); }
    public async Task<IActionResult> MyResults() { if (!Allowed()) return RedirectToAction("Login", "Account"); ViewBag.Items = await _api.GetAsync<List<System.Text.Json.JsonElement>>("api/results/my-results") ?? new(); return View(); }
}
