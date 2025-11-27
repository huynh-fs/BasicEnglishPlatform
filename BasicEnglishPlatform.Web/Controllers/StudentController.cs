using BasicEnglishPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BasicEnglishPlatform.Web.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        public async Task<IActionResult> MyCourses()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return RedirectToAction("Login", "Account");

            int studentId = int.Parse(userIdStr);
            var myCourses = await _studentService.GetMyCoursesAsync(studentId);

            return View(myCourses);
        }

        [HttpPost]
        public async Task<IActionResult> Register(int courseId)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

                int studentId = int.Parse(userIdStr);

                await _studentService.RegisterCourseAsync(studentId, courseId);

                TempData["Success"] = "Đăng ký thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Details", "Course", new { id = courseId });
        }
    }
}
