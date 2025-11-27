using BasicEnglishPlatform.Services.Interfaces;
using BasicEnglishPlatform.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BasicEnglishPlatform.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICourseService _courseService;

        public HomeController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            var featuredCourses = courses.Take(6).ToList();

            return View(featuredCourses);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
