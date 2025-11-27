using System.ComponentModel.DataAnnotations;

namespace BasicEnglishPlatform.Web.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Mã sinh viên")]
        [Display(Name = "Mã Sinh Viên")]
        public string StudentCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Họ tên")]
        [Display(Name = "Họ và Tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Năm nhập học")]
        [Range(2000, 2100, ErrorMessage = "Năm không hợp lệ")]
        public int Year { get; set; } = DateTime.Now.Year;

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại Mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}