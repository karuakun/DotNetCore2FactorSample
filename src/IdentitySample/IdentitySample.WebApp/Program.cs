using IdentitySample.WebApp.Data;
using IdentitySample.WebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.LoginPath = "/Login";
    });

builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    options.UseInMemoryDatabase("db");
});
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    // Migration
    using var scope = app.Services.CreateScope();
    var provider = scope.ServiceProvider;
    await using var context = provider.GetRequiredService<IdentityDbContext>();
    await context.Database.EnsureCreatedAsync();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
