using WebBanHang.Models;

public static class OrderStatusExtensions
{
    public static string ToFriendlyString(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.ChoXacNhan => "Chờ xác nhận",
            OrderStatus.DangChuanBi => "Đang chuẩn bị hàng",
            OrderStatus.DangVanChuyen => "Đang vận chuyển",
            OrderStatus.DaGiao => "Đã giao",
            OrderStatus.HoanThanh => "Hoàn thành",
            OrderStatus.ChoHuy => "Chờ hủy",
            OrderStatus.DaHuy => "Đã hủy",
            OrderStatus.ChoTra => "Chờ trả hàng",
            OrderStatus.DaTraHang => "Trả hàng",
            _ => "Không xác định"
        };
    }

    public static string ToStatusColor(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.ChoXacNhan => "warning text-dark",
            OrderStatus.DangChuanBi => "info",
            OrderStatus.DangVanChuyen => "primary",
            OrderStatus.DaGiao => "secondary",
            OrderStatus.HoanThanh => "success",
            OrderStatus.ChoHuy => "warning text-dark",
            OrderStatus.DaHuy => "danger",
            OrderStatus.ChoTra => "primary text-dark",
            OrderStatus.DaTraHang => "dark text-white",
            _ => "light"
        };
    }

    public static string GetActionLabel(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.ChoXacNhan => "Xác nhận",
            OrderStatus.DangChuanBi => "Chuẩn bị xong",
            OrderStatus.DangVanChuyen => "Đã giao hàng",
            OrderStatus.DaGiao => "Hoàn tất đơn",
            OrderStatus.ChoHuy => "Xử lý hủy",
            OrderStatus.ChoTra => "Xử lý trả hàng",
            _ => "Tiếp theo"
        };
    }

    public static string ToNotificationMessage(this OrderStatus status, int orderId)
    {
        return status switch
        {
            OrderStatus.ChoXacNhan => $"Đơn hàng #{orderId} đã được tạo. Chờ người bán xác nhận.",
            OrderStatus.DangChuanBi => $"Đơn hàng #{orderId} đã được xác nhận. Người bán đang chuẩn bị hàng.",
            OrderStatus.DangVanChuyen => $"Đơn hàng #{orderId} đang được vận chuyển đến bạn.",
            OrderStatus.DaGiao => $"Đơn hàng #{orderId} đã được giao thành công.",
            OrderStatus.HoanThanh => $"Đơn hàng #{orderId} đã hoàn tất. Cảm ơn bạn đã mua hàng!",
            OrderStatus.ChoHuy => $"Đơn hàng #{orderId} đang được yêu cầu hủy. Chờ admin xử lý.",
            OrderStatus.DaHuy => $"Yêu cầu hủy đơn hàng #{orderId} đã được chấp nhận.",
            OrderStatus.ChoTra => $"Đơn hàng #{orderId} đang chờ xử lý yêu cầu trả hàng.",
            OrderStatus.DaTraHang => $"Đơn hàng #{orderId} đã được chấp nhận hoàn trả thành công. Vui lòng liên hệ với nhân viên để biết thêm chi tiết",
            _ => $"Đơn hàng #{orderId} có cập nhật mới."
        };
    }
}