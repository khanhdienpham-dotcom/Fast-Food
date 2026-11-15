using FastFood.Models;

namespace FastFood.Helpers
{
    /// <summary>
    /// Trạng thái đơn hàng và quy tắc chuyển trạng thái / tính doanh thu.
    /// </summary>
    public static class OrderStatus
    {
        public const string Preparing = "Đang chuẩn bị";
        public const string Shipping = "Đang giao";
        public const string Delivered = "Đã giao";
        public const string Cancelled = "Đã hủy";

        public static readonly string[] All = { Preparing, Shipping, Delivered, Cancelled };

        /// <summary>Đơn đã giao hoặc đã hủy là trạng thái cuối, không được đổi nữa.</summary>
        public static bool IsFinal(string status) => status == Delivered || status == Cancelled;

        /// <summary>Thứ tự tiến trình: chỉ được đi tới, không được quay lui.</summary>
        private static int Rank(string status) => status switch
        {
            Preparing => 0,
            Shipping => 1,
            Delivered => 2,
            _ => -1
        };

        /// <summary>
        /// Trả về null nếu được phép chuyển, ngược lại trả về câu báo lỗi.
        /// </summary>
        public static string? ValidateTransition(string current, string next)
        {
            if (!All.Contains(next)) return "Trạng thái đơn hàng không hợp lệ!";
            if (current == Delivered)
                return "Đơn hàng đã giao thành công, không thể cập nhật lại trạng thái!";
            if (current == Cancelled)
                return "Đơn hàng đã hủy, không thể cập nhật lại trạng thái!";
            if (next != Cancelled && Rank(next) < Rank(current))
                return $"Đơn hàng đang ở trạng thái \"{current}\", không thể quay lại \"{next}\"!";
            return null;
        }

        /// <summary>Các trạng thái hợp lệ có thể chọn từ trạng thái hiện tại (dùng cho dropdown).</summary>
        public static IEnumerable<string> AllowedNext(string current) =>
            All.Where(s => s == current || ValidateTransition(current, s) == null);

        /// <summary>
        /// Đơn có được tính vào doanh thu không:
        /// - Đơn đã hủy: không tính (đã trừ ra).
        /// - Tiền mặt (COD): chỉ tính khi đã giao thành công (shipper đã thu tiền).
        /// - Chuyển khoản QR / VNPAY / MoMo: khách trả trước lúc đặt nên tính ngay, trừ khi bị hủy.
        /// </summary>
        public static bool IsCountedInRevenue(Order o) =>
            o.Status != Cancelled && (o.PaymentMethod != "COD" || o.Status == Delivered);

        /// <summary>Đơn COD chưa giao xong: tiền còn đang chờ shipper thu.</summary>
        public static bool IsPendingCod(Order o) =>
            o.PaymentMethod == "COD" && (o.Status == Preparing || o.Status == Shipping);
    }
}
