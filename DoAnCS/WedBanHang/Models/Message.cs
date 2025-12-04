using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string? SenderId { get; set; }
        public ApplicationUser? Sender { get; set; }

        public string? ReceiverId { get; set; }
        public ApplicationUser? Receiver { get; set; }

        public string Content { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public bool IsFromSupport { get; set; } = false;
        public bool IsRead { get; set; } = false;

        public bool IsAIMessage { get; set; } = false;
        public bool IsUserToAI { get; set; } = false;
    }
}
