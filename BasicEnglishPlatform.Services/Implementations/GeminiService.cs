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
                    return "Timeout - API phản hồi quá lâm";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error calling Gemini API");
                    return $"Lỗi hệ thống: {ex.Message}";
                }
            }

            return "Lỗi: Không thể kết nối sau 3 lần thử";
        }
    }
}