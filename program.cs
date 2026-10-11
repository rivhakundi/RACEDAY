using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------- Controllers ----------
builder.Services.AddControllers();

// ---------- EF Core (Code-First, SQL Server) ----------
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- Password hashing ----------
// Lightweight Identity component (PBKDF2-based) - no full Identity/UserManager stack,
// since Users is our own entity defined to match the Part 1 ERD and SQL script.
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// ---------- Cookie-based session authentication ----------
// The brief asks for session management (not JWT), so the authenticated user's id and
// role are stored in an encrypted auth cookie. This also means Swagger UI's "Try it out"
// works with zero extra setup: the browser sends the cookie automatically.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "RaceDay.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // This is an API, not an MVC app - return status codes instead of
        // redirecting to a non-existent login page.
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "API for the RaceDay event management system - authentication, " +
                      "events, categories, enrolments and results."
    });
});

var app = builder.Build();

// Swagger available in all environments for this assignment so markers can verify
// the live API against the Part 1 plan without a special setup step.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposes Program for the WebApplicationFactory<Program> used by the integration/unit test project.
public partial class Program { }