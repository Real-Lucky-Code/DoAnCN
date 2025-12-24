using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;
using WedBanHang.Models;

namespace WebBanHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly AiChatService _aiService;
        private readonly ApplicationDbContext _context;

        public AiController(AiChatService aiService, ApplicationDbContext context)
        {
            _aiService = aiService;
            _context = context;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest("Tin nhắn rỗng");

            var userId = User?.Identity?.IsAuthenticated == true
                ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                : null;

            if (userId == null)
                return Unauthorized("Vui lòng đăng nhập để chat với AI.");

            var userMessage = new Message
            {
                SenderId = userId,
                ReceiverId = null, // vì AI không có id người dùng thật
                Content = request.Message,
                IsUserToAI = true,
                IsAIMessage = false,
                Timestamp = DateTime.Now
            };
            _context.Messages.Add(userMessage);
            await _context.SaveChangesAsync();

            // ✅ Gọi AI để lấy phản hồi
            var reply = await _aiService.GetAiResponse(request.Message);

            // ✅ Lưu phản hồi của AI vào DB
            var aiMessage = new Message
            {
                SenderId = null, // AI ảo
                ReceiverId = userId,
                Content = reply,
                IsAIMessage = true,
                IsUserToAI = false,
                Timestamp = DateTime.Now
            };
            _context.Messages.Add(aiMessage);
            await _context.SaveChangesAsync();

            // ✅ Trả phản hồi về cho frontend
            return Ok(reply);
        }


        public class ChatRequest
        {
            public string Message { get; set; }
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetAiMessages()
        {
            var userId = User?.Identity?.IsAuthenticated == true
                ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                : null;

            if (userId == null)
                return Unauthorized();

            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId && m.IsUserToAI)
                         || (m.ReceiverId == userId && m.IsAIMessage))
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    sender = m.IsUserToAI ? "Bạn" : "AI",
                    content = m.Content,
                    avatar = m.IsUserToAI
                        ? (m.Sender.AvatarUrl ?? "/images/default-avatar.png")
                        : "/images/logo.png",
                    rawTimestamp = m.Timestamp
                })
                .ToListAsync();

            return Ok(messages);
        }
    }
}
