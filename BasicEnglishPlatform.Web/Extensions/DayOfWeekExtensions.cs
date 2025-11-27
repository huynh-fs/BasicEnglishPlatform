namespace BasicEnglishPlatform.Web.Extensions
{
    public static class DayOfWeekExtensions
    {
        public static string ToVietnamese(this DayOfWeek day)
        {
            // Nếu là Chủ Nhật (0) -> Trả về "Chủ Nhật"
            // Các thứ khác -> Cộng 1 (Ví dụ: Monday=1 -> Thứ 2)
            return day == DayOfWeek.Sunday ? "Chủ Nhật" : $"Thứ {(int)day + 1}";
        }
    }
}
