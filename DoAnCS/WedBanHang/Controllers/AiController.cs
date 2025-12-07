using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebBanHang.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly AiChatService _aiService;

        public AiController(AiChatService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
                return BadRequest("Tin nhắn rỗng");

            var reply = await _aiService.GetAiResponse(request.Message);
            return Ok(reply);
        }

        public class ChatRequest
        {
            public string Message { get; set; }
        }
    }
}
