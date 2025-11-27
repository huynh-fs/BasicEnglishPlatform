using BasicEnglishPlatform.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicEnglishPlatform.Services.Interfaces
{
    public interface IStudentService
    {
        Task RegisterCourseAsync(int studentId, int courseId);
        Task<List<Enrollment>> GetMyCoursesAsync(int studentId);
        Task<List<Student>> GetStudentListAsync();
        Task DeleteStudentAsync(int id);
        Task<Student> GetStudentDetailsAsync(int id);
    }
}
