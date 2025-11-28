using BasicEnglishPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicEnglishPlatform.Web.Controllers
{
    [Authorize] // Chỉ học viên mới được thi
    public class QuizController : Controller
    {
        private readonly IAiService _aiService;

        public QuizController(IAiService aiService)
        {
            _aiService = aiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // API trả về câu hỏi mới (Gọi bằng AJAX)
        [HttpGet]
        public async Task<IActionResult> GetNewQuestion(string topic = "Grammar")
        {
            try
            {
                var quiz = await _aiService.GenerateQuizAsync(topic);
                return Ok(quiz);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi tạo câu hỏi: " + ex.Message);
            }
        }
    }
}
