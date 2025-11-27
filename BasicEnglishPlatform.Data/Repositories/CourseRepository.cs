using BasicEnglishPlatform.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicEnglishPlatform.Data.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetAllAsync()
        {
            // Include Schedules để hiển thị lịch học ra ngoài danh sách
            return await _context.Courses
                .Include(c => c.Schedules)
                .Include(c => c.Enrollments)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Course> GetByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Schedules)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Course course)
        {
            var existingCourse = await _context.Courses
                .Include(c => c.Schedules)
                .FirstOrDefaultAsync(c => c.Id == course.Id);

            if (existingCourse != null)
            {
                _context.Entry(existingCourse).CurrentValues.SetValues(course);

                _context.CourseSchedules.RemoveRange(existingCourse.Schedules);

                foreach (var schedule in course.Schedules)
                {
                    existingCourse.Schedules.Add(schedule);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Enrollment>> GetEnrollmentsByCourseIdAsync(int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.Student) // Lấy thông tin sinh viên để hiện tên
                .Include(e => e.Course)
                .Where(e => e.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<Enrollment> GetEnrollmentByIdAsync(int enrollmentId)
        {
            return await _context.Enrollments.FindAsync(enrollmentId);
        }

        public async Task UpdateEnrollmentAsync(Enrollment enrollment)
        {
            _context.Enrollments.Update(enrollment);
            await _context.SaveChangesAsync();
        }

        public async Task ToggleStatusAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                course.IsPublished = !course.IsPublished;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Course>> GetPublishedCoursesAsync()
        {
            return await _context.Courses
                .Include(c => c.Schedules)
                .Include(c => c.Enrollments)
                .Where(c => c.IsPublished == true)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
