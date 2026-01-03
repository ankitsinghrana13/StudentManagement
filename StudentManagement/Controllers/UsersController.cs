using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Core.Entities;

[Authorize(Roles = "Admin")] // Only Admin can manage users
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(UserManager<ApplicationUser> userManager,
                           RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // List all users
    [AllowAnonymous]
    public IActionResult UserLists()
    {
        var users = _userManager.Users.ToList();
        return View(users);
    }

    // GET: Create user form
    [AllowAnonymous]
    public IActionResult Create() => View();

    // POST: Create user
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(string email, string password, string role)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
        {
            ViewBag.Error = "All fields are required!";
            return View();
        }

        // Check if role exists
        if (!await _roleManager.RoleExistsAsync(role))
        {
            ViewBag.Error = "Role does not exist!";
            return View();
        }

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            ViewBag.Error = "User already exists!";
            return View();
        }

        // Create user
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            ViewBag.Error = string.Join(", ", result.Errors.Select(e => e.Description));
            return View();
        }

        // Assign role
        await _userManager.AddToRoleAsync(user, role);

        ViewBag.Success = "User created successfully!";
        return RedirectToAction("Login","Account");
    }
}
