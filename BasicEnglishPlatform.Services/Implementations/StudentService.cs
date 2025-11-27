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
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepo;
        private readonly ICourseRepository _courseRepo;

        public StudentService(IStudentRepository studentRepo, ICourseRepository courseRepo)
        {
            _studentRepo = studentRepo;
            _courseRepo = courseRepo;
        }

        public async Task<List<Enrollment>> GetMyCoursesAsync(int studentId)
        {
            return await _studentRepo.GetEnrollmentsByStudentIdAsync(studentId);
        }

        public async Task RegisterCourseAsync(int studentId, int courseId)
        {
            var courseToRegister = await _courseRepo.GetByIdAsync(courseId);
            if (courseToRegister == null) throw new Exception("Khóa học không tồn tại.");

            var currentCount = await _studentRepo.CountEnrollmentsByCourseIdAsync(courseId);
            if (currentCount >= courseToRegister.MaxCapacity)
            {
                throw new Exception($"Khóa học đã đủ số lượng ({currentCount}/{courseToRegister.MaxCapacity}).");
            }

            var existingEnrollments = await _studentRepo.GetEnrollmentsByStudentIdAsync(studentId);

            if (existingEnrollments.Any(e => e.CourseId == courseId))
            {
                throw new Exception("Bạn đã đăng ký khóa học này rồi.");
            }

            foreach (var newSchedule in courseToRegister.Schedules)
            {
                // Duyệt qua từng môn ĐÃ đăng ký
                foreach (var enrollment in existingEnrollments)
                {
                    var existingCourse = enrollment.Course;

                    // Duyệt qua từng buổi học của môn ĐÃ đăng ký
                    foreach (var existingSchedule in existingCourse.Schedules)
                    {
                        // Nếu cùng Thứ trong tuần
                        if (newSchedule.DayOfWeek == existingSchedule.DayOfWeek)
                        {
                            // Kiểm tra giao nhau về thời gian
                            // Công thức: Max(StartA, StartB) < Min(EndA, EndB)
                            if (newSchedule.StartTime < existingSchedule.EndTime &&
                                existingSchedule.StartTime < newSchedule.EndTime)
                            {
                                throw new Exception($"Trùng lịch vào thứ {newSchedule.DayOfWeek} với môn: {existingCourse.Title}");
                            }
                        }
                    }
                }
            }


            var newEnrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                RegisteredAt = DateTime.Now
            };

            await _studentRepo.AddEnrollmentAsync(newEnrollment);
        }

        public async Task<List<Student>> GetStudentListAsync()
        {
            return await _studentRepo.GetAllStudentsAsync();
        }

        public async Task DeleteStudentAsync(int id)
        {
            // Có thể thêm logic: Nếu sinh viên đang nợ học phí thì không cho xóa (Demo thì xóa luôn)
            await _studentRepo.DeleteAsync(id);
        }

        public async Task<Student> GetStudentDetailsAsync(int id)
        {
            // Tận dụng hàm cũ hoặc viết hàm mới trong Repo nếu cần load sâu hơn
            // Ở đây ta dùng hàm GetByIdAsync của Repo (cần đảm bảo Repo đã Include Enrollments.Course)
            // Để đơn giản, ta gọi lại hàm GetEnrollmentsByStudentIdAsync để lấy list khóa học
            var student = await _studentRepo.GetByIdAsync(id);
            if (student != null)
            {
                student.Enrollments = await _studentRepo.GetEnrollmentsByStudentIdAsync(id);
            }
            return student;
        }
    }
}
