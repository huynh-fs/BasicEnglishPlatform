using BasicEnglishPlatform.Data.Entities;
using BasicEnglishPlatform.Data.Repositories;
using BasicEnglishPlatform.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicEnglishPlatform.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepo;

        public CourseService(ICourseRepository courseRepo)
        {
            _courseRepo = courseRepo;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _courseRepo.GetAllAsync();
        }

        public async Task<Course> GetCourseByIdAsync(int id)
        {
            return await _courseRepo.GetByIdAsync(id);
        }

        public async Task CreateCourseAsync(Course course)
        {
            course.CreatedAt = DateTime.Now;
            await _courseRepo.AddAsync(course);
        }

        public async Task UpdateCourseAsync(Course course)
        {
            await _courseRepo.UpdateAsync(course);
        }

        public async Task<List<Enrollment>> GetStudentsInCourseAsync(int courseId)
        {
            return await _courseRepo.GetEnrollmentsByCourseIdAsync(courseId);
        }

        public async Task UpdateGradeAsync(int enrollmentId, double grade)
        {
            if (grade < 0 || grade > 10) throw new Exception("Điểm số phải từ 0 đến 10.");

            var enrollment = await _courseRepo.GetEnrollmentByIdAsync(enrollmentId);
            if (enrollment == null) throw new Exception("Không tìm thấy bản ghi đăng ký.");

            enrollment.Grade = grade;
            await _courseRepo.UpdateEnrollmentAsync(enrollment);
        }

        public async Task ToggleCourseStatusAsync(int id)
        {
            await _courseRepo.ToggleStatusAsync(id);
        }

        public async Task<List<Course>> GetPublishedCoursesAsync()
        {
            return await _courseRepo.GetPublishedCoursesAsync();
        }
    }
}
