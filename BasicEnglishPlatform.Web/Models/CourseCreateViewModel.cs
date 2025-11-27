using System.ComponentModel.DataAnnotations;

namespace BasicEnglishPlatform.Web.Models
{
    public class CourseCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên khóa học")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ảnh bìa")]
        public IFormFile CoverImage { get; set; } // Dùng IFormFile để hứng file upload

        public int MaxCapacity { get; set; } = 30;

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(3);

        // Danh sách lịch học để hứng dữ liệu từ Form
        public List<ScheduleInputModel> Schedules { get; set; } = new List<ScheduleInputModel>();
    }

    public class ScheduleInputModel
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập phòng học hoặc link Zoom")]
        public string Room { get; set; }
    }
}
