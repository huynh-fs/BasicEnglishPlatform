using BasicEnglishPlatform.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicEnglishPlatform.Data.Repositories
{
    public interface ICourseRepository
    {
        Task<List<Course>> GetAllAsync();
        Task<Course> GetByIdAsync(int id);
        Task AddAsync(Course course);
        Task UpdateAsync(Course course);
        Task DeleteAsync(int id);
        Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId);
        Task<Enrollment> GetEnrollmentByIdAsync(int enrollmentId);
        Task UpdateEnrollmentAsync(Enrollment enrollment);
        Task ToggleStatusAsync(int id);
        Task<List<Course>> GetPublishedCoursesAsync();
    }
}
