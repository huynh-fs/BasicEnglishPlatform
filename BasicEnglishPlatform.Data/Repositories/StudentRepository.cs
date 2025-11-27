using BasicEnglishPlatform.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicEnglishPlatform.Data.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Student?> GetByEmailAsync(string email)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            return await _context.Students.FindAsync(id);
        }

        public async Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(int studentId)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Schedules)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<int> CountEnrollmentsByCourseIdAsync(int courseId)
        {
            return await _context.Enrollments.CountAsync(e => e.CourseId == courseId);
        }

        public async Task AddEnrollmentAsync(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string email, string studentCode)
        {
            return await _context.Students.AnyAsync(s => s.Email == email || s.StudentCode == studentCode);
        }

        public async Task AddAsync(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            // Chỉ lấy những user có Role là Student
            // Include Enrollments để đếm số khóa học họ đã đăng ký
            return await _context.Students
                .Where(s => s.Role == "Student")
                .Include(s => s.Enrollments)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
        }
    }
}
