using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WebBanHang.Models;
using Microsoft.AspNetCore.Identity;
using WedBanHang.Models;
using Microsoft.EntityFrameworkCore;
using System;

public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AiChatService _aiChatService;

    public ChatHub(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            AiChatService aiChatService)
    {
        _context = context;
        _userManager = userManager;
        _aiChatService = aiChatService;
    }

    public async Task SendMessage(string contextType, string message)
    {
        var senderId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(senderId))
        {
            Console.WriteLine("⚠️ Không xác định được senderId (user chưa đăng nhập?)");
            return;
        }

        var sender = await _userManager.FindByIdAsync(senderId);
        if (sender == null)
        {
            Console.WriteLine("⚠️ Không tìm thấy người gửi.");
            return;
        }

        var isAdmin = await _userManager.IsInRoleAsync(sender, SD.Role_Admin);
        bool isPopupContext = contextType == "popup";

        var adminUsers = await _userManager.GetUsersInRoleAsync(SD.Role_Admin);
        var adminToSave = adminUsers.FirstOrDefault();
        if (adminToSave == null) return;

        // Lưu tin nhắn
        var msg = new Message
        {
            SenderId = senderId,
            ReceiverId = adminToSave.Id,
            Content = message,
            Timestamp = DateTime.Now,
            IsFromSupport = !isPopupContext
        };

      
        string timeFormatted = msg.Timestamp.ToString("HH:mm dd/MM/yyyy");

        if (!isAdmin || isPopupContext)
        {
            // Người gửi là người dùng hoặc admin dùng ChatPopup (giao diện user)
            var adminRecipients = adminUsers.Where(a => a.Id != senderId).ToList();

            foreach (var admin in adminRecipients)
            {
                await Clients.User(admin.Id).SendAsync(
                    "ReceiveMessage",
                    sender.FullName,
                    message,
                    timeFormatted,
                    sender.AvatarUrl,
                    senderId
                );
            }

            // Gửi lại cho chính người gửi với label "Bạn"
            await Clients.User(senderId).SendAsync(
                "ReceiveMessage",
                "Bạn",
                message,
                timeFormatted,
                sender.AvatarUrl,
                senderId
            );
            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();
            await Clients.Group("AdminIndex").SendAsync("UpdateUnread", senderId);
            // Nếu admin đang mở chat của user này → đánh dấu đã đọc ngay
            await Clients.Group("ChatRoom_" + senderId)
                .SendAsync("MarkAsRead", msg.Id);


        }
        else
        {
            // Gửi từ admin panel
            var receiverId = msg.ReceiverId;

            // Gửi đến người dùng: luôn là "Hỗ trợ viên"
            await Clients.User(receiverId).SendAsync(
                "ReceiveMessage",
                "Hỗ trợ viên",
                message,
                timeFormatted,
                "/images/logo.png",
                senderId
            );

            // Gửi lại cho chính admin: vẫn là "Hỗ trợ viên", để hiển thị bên trái ở ChatPopup
            await Clients.User(senderId).SendAsync(
                "ReceiveMessage",
                "Hỗ trợ viên",
                message,
                timeFormatted,
                "/images/logo.png",
                senderId
            );
        }
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("SignalR Connected: " + Context.UserIdentifier);
        await base.OnConnectedAsync();
    }
    public Task JoinAdminIndex()
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, "AdminIndex");
    }
    public Task JoinChatRoom(string userId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, "ChatRoom_" + userId);
    }

    public async Task SendAiMessage(string userMessage)
    {
        var senderId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(senderId)) return;

        // 1. Lưu tin nhắn của người dùng gửi cho AI
        var msgUser = new Message
        {
            SenderId = senderId,
            ReceiverId = null,
            Content = userMessage,
            Timestamp = DateTime.Now,
            IsAIMessage = true,
            IsUserToAI = true
        };
        _context.Messages.Add(msgUser);
        await _context.SaveChangesAsync();

        await Clients.User(senderId).SendAsync(
            "ReceiveMessage",
            "Bạn",
            userMessage,
            msgUser.Timestamp.ToString("HH:mm dd/MM/yyyy"),
            null
        );

        // 2. Gọi AI
        var aiText = await _aiChatService.GetAiResponse(userMessage);

        // 3. Lưu tin nhắn AI trả lời
        var msgAI = new Message
        {
            SenderId = null,
            ReceiverId = senderId,
            Content = aiText,
            Timestamp = DateTime.Now,
            IsAIMessage = true,
            IsUserToAI = false
        };
        _context.Messages.Add(msgAI);
        await _context.SaveChangesAsync();

        // 4. Gửi real-time cho user
        await Clients.User(senderId).SendAsync(
            "ReceiveMessage",
            "AI",
            aiText,
            msgAI.Timestamp.ToString("HH:mm dd/MM/yyyy"),
            "/images/ai.png"
        );
    }

}
