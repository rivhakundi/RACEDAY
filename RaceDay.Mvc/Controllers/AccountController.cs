using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;
public class AccountController : Controller
{
    private readonly RaceDayApiClient _api; public AccountController(RaceDayApiClient api) => _api = api;
    [HttpGet] public IActionResult Login() => View(new LoginViewModel());
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var r = await _api.SendAsync(HttpMethod.Post, "api/auth/login", new { model.Email, model.Password });
        if (!r.Ok) { ModelState.AddModelError(string.Empty, r.Message); return View(model); }
        var username = ReadString(r.Data, "username") ?? model.Email; var role = ReadString(r.Data, "role") ?? "Participant";
        if (role is not ("Organiser" or "Participant")) { ModelState.AddModelError("", "The API returned an unsupported user role."); return View(model); }
        HttpContext.Session.SetString("Username", username); HttpContext.Session.SetString("Role", role); HttpContext.Session.SetString("Email", ReadString(r.Data, "email") ?? model.Email); if (r.Data.ValueKind == System.Text.Json.JsonValueKind.Object && r.Data.TryGetProperty("id", out var userId) && userId.TryGetInt32(out var parsedId)) HttpContext.Session.SetInt32("UserId", parsedId);
        return RedirectToAction("Index", role == "Organiser" ? "Organiser" : "Participant");
    }
    [HttpGet] public IActionResult Register() => View(new RegisterViewModel());
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (model.Role is not ("Participant" or "Organiser")) { ModelState.AddModelError(nameof(model.Role), "Choose a valid role."); return View(model); }
        var r = await _api.SendAsync(HttpMethod.Post, "api/auth/register", new { model.Username, model.Email, model.Password, model.Role });
        if (!r.Ok) { ModelState.AddModelError("", r.Message); return View(model); }
        TempData["Success"] = "Account created. Please log in."; return RedirectToAction(nameof(Login));
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Logout()
    {
        await _api.SendAsync(HttpMethod.Post, "api/auth/logout", new { }); HttpContext.Session.Clear(); return RedirectToAction("Index", "Home");
    }
    private static string? ReadString(System.Text.Json.JsonElement e, string name) => e.ValueKind == System.Text.Json.JsonValueKind.Object && e.TryGetProperty(name, out var v) ? v.GetString() : null;
}
