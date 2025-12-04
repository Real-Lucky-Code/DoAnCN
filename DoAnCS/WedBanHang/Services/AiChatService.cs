using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WedBanHang.Models;

public class AiChatService
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _http;
    private readonly string? _apiKey;
    private readonly string _endpoint;
    private readonly string _model;

    public AiChatService(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _context = context;
        _http = httpClientFactory.CreateClient();
        // Read settings from configuration (appsettings.json or secrets)
        _apiKey = config["AI:OpenRouterApiKey"];
        _endpoint = config["AI:Endpoint"] ?? "https://openrouter.ai/api/v1/chat/completions";
        _model = config["AI:Model"] ?? "openai/gpt-3.5-turbo:free";
    }

    public async Task<string> GetAiResponse(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return "AI lỗi: API key chưa được cấu hình trên server.";

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = "Bạn là trợ lý AI của cửa hàng. Trả lời tiếng Việt, thân thiện, ngắn gọn." },
                new { role = "user", content = userMessage }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        try
        {
            var response = await _http.PostAsync(_endpoint, content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"AI lỗi (status {response.StatusCode}): {responseJson}";

            dynamic result = JsonConvert.DeserializeObject(responseJson);
            // Defensive: ensure path exists
            try
            {
                var text = result.choices[0].message.content.ToString();
                return string.IsNullOrWhiteSpace(text) ? "AI trả về nội dung rỗng." : text;
            }
            catch
            {
                return "AI trả về dữ liệu không đúng định dạng.";
            }
        }
        catch (Exception ex)
        {
            return "AI lỗi hệ thống: " + ex.Message;
        }
    }
}
