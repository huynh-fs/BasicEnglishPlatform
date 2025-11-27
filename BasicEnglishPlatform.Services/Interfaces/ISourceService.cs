using BasicEnglishPlatform.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicEnglishPlatform.Services.Interfaces
{
    public interface ICourseService
    {
        Task<List<Course>> GetAllCoursesAsync();
        Task<Course> GetCourseByIdAsync(int id);
        Task CreateCourseAsync(Course course);
        Task UpdateCourseAsync(Course course);
        Task<List<Enrollment>> GetStudentsInCourseAsync(int courseId);
        Task UpdateGradeAsync(int enrollmentId, double grade);
        Task ToggleCourseStatusAsync(int id);
        Task<List<Course>> GetPublishedCoursesAsync();
    }
}
