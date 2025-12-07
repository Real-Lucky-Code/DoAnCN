using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WedBanHang.Models; 

public class AiChatService
{
    private readonly ApplicationDbContext _context;  
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;

    public AiChatService(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _context = context; // ✅ nhận context từ DI
        _http = httpClientFactory.CreateClient();

        _apiKey = config["AI:HuggingFaceToken"]
                    ?? throw new Exception("Chưa cấu hình AI:HuggingFaceToken trong appsettings.");

        _model = config["AI:Model"] ?? "meta-llama/Llama-3.2-3B-Instruct";
    }

    public async Task<string> GetAiResponse(string userMessage)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return "AI lỗi: API key chưa được cấu hình.";

        // 🔹 1. Lấy dữ liệu sản phẩm từ DB
        var products = await _context.Products
            .Select(p => new { p.Name, p.Price, p.Description })
            .Take(50)
            .ToListAsync();

        // 🔹 2. Chuyển dữ liệu sản phẩm thành text
        var productContext = string.Join("\n", products.Select(p =>
            $"- {p.Name}: {p.Price:N0} VNĐ. Mô tả: {p.Description}"
        ));

        // 🔹 3. Prompt cho AI
        var systemPrompt = @$"
            Bạn là trợ lý bán hàng của cửa hàng WebBanHang.
            Chỉ trả lời dựa trên thông tin sản phẩm trong danh sách dưới đây.
            Không bịa đặt, không nói ngoài phạm vi sản phẩm.
            Nếu không tìm thấy sản phẩm phù hợp thì nói: 'Xin lỗi, hiện chưa có sản phẩm phù hợp.'

            Danh sách sản phẩm:
            {productContext}
            ";

        // 🔹 4. Gửi request tới HuggingFace Router
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        try
        {
            var response = await _http.PostAsync("https://router.huggingface.co/v1/chat/completions", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"AI lỗi: {responseJson}";

            dynamic result = JsonConvert.DeserializeObject(responseJson);
            var text = result.choices[0].message.content.ToString();
            return text ?? "AI không trả về nội dung.";
        }
        catch (Exception ex)
        {
            return "AI lỗi hệ thống: " + ex.Message;
        }
    }
}
