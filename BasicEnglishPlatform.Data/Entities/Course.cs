using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicEnglishPlatform.Data.Entities
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0;

        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public int MaxCapacity { get; set; } = 50;

        public bool IsPublished { get; set; } = true;

        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(3);
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<CourseSchedule> Schedules { get; set; } = new List<CourseSchedule>();

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
