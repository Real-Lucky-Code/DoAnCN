using WebBanHang.Models;

namespace WebBanHang.ViewModels
{
    public class UserChatVM
    {
        public ApplicationUser User { get; set; }
        public int UnreadCount { get; set; }
    }

}
