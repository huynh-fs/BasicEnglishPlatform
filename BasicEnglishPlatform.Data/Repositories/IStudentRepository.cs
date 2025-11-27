using BasicEnglishPlatform.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicEnglishPlatform.Data.Repositories
{
    public interface IStudentRepository
    {
        Task<Student?> GetByEmailAsync(string email);
        Task<Student?> GetByIdAsync(int id);
        Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId);
        Task<int> CountEnrollmentsByCourseIdAsync(int courseId);
        Task AddEnrollmentAsync(Enrollment enrollment);
        Task<bool> ExistsAsync(string email, string studentCode);
        Task AddAsync(Student student);
        Task<List<Student>> GetAllStudentsAsync();
        Task DeleteAsync(int id);
    }
}
