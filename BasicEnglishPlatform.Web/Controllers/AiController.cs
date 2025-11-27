using BasicEnglishPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BasicEnglishPlatform.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] QuestionRequest request)
        {
            var answer = await _aiService.AskAiAsync(request.Question);
            return Ok(new { answer });
        }
    }

    public class QuestionRequest
    {
        public string Question { get; set; }
    }
}
