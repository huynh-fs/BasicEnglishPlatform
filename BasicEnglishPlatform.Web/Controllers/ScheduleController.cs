using BasicEnglishPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BasicEnglishPlatform.Web.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly IStudentService _studentService;

        public ScheduleController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        // 1. Trả về View chứa cái Lịch
        public IActionResult Index()
        {
            return View();
        }

        // 2. API trả về JSON sự kiện cho FullCalendar
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var myEnrollments = await _studentService.GetMyCoursesAsync(int.Parse(userIdStr));
            var events = new List<object>();

            var notionColors = new[] { "#ff7b7b", "#ffb347", "#fdfd96", "#77dd77", "#aec6cf", "#b39eb5", "#ff6961" };
            int colorIndex = 0;

            foreach (var item in myEnrollments)
            {
                var color = notionColors[colorIndex % notionColors.Length];
                colorIndex++;

                foreach (var schedule in item.Course.Schedules)
                {
                    events.Add(new
                    {
                        title = $"{item.Course.Title}\n({schedule.Room})",

                        // 1. Lặp lại vào thứ mấy
                        daysOfWeek = new[] { (int)schedule.DayOfWeek },

                        // 2. Giờ học trong ngày
                        startTime = schedule.StartTime.ToString(@"hh\:mm"),
                        endTime = schedule.EndTime.ToString(@"hh\:mm"),

                        // 3. GIỚI HẠN THỜI GIAN (QUAN TRỌNG)
                        // FullCalendar yêu cầu định dạng YYYY-MM-DD
                        startRecur = item.Course.StartDate.ToString("yyyy-MM-dd"),

                        // Lưu ý: endRecur của FullCalendar là "Exclusive" (Không bao gồm ngày đó)
                        // Nên ta cộng thêm 1 ngày để hiển thị đúng ngày kết thúc
                        endRecur = item.Course.EndDate.AddDays(1).ToString("yyyy-MM-dd"),

                        // Style & Props
                        backgroundColor = color,
                        borderColor = color,
                        textColor = "#333",
                        extendedProps = new
                        {
                            description = item.Course.Description,
                            room = schedule.Room,
                            dateRange = $"{item.Course.StartDate:dd/MM} - {item.Course.EndDate:dd/MM/yyyy}"
                        }
                    });
                }
            }

            return Json(events);
        }
    }
}