using BasicEnglishPlatform.Services.DTOs;
using BasicEnglishPlatform.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace BasicEnglishPlatform.Services.Implementations
{
    public class GeminiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GeminiService> _logger;

        public GeminiService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"];
            _logger = logger;
        }

        public async Task<string> AskAiAsync(string question)
        {
            if (string.IsNullOrEmpty(_apiKey))
                return "Chưa cấu hình API Key.";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var payload = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[] { new { text = question } }
                }
            }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            request.Headers.Add("User-Agent", "BasicEnglishPlatform/1.0");
            request.Headers.ConnectionClose = true;

            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    _logger.LogInformation($"Attempt {i + 1}/{maxRetries} - Calling Gemini API");

                    var response = await _httpClient.SendAsync(
                        request,
                        HttpCompletionOption.ResponseContentRead
                    );

                    var responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"API Error {response.StatusCode}: {responseString}");

                        if ((int)response.StatusCode >= 500 && i < maxRetries - 1)
                        {
                            await Task.Delay(1000 * (i + 1)); // Exponential backoff
                            continue;
                        }

                        return $"Lỗi API ({response.StatusCode}): {responseString}";
                    }

                    using var doc = JsonDocument.Parse(responseString);
                    var text = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    return text ?? "AI không trả lời";
                }
                catch (HttpRequestException ex) when (ex.InnerException?.Message.Contains("reset") == true)
                {
                    _logger.LogWarning($"Connection reset on attempt {i + 1}: {ex.Message}");

                    if (i == maxRetries - 1)
                        return "Lỗi kết nối: Server đóng kết nối bất ngờ. Vui lòng thử lại.";

                    await Task.Delay(2000);
                }
                catch (TaskCanceledException)
                {
                    return "Timeout - API phản hồi quá lâu";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error calling Gemini API");
                    return $"Lỗi hệ thống: {ex.Message}";
                }
            }

            return "Lỗi: Không thể kết nối sau 3 lần thử";
        }

        public async Task<AiQuizResponse> GenerateQuizAsync(string topic)
        {
            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("Chưa cấu hình API Key.");

            // 1. Prompt Engineering: Ép AI trả về JSON chuẩn
            string prompt = $@"
        Tạo 1 câu hỏi trắc nghiệm tiếng Anh về chủ đề: {topic}.
        Yêu cầu trả về định dạng JSON thô (Raw JSON), không dùng Markdown (không dùng ```json).
        Cấu trúc:
        {{
            ""question"": ""Câu hỏi tiếng Anh"",
            ""options"": [""A"", ""B"", ""C"", ""D""],
            ""correctAnswer"": ""Đáp án đúng (chép y nguyên từ mảng options)"",
            ""explanation"": ""Giải thích ngắn gọn bằng tiếng Việt""
        }}";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            var payload = new
            {
                contents = new[]
                {
            new { parts = new[] { new { text = prompt } } }
        }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            int maxRetries = 3;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    _logger.LogInformation($"Attempt {i + 1}/{maxRetries} - Generating Quiz");

                    // QUAN TRỌNG: Tạo request mới trong mỗi lần lặp để tránh lỗi "Request already sent"
                    using var request = new HttpRequestMessage(HttpMethod.Post, url);
                    request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                    // Các header giống hệt hàm cũ của bạn
                    request.Headers.Add("User-Agent", "BasicEnglishPlatform/1.0");
                    request.Headers.ConnectionClose = true;

                    var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"API Error {response.StatusCode}: {responseString}");

                        if ((int)response.StatusCode >= 500 && i < maxRetries - 1)
                        {
                            await Task.Delay(1000 * (i + 1)); // Exponential backoff
                            continue;
                        }

                        throw new Exception($"Lỗi API ({response.StatusCode}): {responseString}");
                    }

                    // 2. Parse kết quả từ Gemini
                    using var doc = JsonDocument.Parse(responseString);
                    var text = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    if (string.IsNullOrEmpty(text)) throw new Exception("AI trả về rỗng");

                    // 3. Làm sạch JSON (phòng trường hợp AI vẫn thêm markdown)
                    text = text.Replace("```json", "").Replace("```", "").Trim();

                    // 4. Deserialize sang Object
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<AiQuizResponse>(text, options);
                }
                catch (HttpRequestException ex) when (ex.InnerException?.Message.Contains("reset") == true)
                {
                    _logger.LogWarning($"Connection reset on attempt {i + 1}: {ex.Message}");

                    if (i == maxRetries - 1)
                        throw new Exception("Lỗi kết nối: Server đóng kết nối bất ngờ.");

                    await Task.Delay(2000);
                }
                catch (TaskCanceledException)
                {
                    throw new Exception("Timeout - API phản hồi quá lâu");
                }
                catch (JsonException)
                {
                    // Nếu AI trả về JSON lỗi, thử lại (vì có thể lần sau nó sẽ trả đúng)
                    if (i < maxRetries - 1) continue;
                    throw new Exception("AI trả về định dạng không đúng chuẩn JSON.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error calling Gemini API for Quiz");
                    throw; // Ném lỗi ra để Controller bắt
                }
            }

            throw new Exception("Không thể tạo câu hỏi sau 3 lần thử");
        }
    }
}