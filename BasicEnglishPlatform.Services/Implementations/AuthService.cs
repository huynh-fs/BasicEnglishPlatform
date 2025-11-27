using BasicEnglishPlatform.Data.Entities;
using BasicEnglishPlatform.Data.Repositories;
using BasicEnglishPlatform.Services.Helpers;
using BasicEnglishPlatform.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BasicEnglishPlatform.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IStudentRepository _studentRepo;
        private readonly IConfiguration _configuration;

        public AuthService(IStudentRepository studentRepo, IConfiguration configuration)
        {
            _studentRepo = studentRepo;
            _configuration = configuration;
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await _studentRepo.GetByEmailAsync(email);
            if (user == null) return null;

            var inputHash = SecurityHelper.HashPassword(password);
            if (user.PasswordHash != inputHash) return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task RegisterAsync(Student student, string rawPassword)
        {
            if (await _studentRepo.ExistsAsync(student.Email, student.StudentCode))
            {
                throw new Exception("Email hoặc Mã sinh viên đã tồn tại trong hệ thống.");
            }

            student.PasswordHash = SecurityHelper.HashPassword(rawPassword);

            student.Role = "Student";

            await _studentRepo.AddAsync(student);
        }
    }
}
