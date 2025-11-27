using BasicEnglishPlatform.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicEnglishPlatform.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<CourseSchedule> CourseSchedules { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    Id = 1,
                    StudentCode = "ADMIN01",
                    FullName = "System Admin",
                    Email = "admin@school.com",
                    Year = 2020,
                    PasswordHash = "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9",
                    Role = "Admin"
                }
            );
        }
    }
}
