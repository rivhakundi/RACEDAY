using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class ProfileController : Controller
{
    private readonly RaceDayApiClient _api; public ProfileController(RaceDayApiClient api) => _api = api;
    public async Task<IActionResult> Index() { if (HttpContext.Session.GetString("Role") is null) return RedirectToAction("Login", "Account"); var p = await _api.GetAsync<ProfileViewModel>("api/auth/profile"); return View(p ?? new ProfileViewModel { Username = HttpContext.Session.GetString("Username") ?? "", Email = HttpContext.Session.GetString("Email") ?? "" }); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Index(ProfileViewModel model, IFormFile? picture) { if (HttpContext.Session.GetString("Role") is null) return RedirectToAction("Login", "Account"); if (!ModelState.IsValid) return View(model); var r = await _api.SendAsync(HttpMethod.Put, "api/auth/profile", new { model.Email }); if (!r.Ok) { ModelState.AddModelError("", r.Message); return View(model); } if (picture is { Length: > 0 }) { var up = await _api.UploadAsync("api/auth/profile-picture", picture); if (!up.Ok) TempData["Error"] = up.Message; } HttpContext.Session.SetString("Email", model.Email); TempData["Success"] = "Profile updated."; return RedirectToAction(nameof(Index)); }
}
