using BasicEnglishPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicEnglishPlatform.Web.Controllers
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới vào được
    public class AdminController : Controller
    {
        private readonly IStudentService _studentService;

        public AdminController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // 1. Danh sách học viên
        public async Task<IActionResult> Students()
        {
            var students = await _studentService.GetStudentListAsync();
            return View(students);
        }

        // 2. Xem chi tiết (Hồ sơ + Các môn đã học)
        public async Task<IActionResult> StudentDetails(int id)
        {
            var student = await _studentService.GetStudentDetailsAsync(id);
            if (student == null) return NotFound();
            return View(student);
        }

        // 3. Xóa học viên
        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            await _studentService.DeleteStudentAsync(id);
            TempData["Success"] = "Đã xóa học viên thành công.";
            return RedirectToAction(nameof(Students));
        }
    }
}