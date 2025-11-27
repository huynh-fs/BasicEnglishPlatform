using System.ComponentModel.DataAnnotations;

namespace BasicEnglishPlatform.Web.Models
{
    public class CourseEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khóa học")]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public IFormFile? NewCoverImage { get; set; }

        public string? ExistingImageUrl { get; set; }

        public int MaxCapacity { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public List<ScheduleInputModel> Schedules { get; set; } = new List<ScheduleInputModel>();
    }
}