using BasicEnglishPlatform.Data.Entities;
using BasicEnglishPlatform.Services.Interfaces;
using BasicEnglishPlatform.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicEnglishPlatform.Web.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IWebHostEnvironment _webHostEnvironment; // Để xử lý file upload

        public CourseController(ICourseService courseService, IWebHostEnvironment webHostEnvironment)
        {
            _courseService = courseService;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. Action Index: Dành cho Public/Student (Giao diện Card)
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var courses = await _courseService.GetPublishedCoursesAsync();
            return View(courses);
        }

        // 2. Action Manage: Dành riêng cho Admin (Giao diện Table)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return View(courses);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            await _courseService.ToggleCourseStatusAsync(id);

            TempData["Success"] = "Đã cập nhật trạng thái khóa học.";
            return RedirectToAction(nameof(Manage));
        }


        // 2. Trang tạo mới (Chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            var model = new CourseCreateViewModel();
            // Tạo sẵn 1 dòng lịch học mẫu để form không bị trống
            model.Schedules.Add(new ScheduleInputModel { StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(20, 0, 0) });
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CourseCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // A. Xử lý Upload Ảnh
                string uniqueFileName = null;
                if (model.CoverImage != null)
                {
                    // Tạo thư mục nếu chưa có: wwwroot/images/courses
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "courses");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    // Tạo tên file độc nhất để tránh trùng
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.CoverImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.CoverImage.CopyToAsync(fileStream);
                    }
                }

                // B. Map ViewModel sang Entity
                var course = new Course
                {
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    MaxCapacity = model.MaxCapacity,
                    ImageUrl = uniqueFileName != null ? "/images/courses/" + uniqueFileName : null,
                    IsPublished = true,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    Schedules = model.Schedules.Select(s => new CourseSchedule
                    {
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        Room = s.Room
                    }).ToList()
                };

                // C. Gọi Service lưu vào DB
                await _courseService.CreateCourseAsync(course);

                TempData["Success"] = "Tạo khóa học mới thành công!";

                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "Vui lòng kiểm tra lại thông tin nhập vào.";
            return View(model);
        }

        // 1. Trang giao diện nhập điểm (GET)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Grading(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            ViewData["CourseTitle"] = course.Title;
            ViewData["CourseId"] = course.Id;

            var enrollments = await _courseService.GetStudentsInCourseAsync(id);
            return View(enrollments);
        }

        // 2. API xử lý lưu điểm (POST - gọi bằng AJAX)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateGrade(int enrollmentId, double grade)
        {
            try
            {
                await _courseService.UpdateGradeAsync(enrollmentId, grade);
                return Json(new { success = true, message = "Đã lưu điểm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 1. GET: Hiển thị form sửa
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound();

            // Map Entity -> ViewModel
            var model = new CourseEditViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                MaxCapacity = course.MaxCapacity,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                ExistingImageUrl = course.ImageUrl,
                // Map lịch học
                Schedules = course.Schedules.Select(s => new ScheduleInputModel
                {
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Room = s.Room
                }).ToList()
            };

            return View(model);
        }

        // 2. POST: Xử lý lưu
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(CourseEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                // A. Xử lý ảnh (Nếu có upload ảnh mới)
                string imageUrl = model.ExistingImageUrl; // Mặc định giữ ảnh cũ

                if (model.NewCoverImage != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "courses");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.NewCoverImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.NewCoverImage.CopyToAsync(fileStream);
                    }
                    imageUrl = "/images/courses/" + uniqueFileName;
                }

                // B. Map ViewModel -> Entity
                var courseToUpdate = new Course
                {
                    Id = model.Id,
                    Title = model.Title,
                    Description = model.Description,
                    Price = model.Price,
                    MaxCapacity = model.MaxCapacity,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    ImageUrl = imageUrl, // Ảnh mới hoặc cũ

                    // Map lại lịch học mới
                    Schedules = model.Schedules.Select(s => new CourseSchedule
                    {
                        CourseId = model.Id, // Quan trọng
                        DayOfWeek = s.DayOfWeek,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        Room = s.Room
                    }).ToList()
                };

                // C. Gọi Service
                await _courseService.UpdateCourseAsync(courseToUpdate);

                TempData["Success"] = "Cập nhật khóa học thành công!";

                return RedirectToAction(nameof(Manage));
            }

            TempData["Error"] = "Vui lòng kiểm tra lại thông tin nhập vào.";
            return View(model);
        }

        [AllowAnonymous] // Cho phép khách xem
        public async Task<IActionResult> Details(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound();
            return View(course);
        }
    }
}
