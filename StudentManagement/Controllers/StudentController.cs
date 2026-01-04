using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using StudentManagement.Application.Services;
using StudentManagement.Core.Entities;
using StudentManagement.Models;

namespace StudentManagement.Controllers;

public class StudentController : Controller
{

    private readonly StudentService _service;
    private readonly IMemoryCache _memoryCache;
    private const string StudentCacheKey = "AllStudentsList";

    public StudentController(StudentService service, IMemoryCache memoryCache)
    {
        _service = service;
        _memoryCache = memoryCache;
    }

    [Authorize(Policy = "StudentDashboard")]
    public async Task<IActionResult> StudentDashboard()
    {
        var aef = User.Identity.Name;
        var chc = HttpContext.User.Identity.IsAuthenticated;

        DashboardData result = new DashboardData();

        if (!_memoryCache.TryGetValue(StudentCacheKey, out DashboardData? studentsCount))
        {
            // 🔹 Data NOT found in cache
            var students = await _service.GetStudentsAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            _memoryCache.Set(StudentCacheKey, students, cacheOptions);

            var courseCount = students.GroupBy(x => x.Course).ToList().Count();

            var das = new DashboardData()
            {
                studentCount = students.Count(),
                courseCount = courseCount

            };
            studentsCount = das;
        }

        return View(studentsCount);
    }

    [Authorize(Policy = "StudentView")]
    public async Task<IActionResult> Index()
    {


        if (!_memoryCache.TryGetValue(StudentCacheKey, out List<Student> students))
        {
            // 🔹 Data NOT found in cache
            students = await _service.GetStudentsAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            _memoryCache.Set(StudentCacheKey, students, cacheOptions);
        }

        return View(students);
    }

    [Authorize(Policy = "StudentCreate")]
    public IActionResult Create() => View();

    [HttpPost]
    [Authorize(Policy = "StudentCreate")]
    public async Task<IActionResult> Create(Student student)
    {
        if (!ModelState.IsValid) return View(student);
        await _service.AddStudentAsync(student);
        _memoryCache.Remove(StudentCacheKey);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "StudentEdit")]
    public async Task<IActionResult> Edit(int id) => View(await _service.GetStudentByIdAsync(id));

    [HttpPost]
    [Authorize(Policy = "StudentEdit")]
    public async Task<IActionResult> Edit(Student student)
    {
        if (!ModelState.IsValid) return View(student);
        await _service.UpdateStudentAsync(student);
        _memoryCache.Remove(StudentCacheKey);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "StudentDelete")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteStudentAsync(id);
        _memoryCache.Remove(StudentCacheKey);
        return RedirectToAction(nameof(Index));
    }
}
