// Program.cs
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerUI; // add this using

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=(localdb)\\mssqllocaldb;Database=RaceDayDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"));

// Configure Session State
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton<BlobStorageService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure the database and schema are created automatically
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<RaceDayDbContext>();
        db.Database.Migrate();
        logger.LogInformation("Database 'RaceDayDb' verified / migrated successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating or migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
    });
}

app.UseHttpsRedirection();

// Enable Session Middleware
app.UseSession();

app.UseAuthorization();

app.MapControllers();

// Give the API root a useful response instead of HTTP 404.
app.MapGet("/", (IWebHostEnvironment environment) =>
{
    if (environment.IsDevelopment())
        return Results.Redirect("/swagger");

    return Results.Ok(new
    {
        application = "RaceDay API",
        status = "Running",
        documentation = "Swagger is available in Development only."
    });
});

app.Run();