using StudentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using StudentManagement.Core.Entities;
using StudentManagement.Application.Services;
using StudentManagement.Core.Interfaces;
using StudentManagement.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// DB Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Detect if running inside Docker
var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
if (isDocker)
{
    builder.WebHost.UseUrls("http://+:80"); // listen on HTTP port 80
}


builder.Services.AddScoped<IStudentRepository,StudentRepository>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentView", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("StudentCreate", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StudentDashboard", policy => policy.RequireRole("Admin","User"));
    options.AddPolicy("StudentEdit", policy => policy.RequireRole("Admin", "User"));
    options.AddPolicy("StudentDelete", policy => policy.RequireRole("Admin"));
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// Apply migrations and seed roles/users
using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new IdentityRole("Admin"));
        if (!await roleManager.RoleExistsAsync("User")) await roleManager.CreateAsync(new IdentityRole("User"));

        var adminEmail = "admin@example.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
            await userManager.CreateAsync(adminUser, "Admin@123");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex.Message}");
    }
}


if (!app.Environment.IsDevelopment() && !isDocker)
{
    app.UseHttpsRedirection(); // Only outside Docker
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
