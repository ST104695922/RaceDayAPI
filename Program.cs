
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Register controllers.
builder.Services.AddControllers();

// Register EF Core and connect to the existing RACEDAY database.
builder.Services.AddDbContext<RaceDayContext>(options =>
    options.UseSqlServer(
        @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=RACEDAY;Integrated Security=True;TrustServerCertificate=True;"));

// Enable server-side session storage.
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy =
        Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
});

// Enable Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Session middleware must run before controller endpoints.
app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();
